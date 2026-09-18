using System;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TicketRefundRequest
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int VisitorId { get; set; }

    public decimal Amount { get; set; }

    public string Reason { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string AccountNumber { get; set; } = null!;

    public string AccountHolderName { get; set; } = null!;

    public string Status { get; set; } = "Pending";

    public string? RejectReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Visitor Visitor { get; set; } = null!;
}
