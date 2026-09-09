using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;

public class ExhibitScanResultDto
{
    public int ExhibitId { get; set; }
    public string ExhibitCode { get; set; } = null!;
    public string QrcodeData { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? AudioUrl { get; set; }
    public string LanguageCode { get; set; } = "vi";

    public string? CategoryName { get; set; }
    public string? RoomName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? AroverlayUrl { get; set; }
    public string? ArmarkerUrl { get; set; }
    public bool HasArModel { get; set; }

    public List<string> Images { get; set; } = new();
    public List<ArAssetScanDto> ArAssets { get; set; } = new();
}

public class ArAssetScanDto
{
    public int AssetId { get; set; }
    public string AssetType { get; set; } = null!;
    public string AssetUrl { get; set; } = null!;
    public long? FileSizeBytes { get; set; }
    public string? Description { get; set; }
}
