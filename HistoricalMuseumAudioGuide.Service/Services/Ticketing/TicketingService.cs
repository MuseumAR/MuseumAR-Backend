using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Payment;
using Microsoft.EntityFrameworkCore;
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
        var ticketTypes = (await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync()).ToList();

        // Tự động ẩn khỏi quầy vé các loại vé dành cho triển lãm đã kết thúc hoặc đóng cửa
        var today = DateTime.UtcNow.AddHours(7).Date;
        ticketTypes = ticketTypes.Where(t =>
            t.Exhibition == null ||
            ((!t.Exhibition.EndDate.HasValue || t.Exhibition.EndDate.Value.Date >= today) &&
             !string.Equals(t.Exhibition.Status, "Ended", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(t.Exhibition.Status, "Closed", StringComparison.OrdinalIgnoreCase))
        ).ToList();

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

        var ticketTypesList = ticketTypes.ToList();
        var ticketTypeMap = ticketTypesList.ToDictionary(t => t.Id);
        bool isEn = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

        foreach (var dto in dtos)
        {
            if (ticketTypeMap.TryGetValue(dto.Id, out var entity) && entity.Exhibition != null)
            {
                var ex = entity.Exhibition;
                var targetLang = isEn ? "en" : "vi";
                var trans = ex.ExhibitionTranslations?.FirstOrDefault(t => string.Equals(t.LanguageCode, targetLang, StringComparison.OrdinalIgnoreCase))
                    ?? ex.ExhibitionTranslations?.FirstOrDefault();

                dto.ExhibitionName = trans?.Name;
                dto.ExhibitionStartDate = ex.StartDate;
                dto.ExhibitionEndDate = ex.EndDate;
                dto.ExhibitionStatus = ex.Status;
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
                _unitOfWork.Transactions.Update(t);
                dbChanged = true;

                var oldTickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(t.Id);
                foreach (var ticket in oldTickets)
                {
                    ticket.Status = "Cancelled";
                    ticket.UpdatedAt = now;
                    _unitOfWork.Tickets.Update(ticket);
                }
            }
            else
            {
                // Kiểm tra trực tiếp với PayOS xem đơn hàng này đã được thanh toán chưa
                try
                {
                    var checkRes = await _paymentService.CheckPaymentStatusAsync(t.OrderCode);
                    if (checkRes.StatusCode == 200 && checkRes.Data != null)
                    {
                        var dataObj = checkRes.Data;
                        var isPaidProp = dataObj.GetType().GetProperty("isPaid");
                        bool isPaid = isPaidProp?.GetValue(dataObj) as bool? ?? false;
                        if (isPaid)
                        {
                            // Đơn đã thanh toán thành công và được CheckPaymentStatusAsync cập nhật xong
                            continue;
                        }

                        var isCancelledProp = dataObj.GetType().GetProperty("isCancelled");
                        bool isCancelled = isCancelledProp?.GetValue(dataObj) as bool? ?? false;
                        if (isCancelled)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GetPendingOrder CheckPayOS Warning]: {ex.Message}");
                }

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

        string? checkoutUrl = pendingTransaction.Description;
        string? qrCode = pendingTransaction.Description;

        // Nếu đơn chưa lưu link thanh toán thì mới gọi CreatePaymentLinkAsync để lấy link
        if (string.IsNullOrEmpty(checkoutUrl))
        {
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
        }

        var dto = new PendingOrderDto
        {
            OrderCode = pendingTransaction.OrderCode,
            TicketTypeId = ticketTypeId,
            TicketTypeName = ticketTypeName,
            Quantity = pendingTickets.Count,
            TotalAmount = pendingTransaction.TotalAmount,
            CheckoutUrl = checkoutUrl,
            QrCode = qrCode ?? checkoutUrl,
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

            if (request.Quantity <= 0 || request.Quantity > 500)
            {
                return ResponseModel.BadRequest("Số lượng vé đặt mua phải từ 1 đến tối đa 500 vé cho mỗi đơn hàng.");
            }

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(request.TicketTypeId);
            if (ticketType == null || !ticketType.IsActive || (ticketType.Status != "Approved" && ticketType.Status != "Active" && !string.IsNullOrEmpty(ticketType.Status)))
            {
                return ResponseModel.BadRequest("Invalid or inactive ticket type.");
            }

            // Nếu là vé triển lãm: kiểm tra triển lãm còn mở và chưa hết hạn
            DateTime? initialValidDate = null;
            if (ticketType.ExhibitionId.HasValue)
            {
                var exhibition = await _unitOfWork.Exhibitions.GetByIdAsync(ticketType.ExhibitionId.Value);
                if (exhibition != null)
                {
                    if (exhibition.EndDate.HasValue && exhibition.EndDate.Value.Date < now.Date)
                    {
                        return ResponseModel.BadRequest("Triển lãm này đã kết thúc, không thể mua vé.");
                    }
                    if (string.Equals(exhibition.Status, "Closed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(exhibition.Status, "Ended", StringComparison.OrdinalIgnoreCase))
                    {
                        return ResponseModel.BadRequest("Triển lãm đã đóng cửa hoặc đã kết thúc, không thể mua vé.");
                    }
                    if (exhibition.EndDate.HasValue)
                    {
                        initialValidDate = exhibition.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                    }
                }
            }

            decimal basePrice = ticketType.Price;
            decimal unitPrice = basePrice;
            int quantity = request.Quantity;
            decimal discountPercent = 0m;
            int focCount = 0;

            // Tiered discount for group orders (>= 30 tickets: disable vouchers, auto apply group discount + FOC)
            if (quantity >= 50)
            {
                discountPercent = 0.10m; // Giảm 10% cho đoàn >= 50 vé
                focCount = quantity / 30; // Cứ 30 vé tặng 1 vé FOC
                unitPrice = Math.Round(basePrice * (1 - discountPercent), 0);
            }
            else if (quantity >= 30)
            {
                discountPercent = 0.08m; // Giảm 8% cho đoàn 30 - 49 vé
                focCount = quantity / 30; // Cứ 30 vé tặng 1 vé FOC
                unitPrice = Math.Round(basePrice * (1 - discountPercent), 0);
            }
            else if (request.PromotionId.HasValue)
            {
                // Single / regular ticket promotion
                var selectedPromo = await _unitOfWork.TicketPromotions
                    .GetActivePromotionByIdAndTicketTypeIdAsync(request.PromotionId.Value, request.TicketTypeId);

                if (selectedPromo == null)
                {
                    return ResponseModel.BadRequest("Selected ticket promotion is invalid, paused, or expired.");
                }

                unitPrice = CalculateDiscountedPrice(selectedPromo, basePrice);
            }

            decimal totalAmount = unitPrice * quantity;

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

            // Pre-create tickets in Pending state with snapshot unit price
            string ticketRandomGroup = Random.Shared.Next(1000, 9999).ToString();
            for (int i = 0; i < quantity; i++)
            {
                string ticketCode = $"TK-{now:yyMMdd}-{ticketRandomGroup}-{(i + 1):D2}";
                transaction.Tickets.Add(new Ticket
                {
                    VisitorId = visitorId,
                    TicketTypeId = request.TicketTypeId,
                    TicketCode = ticketCode,
                    Price = unitPrice,
                    PurchaseDate = now,
                    ValidDate = initialValidDate,
                    Status = "Pending",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            // Create FOC tickets (Free of Charge for leaders/teachers)
            for (int j = 0; j < focCount; j++)
            {
                string focTicketCode = $"TK-{now:yyMMdd}-{ticketRandomGroup}-FOC{(j + 1):D2}";
                transaction.Tickets.Add(new Ticket
                {
                    VisitorId = visitorId,
                    TicketTypeId = request.TicketTypeId,
                    TicketCode = focTicketCode,
                    Price = 0, // FOC = 0 VND
                    PurchaseDate = now,
                    ValidDate = initialValidDate,
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
        // Tự động đồng bộ các đơn chờ thanh toán với PayOS để cập nhật vé mới mua
        try
        {
            await GetPendingOrderAsync(visitorId, lang);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetMyTickets Sync Warning]: {ex.Message}");
        }

        var tickets = await _unitOfWork.Tickets.GetTicketsByVisitorIdAsync(visitorId);
        // Return Paid, Used, Refund_Pending, and Refunded tickets to the user
        var activeTickets = tickets.Where(t => t.Status == "Paid" || t.Status == "Used" || t.Status == "Refund_Pending" || t.Status == "Refunded").ToList();
        var dtos = _mapper.Map<IEnumerable<TicketDto>>(activeTickets).ToList();
        var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

        foreach (var dto in dtos)
        {
            var match = activeTickets.FirstOrDefault(t => t.Id == dto.Id);
            if (en)
            {
                dto.TicketTypeName = ResolveTicketTypeName(match?.TicketType?.Name, match?.TicketType?.NameEn, en);
            }
            if (match != null)
            {
                dto.IsFoc = match.Price == 0;
                dto.OrderCode = match.Transaction?.OrderCode;
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

        decimal purchasedPrice = ticket.Price > 0 ? ticket.Price : (ticket.TicketType?.Price ?? 0);
        bool isFoc = ticket.Price == 0;
        bool isGroupOrder = isFoc || (ticket.TransactionId.HasValue && (await _unitOfWork.Tickets.FindAsync(t => t.TransactionId == ticket.TransactionId.Value)).Count() >= 30);

        var detailDto = new TicketDetailDto
        {
            Id = ticket.Id,
            TicketCode = ticket.TicketCode,
            Price = purchasedPrice,
            Status = ticket.Status,
            IsFoc = isFoc,
            IsGroupOrder = isGroupOrder,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            TicketType = new TicketDetailTypeDto
            {
                Id = ticket.TicketType?.Id ?? 0,
                Name = ticketTypeName,
                Price = purchasedPrice,
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
                TotalAmount = ticket.Transaction?.TotalAmount ?? purchasedPrice,
                Currency = ticket.Transaction?.Currency ?? "VND",
                PaymentStatus = ticket.Transaction?.PaymentStatus ?? (ticket.Status == "Paid" ? "Completed" : ticket.Status),
                PaymentMethod = ticket.Transaction?.PaymentMethod?.Name ?? "PayOS",
                PaidAt = ticket.Transaction?.PaymentDate ?? ticket.PurchaseDate
            },
            QrCodeData = ticket.TicketCode,
            QrCodeImageUrl = null
        };

        var latestRefund = await _unitOfWork.Context.TicketRefundRequests
            .AsNoTracking()
            .Where(r => r.TicketId == ticket.Id)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestRefund != null)
        {
            detailDto.LatestRefundRequest = new TicketDetailRefundRequestDto
            {
                Id = latestRefund.Id,
                Amount = latestRefund.Amount,
                Reason = latestRefund.Reason,
                BankName = latestRefund.BankName,
                AccountNumber = latestRefund.AccountNumber,
                AccountHolderName = latestRefund.AccountHolderName,
                Status = latestRefund.Status,
                RejectReason = latestRefund.RejectReason,
                CreatedAt = latestRefund.CreatedAt,
                ProcessedAt = latestRefund.ProcessedAt
            };
        }

        return ResponseModel.Success("Get ticket detail successfully.", detailDto);
    }


    public async Task<ResponseModel> ValidateTicketAsync(string ticketCode)
    {
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return ResponseModel.BadRequest("Ticket code cannot be empty.");
        }

        var trimmedCode = ticketCode.Trim();

        // Check if the code is an OrderCode (Master QR for Group or Individual Order)
        var orderTransaction = (await _unitOfWork.Transactions.FindAsync(t => t.OrderCode == trimmedCode)).FirstOrDefault();
        if (orderTransaction != null)
        {
            var orderTickets = (await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(orderTransaction.Id)).ToList();
            if (orderTickets.Count == 0)
            {
                return ResponseModel.Success("Order validation completed.", new ValidateTicketResponseDto
                {
                    TicketId = 0,
                    TicketCode = trimmedCode,
                    OrderCode = trimmedCode,
                    IsGroupOrder = true,
                    Status = orderTransaction.PaymentStatus,
                    IsValid = false,
                    Message = "Đơn hàng này chưa có vé hợp lệ trong hệ thống!",
                    TicketTypeName = "Vé tham quan đoàn",
                    Price = orderTransaction.TotalAmount,
                    VisitorName = orderTransaction.Visitor?.User?.FullName ?? orderTransaction.Visitor?.DisplayName ?? "Khách tham quan",
                    PurchaseDate = orderTransaction.CreatedAt
                });
            }

            var sampleTicket = orderTickets.First();
            string groupVisitorName = sampleTicket.Visitor?.User?.FullName ?? sampleTicket.Visitor?.DisplayName ?? orderTransaction.Visitor?.User?.FullName ?? "Khách tham quan";
            string? groupVisitorEmail = sampleTicket.Visitor?.Email ?? sampleTicket.Visitor?.User?.Email ?? orderTransaction.Visitor?.Email;
            string groupTicketTypeName = sampleTicket.TicketType?.Name ?? "Vé tham quan";

            int totalCount = orderTickets.Count;
            int usedCount = orderTickets.Count(t => t.Status == "Used");
            int remainingCount = orderTickets.Count(t => t.Status == "Paid" || t.Status == "Active");
            int focCount = orderTickets.Count(t => t.Price == 0);

            var now = DateTime.UtcNow.AddHours(7);
            var exhibition = sampleTicket.TicketType?.Exhibition;
            bool notStarted = exhibition?.StartDate.HasValue == true && now.Date < exhibition.StartDate.Value.Date;
            bool isExpired = sampleTicket.ValidDate.HasValue && now > sampleTicket.ValidDate.Value;

            bool isValid = remainingCount > 0 && !notStarted && !isExpired && (orderTransaction.PaymentStatus == "Completed" || orderTransaction.PaymentStatus == "Paid" || remainingCount > 0);

            string message = notStarted
                ? $"Triển lãm chưa bắt đầu (Bắt đầu từ ngày {exhibition!.StartDate:dd/MM/yyyy})!"
                : isExpired
                    ? $"Đơn vé đoàn này đã hết hạn sử dụng vào lúc {sampleTicket.ValidDate:dd/MM/yyyy HH:mm}!"
                    : remainingCount == 0 && usedCount > 0
                        ? $"Toàn bộ vé trong đơn ({totalCount} vé) đã được Check-in sử dụng trước đó!"
                        : remainingCount > 0
                            ? $"Đơn vé đoàn hợp lệ! Sẵn sàng Check-in cho {remainingCount}/{totalCount} vé (Bao gồm {focCount} vé FOC)."
                            : $"Trạng thái đơn hàng: {orderTransaction.PaymentStatus}";

            var groupResponseDto = new ValidateTicketResponseDto
            {
                TicketId = sampleTicket.Id,
                TicketCode = trimmedCode,
                OrderCode = orderTransaction.OrderCode,
                IsGroupOrder = totalCount >= 30 || focCount > 0,
                TotalTickets = totalCount,
                UsedTickets = usedCount,
                RemainingTickets = remainingCount,
                FocTickets = focCount,
                Status = remainingCount > 0 ? "Paid" : (usedCount == totalCount ? "Used" : orderTransaction.PaymentStatus),
                IsValid = isValid,
                Message = message,
                TicketTypeName = groupTicketTypeName,
                Price = orderTransaction.TotalAmount,
                VisitorName = groupVisitorName,
                VisitorEmail = groupVisitorEmail,
                PurchaseDate = orderTransaction.CreatedAt,
                ValidDate = sampleTicket.ValidDate,
                UsedAt = usedCount > 0 ? orderTickets.Where(t => t.Status == "Used").Max(t => (DateTime?)t.UpdatedAt) : null
            };

            return ResponseModel.Success("Group order validated successfully.", groupResponseDto);
        }

        var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(trimmedCode);
        if (ticket == null)
        {
            return ResponseModel.Success("Ticket validation completed.", new ValidateTicketResponseDto
            {
                TicketId = 0,
                TicketCode = trimmedCode,
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
        decimal price = ticket.Price > 0 ? ticket.Price : (ticket.TicketType?.Price ?? 0);
        bool isFoc = ticket.Price == 0;

        var nowSingle = DateTime.UtcNow.AddHours(7);
        var singleExhibition = ticket.TicketType?.Exhibition;
        bool notStartedYet = singleExhibition?.StartDate.HasValue == true && nowSingle.Date < singleExhibition.StartDate.Value.Date;
        bool isTicketExpired = ticket.ValidDate.HasValue && nowSingle > ticket.ValidDate.Value;
        bool isTicketValid = (ticket.Status == "Paid" || ticket.Status == "Active") && !isTicketExpired && !notStartedYet;

        string ticketMessage = notStartedYet
            ? $"Triển lãm chưa bắt đầu (Bắt đầu từ ngày {singleExhibition!.StartDate:dd/MM/yyyy})!"
            : isTicketExpired
                ? $"Vé này đã hết hạn sử dụng vào lúc {ticket.ValidDate:dd/MM/yyyy HH:mm}!"
                : ticket.Status switch
                {
                    "Paid" or "Active" => isFoc ? "Vé FOC (Miễn phí dẫn đoàn) hợp lệ! Có thể Check-in." : "Vé hợp lệ! Có thể thực hiện Check-in.",
                    "Used" => $"Vé này đã được Check-in sử dụng trước đó vào lúc {ticket.UpdatedAt:dd/MM/yyyy HH:mm}!",
                    "Refund_Pending" => "Vé này đang trong quá trình yêu cầu hoàn tiền!",
                    "Refunded" => "Vé này đã được hoàn tiền và không còn hiệu lực!",
                    "Cancelled" => "Vé này đã bị hủy hoặc hết hạn thanh toán!",
                    "Pending" => "Vé này chưa được xác nhận thanh toán!",
                    _ => $"Trạng thái vé: {ticket.Status}"
                };

        var responseDto = new ValidateTicketResponseDto
        {
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode,
            OrderCode = ticket.Transaction?.OrderCode,
            Status = ticket.Status,
            IsFoc = isFoc,
            IsValid = isTicketValid,
            Message = ticketMessage,
            TicketTypeName = isFoc ? $"{ticketTypeName} (FOC - Dẫn đoàn)" : ticketTypeName,
            Price = price,
            VisitorName = visitorName,
            VisitorEmail = visitorEmail,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            UsedAt = ticket.Status == "Used" ? ticket.UpdatedAt : null
        };

        return ResponseModel.Success("Ticket validated successfully.", responseDto);
    }

    public async Task<ResponseModel> CheckInTicketAsync(string ticketCode, int? quantity = null)
    {
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return ResponseModel.BadRequest("Ticket code cannot be empty.");
        }

        var trimmedCode = ticketCode.Trim();
        var now = DateTime.UtcNow.AddHours(7);

        // 1. Check if checking in via Master QR (OrderCode)
        var orderTransaction = (await _unitOfWork.Transactions.FindAsync(t => t.OrderCode == trimmedCode)).FirstOrDefault();
        if (orderTransaction != null)
        {
            var orderTickets = (await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(orderTransaction.Id)).ToList();
            // Ưu tiên check-in vé FOC (trưởng đoàn/hướng dẫn viên) trước, sau đó đến vé khách để vé chừa lại luôn là vé khách
            var validTickets = orderTickets
                .Where(t => t.Status == "Paid" || t.Status == "Active")
                .OrderByDescending(t => t.Price == 0)
                .ThenBy(t => t.Id)
                .ToList();

            if (validTickets.Count == 0)
            {
                int usedTicketsCount = orderTickets.Count(t => t.Status == "Used");
                if (usedTicketsCount > 0)
                {
                    return ResponseModel.BadRequest($"Toàn bộ các vé ({usedTicketsCount} vé) trong đơn đoàn này đã được Check-in trước đó!");
                }
                return ResponseModel.BadRequest($"Đơn hàng này không có vé nào ở trạng thái Đã thanh toán (Paid) để Check-in.");
            }

            var sample = validTickets.First();
            var exhibition = sample.TicketType?.Exhibition;
            if (exhibition?.StartDate.HasValue == true && now.Date < exhibition.StartDate.Value.Date)
            {
                return ResponseModel.BadRequest($"Triển lãm chưa bắt đầu (Bắt đầu từ ngày {exhibition.StartDate:dd/MM/yyyy})!");
            }

            if (sample.ValidDate.HasValue && now > sample.ValidDate.Value)
            {
                return ResponseModel.BadRequest($"Đơn vé đoàn này đã hết hạn sử dụng vào lúc {sample.ValidDate:dd/MM/yyyy HH:mm}!");
            }

            // Determine how many tickets to check in (Full or Partial)
            int checkInCount = quantity.HasValue && quantity.Value > 0 && quantity.Value <= validTickets.Count
                ? quantity.Value
                : validTickets.Count;

            var ticketsToCheckIn = validTickets.Take(checkInCount).ToList();

            foreach (var t in ticketsToCheckIn)
            {
                t.Status = "Used";
                t.UpdatedAt = now;
                _unitOfWork.Tickets.Update(t);
            }

            await _unitOfWork.CompleteAsync();

            int totalTickets = orderTickets.Count;
            int usedTickets = orderTickets.Count(t => t.Status == "Used");
            int remainingTickets = orderTickets.Count(t => t.Status == "Paid" || t.Status == "Active");
            int focTickets = orderTickets.Count(t => t.Price == 0);
            string visitorName = sample.Visitor?.User?.FullName ?? sample.Visitor?.DisplayName ?? orderTransaction.Visitor?.DisplayName ?? "Khách tham quan";
            string ticketTypeName = sample.TicketType?.Name ?? "Vé tham quan";

            string checkInMessage = checkInCount < validTickets.Count
                ? $"Check-in trước thành công cho {checkInCount} người! Còn {remainingTickets} vé sẵn sàng cho các thành viên đến sau (quét vé con)."
                : $"Check-in thành công cho toàn bộ {checkInCount} vé! Chúc đoàn có chuyến tham quan ý nghĩa.";

            var groupCheckInResponse = new ValidateTicketResponseDto
            {
                TicketId = sample.Id,
                TicketCode = trimmedCode,
                OrderCode = orderTransaction.OrderCode,
                IsGroupOrder = totalTickets >= 30 || focTickets > 0,
                TotalTickets = totalTickets,
                UsedTickets = usedTickets,
                RemainingTickets = remainingTickets,
                FocTickets = focTickets,
                Status = remainingTickets == 0 ? "Used" : "Paid",
                IsValid = true,
                Message = checkInMessage,
                TicketTypeName = ticketTypeName,
                Price = orderTransaction.TotalAmount,
                VisitorName = visitorName,
                VisitorEmail = sample.Visitor?.Email ?? sample.Visitor?.User?.Email,
                PurchaseDate = orderTransaction.CreatedAt,
                ValidDate = sample.ValidDate,
                UsedAt = now
            };

            return ResponseModel.Success($"Check-in thành công ({checkInCount} vé).", groupCheckInResponse);
        }

        // 2. Check-in single ticket
        var ticket = await _unitOfWork.Tickets.GetTicketByCodeAsync(trimmedCode);
        if (ticket == null)
        {
            return ResponseModel.NotFound("Mã vé không tồn tại trong hệ thống!");
        }

        if (ticket.Status == "Used")
        {
            return ResponseModel.BadRequest($"Vé này đã được check-in sử dụng trước đó vào {ticket.UpdatedAt:dd/MM/yyyy HH:mm}!");
        }

        if (ticket.Status == "Refund_Pending")
        {
            return ResponseModel.BadRequest("Vé này đang trong quá trình yêu cầu hoàn tiền, không thể check-in!");
        }

        if (ticket.Status == "Refunded")
        {
            return ResponseModel.BadRequest("Vé này đã được hoàn tiền, mã vé không còn hiệu lực để vào cổng!");
        }

        if (ticket.Status != "Paid" && ticket.Status != "Active")
        {
            return ResponseModel.BadRequest($"Không thể check-in vé có trạng thái '{ticket.Status}'. Vé phải ở trạng thái Đã thanh toán (Paid).");
        }

        var singleEx = ticket.TicketType?.Exhibition;
        if (singleEx?.StartDate.HasValue == true && now.Date < singleEx.StartDate.Value.Date)
        {
            return ResponseModel.BadRequest($"Triển lãm chưa bắt đầu (Bắt đầu từ ngày {singleEx.StartDate:dd/MM/yyyy})!");
        }

        if (ticket.ValidDate.HasValue && now > ticket.ValidDate.Value)
        {
            return ResponseModel.BadRequest($"Vé này đã hết hạn sử dụng vào lúc {ticket.ValidDate:dd/MM/yyyy HH:mm}!");
        }

        ticket.Status = "Used";
        ticket.UpdatedAt = now;

        await _unitOfWork.CompleteAsync();

        string singleVisitorName = ticket.Visitor?.User?.FullName ?? ticket.Visitor?.DisplayName ?? "Khách tham quan";
        string singleTicketTypeName = ticket.TicketType?.Name ?? "Vé tham quan";
        decimal price = ticket.Price > 0 ? ticket.Price : (ticket.TicketType?.Price ?? 0);
        bool isFocTicket = ticket.Price == 0;

        var singleResponseDto = new ValidateTicketResponseDto
        {
            TicketId = ticket.Id,
            TicketCode = ticket.TicketCode,
            OrderCode = ticket.Transaction?.OrderCode,
            Status = ticket.Status,
            IsFoc = isFocTicket,
            IsValid = true,
            Message = isFocTicket
                ? "Check-in thành công vé FOC (Trưởng đoàn/Giáo viên)! Chúc quý khách có buổi tham quan vui vẻ."
                : "Check-in thành công! Chúc quý khách có buổi tham quan vui vẻ.",
            TicketTypeName = isFocTicket ? $"{singleTicketTypeName} (FOC - Dẫn đoàn)" : singleTicketTypeName,
            Price = price,
            VisitorName = singleVisitorName,
            VisitorEmail = ticket.Visitor?.Email ?? ticket.Visitor?.User?.Email,
            PurchaseDate = ticket.PurchaseDate,
            ValidDate = ticket.ValidDate,
            UsedAt = now
        };

        return ResponseModel.Success("Check-in ticket successfully.", singleResponseDto);
    }

    public async Task<ResponseModel> RequestTicketRefundAsync(int visitorId, int ticketId, CreateTicketRefundRequestDto dto)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
        if (ticket == null || ticket.VisitorId != visitorId)
        {
            return ResponseModel.NotFound("Không tìm thấy vé hoặc bạn không có quyền thao tác trên vé này.");
        }

        if (ticket.Status == "Used")
        {
            return ResponseModel.BadRequest("Vé này đã được check-in sử dụng, không thể yêu cầu hoàn tiền!");
        }

        if (ticket.Status == "Refund_Pending")
        {
            return ResponseModel.BadRequest("Vé này đã có yêu cầu hoàn tiền đang chờ ban quản lý xét duyệt!");
        }

        if (ticket.Status == "Refunded")
        {
            return ResponseModel.BadRequest("Vé này đã được hoàn tiền trước đó!");
        }

        if (ticket.Status != "Paid" && ticket.Status != "Active")
        {
            return ResponseModel.BadRequest($"Chỉ có thể yêu cầu hoàn tiền cho vé đã thanh toán. Trạng thái hiện tại: {ticket.Status}.");
        }

        var now = DateTime.UtcNow.AddHours(7);
        if (ticket.ValidDate.HasValue && now > ticket.ValidDate.Value)
        {
            return ResponseModel.BadRequest("Vé đã hết hạn sử dụng, không thể gửi yêu cầu hoàn tiền!");
        }

        decimal refundAmount = ticket.Price;
        if (refundAmount <= 0)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticket.TicketTypeId);
            refundAmount = ticketType?.Price ?? 0;
        }

        var refundRequest = new TicketRefundRequest
        {
            TicketId = ticket.Id,
            VisitorId = visitorId,
            Amount = refundAmount,
            Reason = dto.Reason.Trim(),
            BankName = dto.BankName.Trim(),
            AccountNumber = dto.AccountNumber.Trim(),
            AccountHolderName = dto.AccountHolderName.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketRefundRequests.AddAsync(refundRequest);

        ticket.Status = "Refund_Pending";
        ticket.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Tickets.Update(ticket);

        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Yêu cầu hoàn vé đã được gửi thành công. Ban quản lý bảo tàng sẽ kiểm tra và hoàn tiền cho bạn sớm nhất có thể!", new
        {
            refundRequestId = refundRequest.Id,
            ticketId = ticket.Id,
            amount = refundAmount,
            status = refundRequest.Status
        });
    }
}