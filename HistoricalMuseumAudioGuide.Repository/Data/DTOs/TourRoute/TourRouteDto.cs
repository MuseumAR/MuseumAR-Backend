namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class TourRouteDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public string Name { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
}
