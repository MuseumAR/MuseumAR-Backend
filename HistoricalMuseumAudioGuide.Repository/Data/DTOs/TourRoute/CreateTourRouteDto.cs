namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class CreateTourRouteDto
{
    public int MuseumId { get; set; }
    public string Name { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public int? AgeGroupId { get; set; }
    public int? ExhibitionId { get; set; }
    public bool IsDefault { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<CreateTourRouteStopDto>? Stops { get; set; }
    public List<TourRouteTranslationDto>? Translations { get; set; }
}
