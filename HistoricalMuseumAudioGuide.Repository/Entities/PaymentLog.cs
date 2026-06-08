using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class PaymentLog
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string? RawResponse { get; set; }

    public string? LogMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
