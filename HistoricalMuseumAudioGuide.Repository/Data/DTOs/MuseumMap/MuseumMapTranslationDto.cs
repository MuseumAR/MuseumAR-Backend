namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class MuseumMapTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string? MapName { get; set; }
    public string? Description { get; set; }
}
