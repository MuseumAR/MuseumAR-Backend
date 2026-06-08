using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Theme
{
    public int Id { get; set; }

    public string ThemeName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ExhibitMetadatum> ExhibitMetadata { get; set; } = new List<ExhibitMetadatum>();

    public virtual ICollection<TourRoute> TourRoutes { get; set; } = new List<TourRoute>();
}
