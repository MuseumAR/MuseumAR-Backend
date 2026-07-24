using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
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

    public PaymentService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;

        string clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID")
                          ?? _configuration["PAYOS_CLIENT_ID"]
                          ?? throw new ArgumentNullException("PAYOS_CLIENT_ID is missing in .env");

        string apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY")
                        ?? _configuration["PAYOS_API_KEY"]
                        ?? throw new ArgumentNullException("PAYOS_API_KEY is missing in .env");

        string checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY")
                             ?? _configuration["PAYOS_CHECKSUM_KEY"]
                             ?? throw new ArgumentNullException("PAYOS_CHECKSUM_KEY is missing in .env");

        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
    }

    private static DateTime GetVietnamTime() => DateTime.UtcNow.AddHours(7);

    // 1. TẠO LINK / MÃ QR THANH TOÁN PAYOS
    public async Task<ResponseModel> CreatePayOSPaymentLinkAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null)
            return ResponseModel.NotFound("Order not found.");

        if (transaction.PaymentStatus == "Completed")
            return ResponseModel.BadRequest("This order has already been paid.");

        // Tạo mã PayOS duy nhất dạng số (yyMMddHHmmss + TransactionId)
        long payOSOrderCode = long.Parse($"{DateTime.UtcNow:yyMMddHHmmss}{transaction.Id}");
        long amount = (long)transaction.TotalAmount;

        string description = $"Ve {transaction.OrderCode}";
        if (description.Length > 25)
        {
            description = description.Substring(0, 25);
        }

        var returnUrl = Environment.GetEnvironmentVariable("PAYOS_RETURN_URL")
                ?? _configuration["PAYOS_RETURN_URL"]
                ?? "http://localhost:7225/payment-success";

        var cancelUrl = Environment.GetEnvironmentVariable("PAYOS_CANCEL_URL")
                        ?? _configuration["PAYOS_CANCEL_URL"]
                        ?? "http://localhost:7225/payment-cancel";

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = payOSOrderCode,
            Amount = amount,
            Description = description,
            Items = new List<PaymentLinkItem>
            {
                new PaymentLinkItem
                {
                    Name = "Ve tham quan bao tang",
                    Quantity = 1,
                    Price = amount
                }
            },
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        try
        {
            var result = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

            // 🟢 Lưu mã PayOS vào DB
            transaction.GatewayTransactionId = payOSOrderCode.ToString();
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
            if (ex.Message.Contains("đã tồn tại") || ex.Message.Contains("already exists"))
            {
                try
                {
                    if (!string.IsNullOrEmpty(transaction.GatewayTransactionId) &&
                        long.TryParse(transaction.GatewayTransactionId, out long oldPayOSOrderCode))
                    {
                        await _payOS.PaymentRequests.CancelAsync(oldPayOSOrderCode, "Tạo lại link thanh toán mới");
                    }

                    long newPayOSOrderCode = long.Parse($"{DateTime.UtcNow.AddSeconds(1):yyMMddHHmmss}{transaction.Id}");
                    paymentRequest.OrderCode = newPayOSOrderCode;

                    var retryResult = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                    transaction.GatewayTransactionId = newPayOSOrderCode.ToString();
                    await _unitOfWork.CompleteAsync();

                    return ResponseModel.Success("Payment link recreated successfully", new
                    {
                        CheckoutUrl = retryResult.CheckoutUrl,
                        QrCode = retryResult.QrCode,
                        OrderCode = transaction.OrderCode,
                        Amount = transaction.TotalAmount
                    });
                }
                catch (Exception retryEx)
                {
                    return ResponseModel.Error($"Failed to recreate PayOS payment link: {retryEx.Message}");
                }
            }

            return ResponseModel.Error($"Failed to create PayOS payment link: {ex.Message}");
        }
    }

    // 2. XỬ LÝ WEBHOOK KHI KHÁCH CHUYỂN KHỎAN THÀNH CÔNG
    public async Task<ResponseModel> ProcessPayOSWebhookAsync(Webhook webhookBody)
    {
        try
        {
            // 🟢 Bước 1: Xác thực Webhook chữ ký bảo mật từ PayOS
            var verifiedData = await _payOS.Webhooks.VerifyAsync(webhookBody);

            if (verifiedData.Code != "00")
            {
                return ResponseModel.BadRequest($"Payment was not successful. PayOS Code: {verifiedData.Code}");
            }

            string payOSOrderCodeStr = verifiedData.OrderCode.ToString();

            // 🟢 Bước 2: Tìm Transaction theo GatewayTransactionId
            var transactions = await _unitOfWork.Transactions
                .FindAsync(t => t.GatewayTransactionId == payOSOrderCodeStr);

            var transaction = transactions.FirstOrDefault();

            // 🟢 NÂNG CẤP: Nếu không tìm thấy theo GatewayTransactionId, tìm dự phòng theo OrderCode trích từ Description
            if (transaction == null && !string.IsNullOrEmpty(verifiedData.Description))
            {
                // Description dạng "Ve ORD202607..." -> Trích xuất lấy mã ORD202607...
                string extractedOrderCode = verifiedData.Description.Replace("Ve ", "").Trim();
                transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(extractedOrderCode);
            }

            // Nếu vẫn null (Trường hợp PayOS bấm nút Test Webhook với OrderCode = 0)
            if (transaction == null)
            {
                Console.WriteLine($"[Webhook Notice] No transaction matched for PayOS OrderCode: {payOSOrderCodeStr}");
                return ResponseModel.Success("Webhook test or order not found, ignored.");
            }

            // 🟢 Bước 3: Kiểm tra số tiền
            if (verifiedData.Amount != (long)transaction.TotalAmount)
            {
                return ResponseModel.BadRequest($"Payment amount mismatch. PayOS: {verifiedData.Amount}, DB: {transaction.TotalAmount}");
            }

            // 🟢 Bước 4: Cập nhật CSDL
            if (transaction.PaymentStatus != "Completed")
            {
                var now = GetVietnamTime();

                transaction.PaymentStatus = "Completed";
                transaction.GatewayTransactionId = payOSOrderCodeStr; // Cập nhật lại cho đồng bộ
                transaction.PaymentDate = now;
                transaction.UpdatedAt = now;

                var tickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(transaction.Id);
                foreach (var ticket in tickets)
                {
                    ticket.Status = "Active";
                    ticket.UpdatedAt = now;
                }

                await _unitOfWork.CompleteAsync();
                Console.WriteLine($"[Webhook Success] Transaction {transaction.OrderCode} updated to Completed!");
            }

            return ResponseModel.Success("Webhook processed successfully");
        }
        catch (Exception ex)
        {
            // Log lỗi chi tiết ra màn hình Output/Console để dễ kiểm tra
            Console.WriteLine($"[Webhook Error] Verify failed: {ex.Message}");
            return ResponseModel.BadRequest($"Webhook verification failed: {ex.Message}");
        }
    }
}