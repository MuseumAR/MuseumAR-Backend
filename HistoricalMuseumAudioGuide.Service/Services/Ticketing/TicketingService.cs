using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
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

    public TicketingService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
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
        if (ticketType == null || !ticketType.IsActive || ticketType.Status != "Approved")
        {
            return ResponseModel.BadRequest("Invalid or inactive ticket type.");
        }

        decimal totalAmount = ticketType.Price * request.Quantity;
        string orderCode = DateTime.UtcNow.Ticks.ToString();

        // 1 represents VNPay payment method in our DB, ideally get it dynamically.
        var transaction = _mapper.Map<Transaction>(request);
        transaction.VisitorId = visitorId;
        transaction.PaymentMethodId = 1; 
        transaction.OrderCode = orderCode;
        transaction.TotalAmount = totalAmount;
        transaction.Currency = "VND";
        transaction.PaymentStatus = "Pending";
        transaction.CreatedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        // Pre-create tickets in Pending state
        for (int i = 0; i < request.Quantity; i++)
        {
            transaction.Tickets.Add(new Ticket
            {
                VisitorId = visitorId,
                TicketTypeId = request.TicketTypeId,
                TicketCode = Guid.NewGuid().ToString("N"),
                PurchaseDate = DateTime.UtcNow,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.CompleteAsync();

        // MOCK FLOW: Instead of VNPay, we return a local mock URL for testing
        string mockPaymentUrl = $"http://localhost:5149/api/ticketing/mock-confirm?orderCode={orderCode}";

        return ResponseModel.Success("Order created (Mock Mode)", new { paymentUrl = mockPaymentUrl, orderCode });
    }

    public async Task<ResponseModel> ConfirmMockPaymentAsync(string orderCode)
    {
        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null) return ResponseModel.NotFound("Order not found");

        if (transaction.PaymentStatus == "Completed") return ResponseModel.Success("Already completed");

        // Simulate success
        transaction.PaymentStatus = "Completed";
        transaction.PaymentDate = DateTime.UtcNow;
        transaction.GatewayTransactionId = "MOCK_GWAY_" + Guid.NewGuid().ToString("N").Substring(0, 8);

        foreach (var ticket in transaction.Tickets)
        {
            ticket.Status = "Paid";
        }

        _unitOfWork.Transactions.Update(transaction);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Mock payment confirmed! Your tickets are now Active.");
    }

    public async Task<ResponseModel> HandleVnPayIpnAsync(IDictionary<string, string> queryParams)
    {
        // Simple validation logic for VNPay IPN (In production, validate secureHash rigorously)
        if (!queryParams.TryGetValue("vnp_TxnRef", out string? orderCode) || 
            !queryParams.TryGetValue("vnp_ResponseCode", out string? responseCode))
        {
            return ResponseModel.BadRequest("Invalid IPN parameters");
        }

        var transaction = await _unitOfWork.Transactions.GetByOrderCodeAsync(orderCode);
        if (transaction == null)
        {
            return ResponseModel.NotFound("Order not found");
        }

        if (transaction.PaymentStatus == "Completed")
        {
            return ResponseModel.Success("Order already completed.");
        }

        if (responseCode == "00")
        {
            // Payment Success
            transaction.PaymentStatus = "Completed";
            transaction.PaymentDate = DateTime.UtcNow;
            transaction.GatewayTransactionId = queryParams.ContainsKey("vnp_TransactionNo") ? queryParams["vnp_TransactionNo"] : null;

            foreach (var ticket in transaction.Tickets)
            {
                ticket.Status = "Paid";
            }
        }
        else
        {
            // Payment Failed
            transaction.PaymentStatus = "Failed";
            foreach (var ticket in transaction.Tickets)
            {
                ticket.Status = "Cancelled";
            }
        }

        transaction.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Transactions.Update(transaction);
        await _unitOfWork.CompleteAsync();

        // Notify via FCM (Mock)
        if (transaction.PaymentStatus == "Completed")
        {
            Console.WriteLine($"[FCM MOCK] Notifying Visitor {transaction.VisitorId}: Tickets are ready for order {orderCode}!");
        }

        return ResponseModel.Success("IPN Handled successfully");
    }

    public async Task<ResponseModel> GetMyTicketsAsync(int visitorId)
    {
        var tickets = await _unitOfWork.Tickets.GetTicketsByVisitorIdAsync(visitorId);
        // Only return Active tickets to the user
        var activeTickets = tickets.Where(t => t.Status == "Paid");
        var dtos = _mapper.Map<IEnumerable<TicketDto>>(activeTickets);
        
        return ResponseModel.Success("Get tickets successfully", dtos);
    }
}
