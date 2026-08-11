namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class UpdateTourRouteDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public int? AgeGroupId { get; set; }
    public int? ExhibitionId { get; set; }
    public bool? IsDefault { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Status { get; set; }
}
