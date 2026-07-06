using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Transaction
{
    public int Id { get; set; }

    public int VisitorId { get; set; }

    public int PaymentMethodId { get; set; }

    public string OrderCode { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? GatewayTransactionId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual Visitor Visitor { get; set; } = null!;
}
