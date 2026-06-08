using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class PackageDownload
{
    public int Id { get; set; }

    public int PackageId { get; set; }

    public int? VisitorId { get; set; }

    public string? DeviceType { get; set; }

    public DateTime DownloadedAt { get; set; }

    public virtual OfflinePackage Package { get; set; } = null!;

    public virtual Visitor? Visitor { get; set; }
}
