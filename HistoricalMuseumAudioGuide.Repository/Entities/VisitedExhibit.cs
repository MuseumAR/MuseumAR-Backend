using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class VisitedExhibit
{
    public int Id { get; set; }

    public int VisitorId { get; set; }

    public int ExhibitId { get; set; }

    public int MuseumId { get; set; }

    public DateTime VisitedAt { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;

    public virtual Museum Museum { get; set; } = null!;

    public virtual Visitor Visitor { get; set; } = null!;
}
