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

using HistoricalMuseumAudioGuide.Service.Services.Email;

using Microsoft.Extensions.DependencyInjection;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public class TicketingService : ITicketingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IPaymentService _paymentService;
    private readonly IServiceProvider _serviceProvider;

    public TicketingService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IPaymentService paymentService, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _paymentService = paymentService;
        _serviceProvider = serviceProvider;
    }

    private void TriggerTicketEmail(int visitorId, int transactionId, string orderCode, decimal totalAmount)
    {
        var scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                var visitor = await unitOfWork.Visitors.GetVisitorWithUserByIdAsync(visitorId);
                string? email = !string.IsNullOrWhiteSpace(visitor?.Email) ? visitor.Email : visitor?.User?.Email;
                if (string.IsNullOrWhiteSpace(email) && visitor?.UserId != null)
                {
                    var user = await unitOfWork.Users.GetByIdAsync(visitor.UserId.Value);
                    email = user?.Email;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine($"[TriggerTicketEmail Warning]: VisitorId {visitorId} does not have a valid email address.");
                    return;
                }

                string visitorName = !string.IsNullOrWhiteSpace(visitor?.User?.FullName)
                    ? visitor.User.FullName
                    : (!string.IsNullOrWhiteSpace(visitor?.DisplayName) ? visitor.DisplayName : "Quý khách");

                var tickets = (await unitOfWork.Tickets.GetTicketsByTransactionIdAsync(transactionId)).ToList();
                int ticketCount = tickets.Count > 0 ? tickets.Count : 1;

                string ticketTypeName = "Vé tham quan";
                var firstTicket = tickets.FirstOrDefault();
                if (firstTicket != null)
                {
                    var type = await unitOfWork.TicketTypes.GetByIdAsync(firstTicket.TicketTypeId);
                    if (type != null) ticketTypeName = type.Name;
                }

                await emailService.SendTicketConfirmationEmailAsync(
                    email,
                    visitorName,
                    orderCode,
                    totalAmount,
                    ticketCount,
                    ticketTypeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TriggerTicketEmail Exception]: {ex.Message}");
            }
        });
    }

    public async Task<ResponseModel> GetTicketTypesAsync(string? lang = null)
    {
        var ticketTypes = await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync();
        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes).ToList();

        // Batch-load active promotions for all ticket types using the repository
        var ticketTypePrices = dtos.ToDictionary(d => d.Id, d => d.Price);
        var activePromotions = (await _unitOfWork.TicketPromotions
            .GetActivePromotionsForTicketTypesAsync(ticketTypePrices.Keys)).ToList();

        var promotionsGrouped = activePromotions
            .GroupBy(p => p.TicketTypeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var dto in dtos)
        {
            dto.OriginalPrice = dto.Price;
            if (promotionsGrouped.TryGetValue(dto.Id, out var promos))
            {
                var promoDtos = _mapper.Map<List<TicketPromotionDto>>(promos);
                
                // Apply language translation for promotions
                if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var promoDto in promoDtos)
                    {
                        if (!string.IsNullOrEmpty(promoDto.NameEn)) promoDto.Name = promoDto.NameEn;
                        if (!string.IsNullOrEmpty(promoDto.DescriptionEn)) promoDto.Description = promoDto.DescriptionEn;
                    }
                }
                dto.ActivePromotions = promoDtos;
            }
        }

        if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrEmpty(dto.NameEn))
                {
                    dto.Name = dto.NameEn;
                }
                if (!string.IsNullOrEmpty(dto.DescriptionEn))
                {
                    dto.Description = dto.DescriptionEn;
                }
            }
        }

        return ResponseModel.Success("Get ticket types successfully", dtos);
    }

    /// <summary>
    /// Tính giá sau khi áp dụng promotion
    /// </summary>
    private static decimal CalculateDiscountedPrice(Repository.Entities.TicketPromotion promo, decimal originalPrice)
    {
        decimal discounted = promo.DiscountType == "Percentage"
            ? originalPrice * (1 - promo.DiscountValue / 100m)
            : originalPrice - promo.DiscountValue;

        return Math.Max(0, Math.Round(discounted, 0)); // Không cho giá âm, làm tròn VND
    }

    private static string ResolveTicketTypeName(string? name, string? nameEn, bool en)
    {
        if (en)
        {
            if (!string.IsNullOrWhiteSpace(nameEn)) return nameEn.Trim();
            return string.IsNullOrWhiteSpace(name) ? "Admission ticket" : name.Trim();
        }
        return string.IsNullOrWhiteSpace(name) ? "Vé tham quan" : name.Trim();
    }

    private static string? ResolveTicketTypeDescription(string? description, string? descriptionEn, bool en)
    {
        if (en)
        {
            if (!string.IsNullOrWhiteSpace(descriptionEn)) return descriptionEn.Trim();
            return description;
        }
        return description;
    }

    public async Task<ResponseModel> GetPendingOrderAsync(int visitorId, string? lang = null)
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
                var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
                ticketTypeName = ResolveTicketTypeName(ticketType.Name, ticketType.NameEn, en);
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

            // Nếu visitor có chọn promotion cụ thể
            if (request.PromotionId.HasValue)
            {
                var selectedPromo = await _unitOfWork.TicketPromotions
                    .GetActivePromotionByIdAndTicketTypeIdAsync(request.PromotionId.Value, request.TicketTypeId);

                if (selectedPromo == null)
                {
                    return ResponseModel.BadRequest("Selected ticket promotion is invalid, paused, or expired.");
                }

                decimal discountedUnitPrice = CalculateDiscountedPrice(selectedPromo, ticketType.Price);
                totalAmount = discountedUnitPrice * request.Quantity;
            }

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
            string ticketRandomGroup = Random.Shared.Next(1000, 9999).ToString();
            for (int i = 0; i < request.Quantity; i++)
            {
                string ticketCode = $"TK-{now:yyMMdd}-{ticketRandomGroup}-{(i + 1):D2}";
                transaction.Tickets.Add(new Ticket
                {
                    VisitorId = visitorId,
                    TicketTypeId = request.TicketTypeId,
                    TicketCode = ticketCode,
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

    public async Task<ResponseModel> GetMyTicketsAsync(int visitorId, string? lang = null)
    {
        var tickets = await _unitOfWork.Tickets.GetTicketsByVisitorIdAsync(visitorId);
        // Only return Active tickets to the user
        var activeTickets = tickets.Where(t => t.Status == "Paid");
        var dtos = _mapper.Map<IEnumerable<TicketDto>>(activeTickets).ToList();
        var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
        if (en)
        {
            foreach (var dto in dtos)
            {
                var match = activeTickets.FirstOrDefault(t => t.Id == dto.Id)?.TicketType;
                dto.TicketTypeName = ResolveTicketTypeName(match?.Name, match?.NameEn, en);
            }
        }
        
        return ResponseModel.Success("Get tickets successfully", dtos);
    }

    public async Task<ResponseModel> GetTicketDetailAsync(int visitorId, int ticketId, string? lang = null)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketDetailByIdAsync(ticketId, visitorId);
        if (ticket == null)
        {
            return ResponseModel.NotFound("Ticket not found.");
        }

        var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
        var code = en ? "en" : "vi";
        var exhibitionName = ticket.TicketType?.Exhibition?.ExhibitionTranslations?
                .FirstOrDefault(t => t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase))?.Name
            ?? ticket.TicketType?.Exhibition?.ExhibitionTranslations?
                .FirstOrDefault(t => t.LanguageCode == "vi")?.Name
            ?? ticket.TicketType?.Exhibition?.ExhibitionTranslations?.FirstOrDefault()?.Name;

        var museum = ticket.TicketType?.Museum;
        var museumTr = museum?.MuseumTranslations?
            .FirstOrDefault(t => t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
        var museumName = !string.IsNullOrWhiteSpace(museumTr?.Name)
            ? museumTr!.Name
            : museum?.Name ?? (en ? "Museum" : "Bảo tàng");
        var museumAddress = !string.IsNullOrWhiteSpace(museumTr?.Address)
            ? museumTr!.Address
            : museum?.Address;

        var ticketTypeName = ResolveTicketTypeName(
            ticket.TicketType?.Name,
            ticket.TicketType?.NameEn,
            en);
        var ticketTypeDesc = ResolveTicketTypeDescription(
            ticket.TicketType?.Description,
            ticket.TicketType?.DescriptionEn,
            en);

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
                Name = ticketTypeName,
                Price = ticket.TicketType?.Price ?? 0,
                Description = ticketTypeDesc
            },
            Museum = new TicketDetailMuseumDto
            {
                Id = museum?.Id ?? ticket.TicketType?.MuseumId ?? 1,
                Name = museumName,
                Address = museumAddress
            },
            Exhibition = ticket.TicketType?.Exhibition != null ? new TicketDetailExhibitionDto
            {
                Id = ticket.TicketType.Exhibition.Id,
                Name = exhibitionName ?? (en
                    ? $"Exhibition #{ticket.TicketType.Exhibition.Id}"
                    : $"Triển lãm #{ticket.TicketType.Exhibition.Id}")
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


    public async Task<ResponseModel> ValidateTicketAsync(string ticketCode)
    {
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return ResponseModel.BadRequest("Ticket code cannot be empty.");
        }

        var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticketCode.Trim());
        if (ticket == null)
        {
            return ResponseModel.Success("Ticket validation completed.", new ValidateTicketResponseDto
            {
                TicketId = 0,
                TicketCode = ticketCode,
                Status = "NotFound",
                IsValid = false,
                Message = "Mã vé không tồn tại trong hệ thống!",
                TicketTypeName = "N/A",
                Price = 0,
                VisitorName = "N/A",
                VisitorEmail = null,
                PurchaseDate = DateTime.MinValue,
                ValidDate = null,
                UsedAt = null
            });
        }

        string visitorName = ticket.Visitor?.User?.FullName ?? ticket.Visitor?.DisplayName ?? "Khách tham quan";
        string? visitorEmail = ticket.Visitor?.Email ?? ticket.Visitor?.User?.Email;
        string ticketTypeName = ticket.TicketType?.Name ?? "Vé tham quan";
        decimal price = ticket.TicketType?.Price ?? 0;

        bool isExpired = ticket.ValidDate.HasValue && DateTime.UtcNow > ticket.ValidDate.Value;
        bool isValid = (ticket.Status == "Paid" || ticket.Status == "Active") && !isExpired;

        string message = isExpired
            ? $"Vé này đã hết hạn sử dụng vào lúc {ticket.ValidDate:dd/MM/yyyy HH:mm}!"
            : ticket.Status switch
            {
                "Paid" or "Active" => "Vé hợp lệ! Có thể thực hiện Check-in.",
                "Used" => $"Vé này đã được Check-in sử dụng trước đó vào lúc {ticket.UpdatedAt:dd/MM/yyyy HH:mm}!",
                "Cancelled" => "Vé này đã bị hủy hoặc hết hạn thanh toán!",
                "Pending" => "Vé này chưa được xác nhận thanh toán!",
                _ => $"Trạng thái vé: {ticket.Status}"
            };

        var responseDto = new ValidateTicketResponseDto
        {
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode,
            Status = ticket.Status,
            IsValid = isValid,
            Message = message,
            TicketTypeName = ticketTypeName,
            Price = price,
            VisitorName = visitorName,
            VisitorEmail = visitorEmail,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            UsedAt = ticket.Status == "Used" ? ticket.UpdatedAt : null
        };

        return ResponseModel.Success("Ticket validated successfully.", responseDto);
    }

    public async Task<ResponseModel> CheckInTicketAsync(string ticketCode)
    {
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return ResponseModel.BadRequest("Ticket code cannot be empty.");
        }

        var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(ticketCode.Trim());
        if (ticket == null)
        {
            return ResponseModel.NotFound("Mã vé không tồn tại trong hệ thống!");
        }

        if (ticket.Status == "Used")
        {
            return ResponseModel.BadRequest($"Vé này đã được check-in sử dụng trước đó vào {ticket.UpdatedAt:dd/MM/yyyy HH:mm}!");
        }

        if (ticket.Status != "Paid" && ticket.Status != "Active")
        {
            return ResponseModel.BadRequest($"Không thể check-in vé có trạng thái '{ticket.Status}'. Vé phải ở trạng thái Đã thanh toán (Paid).");
        }

        if (ticket.ValidDate.HasValue && DateTime.UtcNow > ticket.ValidDate.Value)
        {
            return ResponseModel.BadRequest($"Vé này đã hết hạn sử dụng vào lúc {ticket.ValidDate:dd/MM/yyyy HH:mm}!");
        }

        var now = DateTime.UtcNow.AddHours(7);
        ticket.Status = "Used";
        ticket.UpdatedAt = now;

        await _unitOfWork.CompleteAsync();

        string visitorName = ticket.Visitor?.User?.FullName ?? ticket.Visitor?.DisplayName ?? "Khách tham quan";
        string ticketTypeName = ticket.TicketType?.Name ?? "Vé tham quan";

        var responseDto = new ValidateTicketResponseDto
        {
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode,
            Status = ticket.Status,
            IsValid = true,
            Message = "Check-in thành công! Chúc quý khách có buổi tham quan vui vẻ.",
            TicketTypeName = ticketTypeName,
            Price = ticket.TicketType?.Price ?? 0,
            VisitorName = visitorName,
            VisitorEmail = ticket.Visitor?.Email ?? ticket.Visitor?.User?.Email,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            UsedAt = now
        };

        return ResponseModel.Success("Check-in ticket successfully.", responseDto);
    }
}