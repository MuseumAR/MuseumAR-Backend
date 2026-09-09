namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag;

public class TagGroupTranslationDto
{
    public int TagGroupId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string GroupName { get; set; } = null!;
}
