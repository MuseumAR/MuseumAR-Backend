using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class AnalyticsLog
{
    public long Id { get; set; }

    public int? VisitorId { get; set; }

    public int? ExhibitId { get; set; }

    public int MuseumId { get; set; }

    public string ActionType { get; set; } = null!;

    public int? ListeningDuration { get; set; }

    public string? LanguageUsed { get; set; }

    public string? SearchQuery { get; set; }

    public string? DeviceType { get; set; }

    public string? SessionId { get; set; }

    public bool IsOfflineEvent { get; set; }

    public DateTime EventTimestamp { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Exhibit? Exhibit { get; set; }

    public virtual Museum Museum { get; set; } = null!;

    public virtual Visitor? Visitor { get; set; }
}
