namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;

public class TourRouteTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string RouteName { get; set; } = null!;
    public string? Description { get; set; }
}
