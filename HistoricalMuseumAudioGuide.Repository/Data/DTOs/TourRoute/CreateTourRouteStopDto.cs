namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class CreateTourRouteStopDto
{
    public int ExhibitId { get; set; }
    public int StopOrder { get; set; }
    public int? EstimatedMinutes { get; set; }
}
