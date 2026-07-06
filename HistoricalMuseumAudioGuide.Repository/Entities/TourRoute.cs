using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TourRoute
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int? EstimatedMinutes { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? AgeGroupId { get; set; }

    public bool IsDefault { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual AgeGroup? AgeGroup { get; set; }

    public virtual Museum Museum { get; set; } = null!;

    public virtual ICollection<TourRouteExhibit> TourRouteExhibits { get; set; } = new List<TourRouteExhibit>();

    public virtual ICollection<TourRouteTranslation> TourRouteTranslations { get; set; } = new List<TourRouteTranslation>();
}
