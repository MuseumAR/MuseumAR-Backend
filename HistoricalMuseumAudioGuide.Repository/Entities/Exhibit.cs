using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Exhibit
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int? CategoryId { get; set; }

    public string? ExhibitCode { get; set; }

    public string? QrcodeData { get; set; }

    public string? QrcodeImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? AroverlayUrl { get; set; }

    public string? ArmarkerUrl { get; set; }

    public int? MapId { get; set; }

    public double? LocationX { get; set; }

    public double? LocationY { get; set; }

    public int SortOrder { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AnalyticsLog> AnalyticsLogs { get; set; } = new List<AnalyticsLog>();

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual Category? Category { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<ExhibitArasset> ExhibitArassets { get; set; } = new List<ExhibitArasset>();

    public virtual ICollection<ExhibitImage> ExhibitImages { get; set; } = new List<ExhibitImage>();

    public virtual ExhibitMetadatum? ExhibitMetadatum { get; set; }

    public virtual ICollection<ExhibitTranslation> ExhibitTranslations { get; set; } = new List<ExhibitTranslation>();

    public virtual MuseumMap? Map { get; set; }

    public virtual Museum Museum { get; set; } = null!;

    public virtual ICollection<TourRouteExhibit> TourRouteExhibits { get; set; } = new List<TourRouteExhibit>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<VisitedExhibit> VisitedExhibits { get; set; } = new List<VisitedExhibit>();

    public virtual ICollection<Exhibition> Exhibitions { get; set; } = new List<Exhibition>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
