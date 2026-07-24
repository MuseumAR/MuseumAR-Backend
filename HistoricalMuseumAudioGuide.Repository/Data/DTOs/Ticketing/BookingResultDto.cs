using System;
using System.Collections.Generic;
using System.Text;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class BookingResultDto
{
    public int TransactionId { get; set; }
    public string OrderCode { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime ValidDate { get; set; }
    public int TotalTickets { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<TicketSummaryDto> Tickets { get; set; } = new();
}

public class TicketSummaryDto
{
    public string TicketCode { get; set; } = null!;
    public string TicketTypeName { get; set; } = null!;
    public decimal Price { get; set; }
}