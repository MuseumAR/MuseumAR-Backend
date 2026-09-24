using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class MuseumMapTranslation
{
    public int Id { get; set; }

    public int MapId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? MapName { get; set; }

    public string? Description { get; set; }

    public virtual MuseumMap Map { get; set; } = null!;
}
