namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;

public class MuseumTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? OpeningHours { get; set; }
}
