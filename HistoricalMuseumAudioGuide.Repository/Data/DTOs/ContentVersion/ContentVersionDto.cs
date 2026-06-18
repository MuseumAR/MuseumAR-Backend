using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.ContentVersion;

public class ContentVersionDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public string VersionNumber { get; set; } = null!;
    public string? ChangeDescription { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
