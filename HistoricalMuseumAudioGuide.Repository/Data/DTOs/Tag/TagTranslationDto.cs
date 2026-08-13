namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag;

public class TagTranslationDto
{
    public int TagId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string TagName { get; set; } = null!;
}
