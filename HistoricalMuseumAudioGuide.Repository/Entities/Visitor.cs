using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Visitor
{
    public int Id { get; set; }

    public string DeviceId { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string PreferredLang { get; set; } = null!;

    public string? DeviceType { get; set; }

    public string? DeviceModel { get; set; }

    public string? AppVersion { get; set; }

    public DateTime FirstSeenAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public virtual ICollection<AnalyticsLog> AnalyticsLogs { get; set; } = new List<AnalyticsLog>();

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<VisitedExhibit> VisitedExhibits { get; set; } = new List<VisitedExhibit>();
}
