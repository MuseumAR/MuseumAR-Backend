namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class TourRouteStopDto
{
    public int ExhibitId { get; set; }
    public string? ExhibitName { get; set; }
    public string? ExhibitCode { get; set; }
    public int StopOrder { get; set; }
    public int? EstimatedMinutes { get; set; }
    public int? MapId { get; set; }
    public int? FloorNumber { get; set; }
    public double? LocationX { get; set; }
    public double? LocationY { get; set; }
}
