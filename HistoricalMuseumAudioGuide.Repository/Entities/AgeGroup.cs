using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class AgeGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ExhibitMetadatum> ExhibitMetadata { get; set; } = new List<ExhibitMetadatum>();

    public virtual ICollection<TourRoute> TourRoutes { get; set; } = new List<TourRoute>();
}
