namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ThemeTranslation
{
    public int Id { get; set; }

    public int ThemeId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string ThemeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Theme Theme { get; set; } = null!;
}
