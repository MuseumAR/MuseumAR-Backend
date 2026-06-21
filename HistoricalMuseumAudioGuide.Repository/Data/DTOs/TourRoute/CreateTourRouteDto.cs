namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class CreateTourRouteDto
{
    public int MuseumId { get; set; }
    public string Name { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
}
