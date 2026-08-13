namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;

public class ThemeTranslationDto
{
    public int ThemeId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string ThemeName { get; set; } = null!;
    public string? Description { get; set; }
}
