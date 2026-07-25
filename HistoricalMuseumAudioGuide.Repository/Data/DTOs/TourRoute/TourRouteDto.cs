namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class TourRouteDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? AgeGroupId { get; set; }
    public string? AgeGroupName { get; set; }
    public int? ExhibitionId { get; set; }
    public string? ExhibitionName { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TourRouteStopDto> Stops { get; set; } = new();
    public List<TourRouteTranslationDto> Translations { get; set; } = new();
}
