using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class OfflinePackage
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int VersionId { get; set; }

    public string PackageUrl { get; set; } = null!;

    public long PackageSizeBytes { get; set; }

    public string? Checksum { get; set; }

    public int? AudioCount { get; set; }

    public int? ImageCount { get; set; }

    public int? ArassetCount { get; set; }

    public int? ExhibitCount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? BuiltAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Museum Museum { get; set; } = null!;

    public virtual ICollection<PackageDownload> PackageDownloads { get; set; } = new List<PackageDownload>();

    public virtual ContentVersion Version { get; set; } = null!;
}
