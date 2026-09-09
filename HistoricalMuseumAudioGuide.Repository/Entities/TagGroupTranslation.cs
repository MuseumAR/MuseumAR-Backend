namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TagGroupTranslation
{
    public int Id { get; set; }

    public int TagGroupId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string GroupName { get; set; } = null!;

    public virtual TagGroup TagGroup { get; set; } = null!;
}
