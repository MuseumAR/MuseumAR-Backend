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

    public async Task<ResponseModel> GetTicketTypesAsync()
    {
        var ticketTypes = await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync();
        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes);
        return ResponseModel.Success("Get ticket types successfully", dtos);
    }

    public async Task<ResponseModel> CreateOrderAsync(int visitorId, CreateOrderRequestDto request)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(request.TicketTypeId);
        if (ticketType == null || !ticketType.IsActive || (ticketType.Status != "Approved" && ticketType.Status != "Active" && !string.IsNullOrEmpty(ticketType.Status)))
        {
            return ResponseModel.BadRequest("Invalid or inactive ticket type.");
        }

        decimal totalAmount = ticketType.Price * request.Quantity;

        var now = DateTime.UtcNow.AddHours(7);
        string orderCode = $"ORD{now:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";

        // 1 represents VNPay payment method in our DB, ideally get it dynamically.
        var transaction = _mapper.Map<Transaction>(request);
        transaction.VisitorId = visitorId;
        transaction.PaymentMethodId = 1;
        transaction.OrderCode = orderCode;
        transaction.TotalAmount = totalAmount;
        transaction.Currency = "VND";
        transaction.PaymentStatus = "Pending";
        transaction.CreatedAt = DateTime.UtcNow.AddHours(7);
        transaction.UpdatedAt = DateTime.UtcNow.AddHours(7);

        // Pre-create tickets in Pending state
        for (int i = 0; i < request.Quantity; i++)
        {
            transaction.Tickets.Add(new Ticket
            {
                VisitorId = visitorId,
                TicketTypeId = request.TicketTypeId,
                TicketCode = Guid.NewGuid().ToString("N"),
                PurchaseDate = DateTime.UtcNow.AddHours(7),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddHours(7),
                UpdatedAt = DateTime.UtcNow.AddHours(7)
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

    public async Task<ResponseModel> GetMyTicketsAsync(int visitorId)
    {
        var tickets = await _unitOfWork.Tickets.GetTicketsByVisitorIdAsync(visitorId);
        // Only return Active tickets to the user
        var activeTickets = tickets.Where(t => t.Status == "Paid");
        var dtos = _mapper.Map<IEnumerable<TicketDto>>(activeTickets);
        
        return ResponseModel.Success("Get tickets successfully", dtos);
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