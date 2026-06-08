using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Ticket
{
    public int Id { get; set; }

    public int VisitorId { get; set; }

    public int TicketTypeId { get; set; }

    public int? TransactionId { get; set; }

    public string TicketCode { get; set; } = null!;

    public DateTime PurchaseDate { get; set; }

    public DateTime? ValidDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TicketType TicketType { get; set; } = null!;

    public virtual Transaction? Transaction { get; set; }

    public virtual Visitor Visitor { get; set; } = null!;
}
