namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;

public class CategoryTranslationDto
{
    public int CategoryId { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}
