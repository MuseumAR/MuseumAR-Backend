namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TagTranslation
{
    public int Id { get; set; }

    public int TagId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string TagName { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
