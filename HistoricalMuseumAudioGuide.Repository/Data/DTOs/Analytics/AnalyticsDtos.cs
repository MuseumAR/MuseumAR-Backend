using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;

public class CreateAnalyticsLogDto
{
    public int MuseumId { get; set; }
    public int? ExhibitId { get; set; }
    public string ActionType { get; set; } = null!; // e.g., "ScanAR", "PlayAudio", "Search"
    public string? LanguageUsed { get; set; }
    public string? DeviceType { get; set; }
    public string? SearchQuery { get; set; }
}

public class DashboardStatsDto
{
    public int TotalExhibits { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalArScans { get; set; }
    public int TotalAudioPlays { get; set; }
}
