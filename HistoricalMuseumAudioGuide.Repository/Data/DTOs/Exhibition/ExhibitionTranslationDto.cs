namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;

public class ExhibitionTranslationDto
{
    public int ExhibitionId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
