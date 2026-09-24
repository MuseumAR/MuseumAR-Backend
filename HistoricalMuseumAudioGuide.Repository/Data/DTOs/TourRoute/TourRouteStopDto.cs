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

    // Bilingual & Media fields
    public string? ExhibitTitleVi { get; set; }
    public string? ExhibitTitleEn { get; set; }
    public string? ExhibitDescriptionVi { get; set; }
    public string? ExhibitDescriptionEn { get; set; }
    public string? RoomNameVi { get; set; }
    public string? RoomNameEn { get; set; }
    public string? AudioUrlVi { get; set; }
    public string? AudioUrlEn { get; set; }
    public string? ThumbnailUrl { get; set; }
    public double? LocationX { get; set; }
    public double? LocationY { get; set; }
}

