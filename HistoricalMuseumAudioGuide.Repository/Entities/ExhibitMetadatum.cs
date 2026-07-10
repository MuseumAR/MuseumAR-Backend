using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ExhibitMetadatum
{
    public int ExhibitId { get; set; }

    public int? AgeGroupId { get; set; }

    public string? Era { get; set; }

    public string? HistoricalEvent { get; set; }

    public virtual AgeGroup? AgeGroup { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;
}
