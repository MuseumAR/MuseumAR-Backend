using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Payment;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public class TicketingService : ITicketingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IPaymentService _paymentService;

    public TicketingService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IPaymentService paymentService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _paymentService = paymentService;
    }

    public async Task<ResponseModel> GetTicketTypesAsync(string? lang = null)
    {
        var ticketTypes = await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync();
        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes);

        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.NameEn))
                {
                    dto.Name = dto.NameEn;
                }
                else
                {
                    dto.Name = TranslateTicketTypeName(dto.Name);
                }

                if (!string.IsNullOrEmpty(dto.DescriptionEn))
                {
                    dto.Description = dto.DescriptionEn;
                }
                else if (!string.IsNullOrEmpty(dto.Description))
                {
                    dto.Description = TranslateTicketTypeDescription(dto.Description);
                }
            }
        }

        return ResponseModel.Success("Get ticket types successfully", dtos);
    }

    private static string TranslateTicketTypeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        return name.Trim() switch
        {
            "Vé vào cổng phổ thông" => "Standard Admission Ticket",
            "Vé chuyên đề Kháng Chiến đặc biệt" => "Special Resistance War Exhibition Ticket",
            "Vé Học Sinh Hè 2026" => "Summer Student Ticket 2026",
            _ => name
        };
    }

    private static string TranslateTicketTypeDescription(string? desc)
    {
        if (string.IsNullOrWhiteSpace(desc)) return "";
        return desc.Trim() switch
        {
            "Áp dụng tham quan toàn bộ khu vực cố định" => "Access to all permanent exhibition areas",
            "Bao gồm lối đi sảnh chuyên đề và tặng kèm tai nghe" => "Includes special exhibition hall entry and complimentary audio guide headphones",
            "Gia ve uu dai cho hoc sinh trong dip he 2026" => "Discounted price for students during Summer 2026",
            "Giá vé ưu đãi cho học sinh trong dịp hè 2026" => "Discounted price for students during Summer 2026",
            _ => desc
        };
    }

    public async Task<ResponseModel> GetPendingOrderAsync(int visitorId)
    {
        var now = DateTime.UtcNow.AddHours(7);
        var pendingTransactions = (await _unitOfWork.Transactions
            .FindAsync(t => t.VisitorId == visitorId && t.PaymentStatus == "Pending")).ToList();

        bool dbChanged = false;
        var validPending = new List<Transaction>();

        foreach (var t in pendingTransactions)
        {
            var elapsed = (now - t.CreatedAt).TotalSeconds;
            if (elapsed >= 15 * 60 || elapsed < 0)
            {
                t.PaymentStatus = "Cancelled";
                t.UpdatedAt = now;
                dbChanged = true;

                var oldTickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(t.Id);
                foreach (var ticket in oldTickets)
                {
                    ticket.Status = "Cancelled";
                    ticket.UpdatedAt = now;
                }
            }
            else
            {
                validPending.Add(t);
            }
        }

        if (dbChanged)
        {
            await _unitOfWork.CompleteAsync();
        }

        var pendingTransaction = validPending.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
        if (pendingTransaction == null)
        {
            return ResponseModel.Success("No active pending order.", null);
        }

        var elapsedSeconds = (now - pendingTransaction.CreatedAt).TotalSeconds;
        int remainingSeconds = Math.Max(0, (int)(15 * 60 - elapsedSeconds));

        var pendingTickets = (await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(pendingTransaction.Id)).ToList();
        var firstTicket = pendingTickets.FirstOrDefault();
        int ticketTypeId = firstTicket?.TicketTypeId ?? 0;
        string ticketTypeName = "";

        if (ticketTypeId > 0)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType != null)
            {
                ticketTypeName = ticketType.Name;
            }
        }

        string? checkoutUrl = null;
        string? qrCode = null;

        try
        {
            var paymentResponse = await _paymentService.CreatePaymentLinkAsync(pendingTransaction.OrderCode);
            if (paymentResponse.StatusCode == 200 && paymentResponse.Data != null)
            {
                var dataObj = paymentResponse.Data;
                var checkoutProp = dataObj.GetType().GetProperty("CheckoutUrl");
                var qrProp = dataObj.GetType().GetProperty("QrCode");
                checkoutUrl = checkoutProp?.GetValue(dataObj)?.ToString();
                qrCode = qrProp?.GetValue(dataObj)?.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetPendingOrder Warning]: CreatePaymentLink failed: {ex.Message}");
        }

        var dto = new PendingOrderDto
        {
            OrderCode = pendingTransaction.OrderCode,
            TicketTypeId = ticketTypeId,
            TicketTypeName = ticketTypeName,
            Quantity = pendingTickets.Count,
            TotalAmount = pendingTransaction.TotalAmount,
            CheckoutUrl = checkoutUrl,
            QrCode = qrCode,
            CreatedAt = pendingTransaction.CreatedAt,
            ExpiresAt = pendingTransaction.CreatedAt.AddMinutes(15),
            RemainingSeconds = remainingSeconds
        };

        return ResponseModel.Success("Active pending order retrieved successfully.", dto);
    }

    public async Task<ResponseModel> CreateOrderAsync(int visitorId, CreateOrderRequestDto request)
    {
        try
        {
            var now = DateTime.UtcNow.AddHours(7);

            // Check if user already has an active pending transaction (< 15 mins)
            var pendingRes = await GetPendingOrderAsync(visitorId);
            if (pendingRes.StatusCode == 200 && pendingRes.Data is PendingOrderDto existingOrder)
            {
                return ResponseModel.Success("Active pending order already exists", new
                {
                    CheckoutUrl = existingOrder.CheckoutUrl,
                    QrCode = existingOrder.QrCode,
                    OrderCode = existingOrder.OrderCode,
                    Amount = existingOrder.TotalAmount
                });
            }

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(request.TicketTypeId);
            if (ticketType == null || !ticketType.IsActive || (ticketType.Status != "Approved" && ticketType.Status != "Active" && !string.IsNullOrEmpty(ticketType.Status)))
            {
                return ResponseModel.BadRequest("Invalid or inactive ticket type.");
            }

            decimal totalAmount = ticketType.Price * request.Quantity;

            string orderCode = $"ORD{now:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";

            var transaction = new Transaction
            {
                VisitorId = visitorId,
                PaymentMethodId = 1,
                OrderCode = orderCode,
                TotalAmount = totalAmount,
                Currency = "VND",
                PaymentStatus = "Pending",
                CreatedAt = now,
                UpdatedAt = now,
            };

            // Pre-create tickets in Pending state
            for (int i = 0; i < request.Quantity; i++)
            {
                transaction.Tickets.Add(new Ticket
                {
                    VisitorId = visitorId,
                    TicketTypeId = request.TicketTypeId,
                    TicketCode = Guid.NewGuid().ToString("N"),
                    PurchaseDate = now,
                    Status = "Pending",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();

            // Gọi PayOS Service để tạo Link/QR thanh toán thật
            var paymentResponse = await _paymentService.CreatePaymentLinkAsync(orderCode);

            // Nếu tạo link PayOS thất bại, đánh dấu đơn hàng là 'Failed' để tránh đơn rác
            if (paymentResponse.StatusCode != 200)
            {
                transaction.PaymentStatus = "Failed";
                foreach (var ticket in transaction.Tickets)
                {
                    ticket.Status = "Cancelled";
                }
                await _unitOfWork.CompleteAsync();

                return ResponseModel.BadRequest($"Order created, but PayOS link generation failed: {paymentResponse.Message}");
            }

            // Trả về CheckoutUrl & QR Code từ PayOS cho Frontend/App
            return ResponseModel.Success("Order created successfully", paymentResponse.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateOrderAsync Exception]: {ex}");
            return ResponseModel.Error($"Failed to create ticket order: {ex.Message}");
        }
    }

    public async Task<ResponseModel> GetMyTicketsAsync(int visitorId)
    {
        var tickets = await _unitOfWork.Tickets.GetTicketsByVisitorIdAsync(visitorId);
        // Only return Active tickets to the user
        var activeTickets = tickets.Where(t => t.Status == "Paid");
        var dtos = _mapper.Map<IEnumerable<TicketDto>>(activeTickets);
        
        return ResponseModel.Success("Get tickets successfully", dtos);
    }

    public async Task<ResponseModel> GetTicketDetailAsync(int visitorId, int ticketId)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketDetailByIdAsync(ticketId, visitorId);
        if (ticket == null)
        {
            return ResponseModel.NotFound("Ticket not found.");
        }

        var exhibitionName = ticket.TicketType?.Exhibition?.ExhibitionTranslations?.FirstOrDefault(t => t.LanguageCode == "vi")?.Name
            ?? ticket.TicketType?.Exhibition?.ExhibitionTranslations?.FirstOrDefault()?.Name;

        var detailDto = new TicketDetailDto
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            Status = ticket.Status,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            TicketType = new TicketDetailTypeDto
            {
                Id = ticket.TicketType?.Id ?? 0,
                Name = ticket.TicketType?.Name ?? "Vé tham quan",
                Price = ticket.TicketType?.Price ?? 0,
                Description = ticket.TicketType?.Description
            },
            Museum = new TicketDetailMuseumDto
            {
                Id = ticket.TicketType?.Museum?.Id ?? ticket.TicketType?.MuseumId ?? 1,
                Name = ticket.TicketType?.Museum?.Name ?? "Bảo tàng Lịch sử TP.HCM",
                Address = ticket.TicketType?.Museum?.Address ?? "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM"
            },
            Exhibition = ticket.TicketType?.Exhibition != null ? new TicketDetailExhibitionDto
            {
                Id = ticket.TicketType.Exhibition.Id,
                Name = exhibitionName ?? $"Triển lãm #{ticket.TicketType.Exhibition.Id}"
            } : null,
            Order = new TicketDetailOrderDto
            {
                OrderCode = ticket.Transaction?.OrderCode ?? ticket.TicketCode,
                TotalAmount = ticket.Transaction?.TotalAmount ?? ticket.TicketType?.Price ?? 0,
                Currency = ticket.Transaction?.Currency ?? "VND",
                PaymentStatus = ticket.Transaction?.PaymentStatus ?? (ticket.Status == "Paid" ? "Completed" : ticket.Status),
                PaymentMethod = ticket.Transaction?.PaymentMethod?.Name ?? "PayOS",
                PaidAt = ticket.Transaction?.PaymentDate ?? ticket.PurchaseDate
            },
            QrCodeData = ticket.TicketCode,
            QrCodeImageUrl = null
        };

        return ResponseModel.Success("Get ticket detail successfully.", detailDto);
    }

    public async Task<ResponseModel> MockConfirmPaymentAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null) return ResponseModel.NotFound("Order not found.");

        var now = DateTime.UtcNow.AddHours(7);
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
        return ResponseModel.Success("Payment mock-confirmed successfully.");
    }
}