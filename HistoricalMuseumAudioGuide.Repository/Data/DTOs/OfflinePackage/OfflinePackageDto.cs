using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;

public class CreateOfflinePackageDto
{
    public int MuseumId { get; set; }
    public int VersionId { get; set; }
    public int? ExhibitionId { get; set; }
    public string? PackageName { get; set; }
    public string? PackageNameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
}

public class OfflinePackageDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public int VersionId { get; set; }
    public int? ExhibitionId { get; set; }
    public string? ExhibitionTitle { get; set; }
    public string? PackageName { get; set; }
    public string? PackageNameEn { get; set; }
    public string? PackageUrl { get; set; }
    public long? PackageSizeBytes { get; set; }
    public string? Checksum { get; set; }
    public string? Status { get; set; }
    public int? ExhibitCount { get; set; }
    public int? ArassetCount { get; set; }
    public int? ImageCount { get; set; }
    public int? AudioCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OfflinePackageTranslationDto> Translations { get; set; } = new();
}
