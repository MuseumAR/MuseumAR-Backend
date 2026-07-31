using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Payment;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayOSClient _payOS;
    private readonly IConfiguration _configuration;

    public PaymentService(IUnitOfWork unitOfWork, PayOSClient payOS, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _payOS = payOS;
        _configuration = configuration;
    }

    private static DateTime GetVietnamTime() => DateTime.UtcNow.AddHours(7);

    // =========================================================================
    // 1. TẠO LINK THANH TOÁN
    // =========================================================================
    public async Task<ResponseModel> CreatePaymentLinkAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null)
            return ResponseModel.NotFound("Order not found.");

        if (transaction.PaymentStatus == "Completed")
            return ResponseModel.BadRequest("This order has already been paid.");

        // Mã PayOS số nguyên duy nhất (13 chữ số, an toàn không lo trùng)
        long payOSOrderCode = long.Parse($"{GetVietnamTime():MMddHHmmss}{Random.Shared.Next(100, 999)}");
        long amount = (long)transaction.TotalAmount;

        string description = $"Ve {transaction.OrderCode}";
        if (description.Length > 25) description = description.Substring(0, 25);

        string returnUrl = _configuration["PAYOS_RETURN_URL"] ?? "http://localhost:7225/payment-success";
        string cancelUrl = _configuration["PAYOS_CANCEL_URL"] ?? "http://localhost:7225/payment-cancel";

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = payOSOrderCode,
            Amount = amount,
            Description = description,
            Items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem { Name = "Ve tham quan bao tang", Quantity = 1, Price = amount }
            },
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        try
        {
            // Gọi API PayOS
            var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

            // Chỉ lưu vết GatewayTransactionId vào CSDL khi PayOS đã tạo link thành công
            transaction.GatewayTransactionId = payOSOrderCode.ToString();
            transaction.UpdatedAt = GetVietnamTime();
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Payment link created successfully", new
            {
                CheckoutUrl = result.CheckoutUrl,
                QrCode = result.QrCode,
                OrderCode = transaction.OrderCode,
                Amount = transaction.TotalAmount
            });
        }
        catch (Exception ex)
        {
            return ResponseModel.Error($"PayOS Create Link Error: {ex.Message}");
        }
    }

    // =========================================================================
    // 2. XỬ LÝ WEBHOOK
    // =========================================================================
    public async Task<ResponseModel> ProcessPayOSWebhookAsync(Webhook webhookBody)
    {
        try
        {
            // Verify chữ ký từ PayOS
            WebhookData verifiedData = await _payOS.Webhooks.VerifyAsync(webhookBody);

            if (verifiedData == null || verifiedData.Code != "00")
            {
                return ResponseModel.BadRequest($"Invalid Webhook Data or Code: {verifiedData?.Code}");
            }

            string payOSOrderCodeStr = verifiedData.OrderCode.ToString();

            // Tìm Transaction trong CSDL theo GatewayTransactionId
            var transactions = await _unitOfWork.Transactions
                .FindAsync(t => t.GatewayTransactionId == payOSOrderCodeStr);
            var transaction = transactions.FirstOrDefault();

            // Fallback: Tìm qua Description nếu không khớp GatewayTransactionId
            if (transaction == null && !string.IsNullOrEmpty(verifiedData.Description))
            {
                string extractedCode = verifiedData.Description.Replace("Ve ", "").Trim();
                transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(extractedCode);
            }

            if (transaction == null)
            {
                return ResponseModel.Success("Webhook test or order not found, ignored.");
            }

            // Tránh xử lý lại nếu đơn đã xong
            if (transaction.PaymentStatus == "Completed")
            {
                return ResponseModel.Success("Order already processed.");
            }

            // Cập nhật trạng thái đơn hàng
            var now = GetVietnamTime();
            transaction.PaymentStatus = "Completed";
            transaction.GatewayTransactionId = payOSOrderCodeStr;
            transaction.PaymentDate = now;
            transaction.UpdatedAt = now;

            // Đổi trạng thái vé sang 'Paid'
            var tickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(transaction.Id);
            foreach (var ticket in tickets)
            {
                ticket.Status = "Paid";
                ticket.UpdatedAt = now;
            }

            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Order and tickets updated to 'Paid' successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOK ERROR]: {ex.Message}");
            return ResponseModel.BadRequest($"Webhook failed: {ex.Message}");
        }
    }

    // =========================================================================
    // 3. KIỂM TRA TRẠNG THÁI THANH TOÁN (Auto Polling)
    // =========================================================================
    public async Task<ResponseModel> CheckPaymentStatusAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null)
            return ResponseModel.NotFound("Order not found.");

        if (transaction.PaymentStatus == "Completed")
        {
            return ResponseModel.Success("Payment completed", new { isPaid = true, status = "Completed" });
        }

        if (!string.IsNullOrEmpty(transaction.GatewayTransactionId) && long.TryParse(transaction.GatewayTransactionId, out long payOSOrderCode))
        {
            try
            {
                var paymentInfo = await _payOS.PaymentRequests.GetAsync(payOSOrderCode);
                if (paymentInfo != null && string.Equals(paymentInfo.Status.ToString(), "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    var now = GetVietnamTime();
                    transaction.PaymentStatus = "Completed";
                    transaction.PaymentDate = now;
                    transaction.UpdatedAt = now;

                    var tickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(transaction.Id);
                    foreach (var ticket in tickets)
                    {
                        ticket.Status = "Paid";
                        ticket.UpdatedAt = now;
                    }

                    await _unitOfWork.CompleteAsync();

                    return ResponseModel.Success("Payment completed via PayOS check", new { isPaid = true, status = "Completed" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckPaymentStatus Error]: {ex.Message}");
            }
        }

        return ResponseModel.Success("Payment pending", new { isPaid = false, status = transaction.PaymentStatus });
    }
}