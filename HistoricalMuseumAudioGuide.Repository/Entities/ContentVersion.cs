using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ContentVersion
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public string VersionNumber { get; set; } = null!;

    public string? ChangeDescription { get; set; }

    public int? TotalExhibits { get; set; }

    public int? TotalMediaFiles { get; set; }

    public long? PackageSizeBytes { get; set; }

    public int? PublishedBy { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ContentChangeLog> ContentChangeLogs { get; set; } = new List<ContentChangeLog>();

    public virtual Museum Museum { get; set; } = null!;

    public virtual ICollection<OfflinePackage> OfflinePackages { get; set; } = new List<OfflinePackage>();

    public virtual User? PublishedByNavigation { get; set; }
}
