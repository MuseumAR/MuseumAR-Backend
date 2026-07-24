using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitorEntity = HistoricalMuseumAudioGuide.Repository.Entities.Visitor;

namespace HistoricalMuseumAudioGuide.Service.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private static DateTime GetVietnamTime() => DateTime.UtcNow.AddHours(7);

    // 1. Lấy danh sách Phương thức thanh toán đang hoạt động
    public async Task<ResponseModel> GetActivePaymentMethodsAsync()
    {
        var methods = await _unitOfWork.PaymentMethods.FindAsync(p => p.IsActive);
        return ResponseModel.Success("Get active payment methods successfully", methods);
    }

    // 2. Khởi tạo đơn đặt vé (Transaction & Tickets ở trạng thái Pending)
    public async Task<ResponseModel> CreateBookingAsync(CreateBookingDto dto, int userId)
    {
        // --- A. VALIDATE DỮ LIỆU ĐẦU VÀO ---
        if (dto.Items == null || !dto.Items.Any())
            return ResponseModel.BadRequest("Please select at least one ticket.");

        var now = GetVietnamTime();
        if (dto.ValidDate.Date < now.Date)
            return ResponseModel.BadRequest("Valid date cannot be in the past.");

        // Kiểm tra Phương thức thanh toán
        var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(dto.PaymentMethodId);
        if (paymentMethod == null || !paymentMethod.IsActive)
            return ResponseModel.BadRequest("Selected payment method is invalid or inactive.");

        // --- B. LẤY THÔNG TIN VISITOR THEO USERID TỪ TOKEN ---
        var visitors = await _unitOfWork.Visitors.FindAsync(v => v.UserId == userId);
        var visitor = visitors.FirstOrDefault();

        if (visitor == null)
            return ResponseModel.NotFound("Visitor account not found for this user.");

        visitor.LastSeenAt = now; // Cập nhật mốc hoạt động

        // --- C. VALIDATE LOẠI VÉ & TÍNH TỔNG TIỀN ---
        decimal totalAmount = 0;
        var ticketTypeIds = dto.Items.Select(i => i.TicketTypeId).Distinct().ToList();

        var ticketTypes = (await _unitOfWork.TicketTypes
            .FindAsync(t => ticketTypeIds.Contains(t.Id) && t.IsActive && t.Status == "Approved"))
            .ToDictionary(t => t.Id);

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ResponseModel.BadRequest("Quantity must be greater than 0.");

            if (!ticketTypes.TryGetValue(item.TicketTypeId, out var ticketType))
            {
                return ResponseModel.NotFound($"Ticket type ID {item.TicketTypeId} is invalid or not available.");
            }

            totalAmount += ticketType.Price * item.Quantity;
        }

        // --- D. TẠO TRANSACTIONS (PENDING) ---
        var orderCode = GenerateOrderCode();
        var transaction = new Transaction
        {
            VisitorId = visitor.Id, // 🟢 Đã gán đúng Visitor.Id = 3 vào Transaction!
            PaymentMethodId = dto.PaymentMethodId,
            OrderCode = orderCode,
            TotalAmount = totalAmount,
            Currency = "VND",
            PaymentStatus = "Pending",
            Description = $"Booking ticket for {dto.ValidDate:dd/MM/yyyy}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // --- E. TẠO DANH SÁCH TICKETS (PENDING) ---
        var ticketsToCreate = new List<Ticket>();

        foreach (var item in dto.Items)
        {
            var ticketType = ticketTypes[item.TicketTypeId];

            for (int i = 0; i < item.Quantity; i++)
            {
                ticketsToCreate.Add(new Ticket
                {
                    VisitorId = visitor.Id, // 🟢 Gán đúng Visitor.Id = 3
                    TicketTypeId = item.TicketTypeId,
                    Transaction = transaction,
                    TicketCode = GenerateTicketCode(),
                    PurchaseDate = now,
                    ValidDate = dto.ValidDate,
                    Status = "Pending",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        // --- F. LƯU VÀO DATABASE ---
        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.Tickets.AddRangeAsync(ticketsToCreate);
        await _unitOfWork.CompleteAsync();

        // --- G. TRẢ VỀ DTO KẾT QUẢ ---
        var result = new BookingResultDto
        {
            TransactionId = transaction.Id,
            OrderCode = transaction.OrderCode,
            TotalAmount = transaction.TotalAmount,
            Currency = transaction.Currency,
            PaymentStatus = transaction.PaymentStatus,
            ValidDate = dto.ValidDate,
            TotalTickets = ticketsToCreate.Count,
            CreatedAt = now,
            Tickets = ticketsToCreate.Select(t => new TicketSummaryDto
            {
                TicketCode = t.TicketCode,
                TicketTypeName = ticketTypes[t.TicketTypeId].Name,
                Price = ticketTypes[t.TicketTypeId].Price
            }).ToList()
        };

        return ResponseModel.Success("Booking created successfully. Ready for payment.", result);
    }

    // 3. Tra cứu thông tin đơn hàng theo OrderCode
    public async Task<ResponseModel> GetBookingByOrderCodeAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null)
            return ResponseModel.NotFound("Order not found.");

        var tickets = await _unitOfWork.Tickets.GetTicketsByTransactionIdAsync(transaction.Id);

        var result = new BookingResultDto
        {
            TransactionId = transaction.Id,
            OrderCode = transaction.OrderCode,
            TotalAmount = transaction.TotalAmount,
            Currency = transaction.Currency,
            PaymentStatus = transaction.PaymentStatus,
            ValidDate = tickets.FirstOrDefault()?.ValidDate ?? transaction.CreatedAt,
            TotalTickets = tickets.Count(),
            CreatedAt = transaction.CreatedAt,
            Tickets = tickets.Select(t => new TicketSummaryDto
            {
                TicketCode = t.TicketCode,
                TicketTypeName = t.TicketType?.Name ?? "Unknown",
                Price = t.TicketType?.Price ?? 0
            }).ToList()
        };

        return ResponseModel.Success("Get order details successfully", result);
    }

    private string GenerateOrderCode()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
    }

    private string GenerateTicketCode()
    {
        return $"TK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}