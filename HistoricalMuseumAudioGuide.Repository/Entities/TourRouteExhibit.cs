using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TourRouteExhibit
{
    public int Id { get; set; }

    public int TourRouteId { get; set; }

    public int ExhibitId { get; set; }

    public int StopOrder { get; set; }

    public int? EstimatedMinutes { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;

    public virtual TourRoute TourRoute { get; set; } = null!;
}
