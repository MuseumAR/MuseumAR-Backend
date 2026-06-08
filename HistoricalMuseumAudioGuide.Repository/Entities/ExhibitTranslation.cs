using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ExhibitTranslation
{
    public int Id { get; set; }

    public int ExhibitId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? AudioUrl { get; set; }

    public int? AudioDuration { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;
}
