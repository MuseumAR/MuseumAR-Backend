using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TourRouteTranslation
{
    public int Id { get; set; }

    public int TourRouteId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string RouteName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual TourRoute TourRoute { get; set; } = null!;
}
