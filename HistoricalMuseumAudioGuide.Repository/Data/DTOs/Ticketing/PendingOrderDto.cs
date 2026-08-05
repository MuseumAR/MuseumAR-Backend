using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class PendingOrderDto
{
    public string OrderCode { get; set; } = null!;
    public int TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int RemainingSeconds { get; set; }
}
