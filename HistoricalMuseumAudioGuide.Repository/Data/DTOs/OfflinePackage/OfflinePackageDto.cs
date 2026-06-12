using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;

public class CreateOfflinePackageDto
{
    public int MuseumId { get; set; }
    public int VersionId { get; set; }
}

public class OfflinePackageDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public int VersionId { get; set; }
    public string? PackageUrl { get; set; }
    public string? Checksum { get; set; }
    public string? Status { get; set; }
    public int? ArassetCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
