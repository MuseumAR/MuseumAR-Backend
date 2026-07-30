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
    public int? RoomId { get; set; }
    public string? RoomCode { get; set; }
    public string? RoomName { get; set; }
}
