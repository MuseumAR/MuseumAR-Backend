using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;

public class ExhibitArassetDto
{
    public int Id { get; set; }
    public int ExhibitId { get; set; }
    public string? AssetUrl { get; set; }
    public string? AssetType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
