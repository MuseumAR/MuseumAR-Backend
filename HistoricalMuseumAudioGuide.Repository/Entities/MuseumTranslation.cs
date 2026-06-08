using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class MuseumTranslation
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? OpeningHours { get; set; }

    public virtual Museum Museum { get; set; } = null!;
}
