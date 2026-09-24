using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class ValidateTicketResponseDto
{
    public int TicketId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsValid { get; set; }
    public string Message { get; set; } = null!;

    public string TicketTypeName { get; set; } = null!;
    public decimal Price { get; set; }
    public string VisitorName { get; set; } = null!;
    public string? VisitorEmail { get; set; }

    public DateTime PurchaseDate { get; set; }
    public DateTime? ValidDate { get; set; }
    public DateTime? UsedAt { get; set; }

    // Group & Hybrid check-in support
    public bool IsGroupOrder { get; set; }
    public bool IsFoc { get; set; }
    public string? OrderCode { get; set; }
    public int TotalTickets { get; set; }
    public int UsedTickets { get; set; }
    public int RemainingTickets { get; set; }
    public int FocTickets { get; set; }
}
