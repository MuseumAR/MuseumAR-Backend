using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class MuseumLanguage
{
    public int MuseumId { get; set; }

    public int LanguageId { get; set; }

    public bool IsDefault { get; set; }

    public virtual Language Language { get; set; } = null!;

    public virtual Museum Museum { get; set; } = null!;
}
