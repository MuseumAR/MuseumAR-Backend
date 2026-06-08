using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ExhibitionTranslation
{
    public int Id { get; set; }

    public int ExhibitionId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Exhibition Exhibition { get; set; } = null!;
}
