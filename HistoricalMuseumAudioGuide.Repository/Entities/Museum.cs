using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Museum
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Province { get; set; }

    public string? Country { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? OpeningHours { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? Website { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AnalyticsLog> AnalyticsLogs { get; set; } = new List<AnalyticsLog>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<ContentVersion> ContentVersions { get; set; } = new List<ContentVersion>();

    public virtual ICollection<Exhibition> Exhibitions { get; set; } = new List<Exhibition>();

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();

    public virtual ICollection<MuseumLanguage> MuseumLanguages { get; set; } = new List<MuseumLanguage>();

    public virtual ICollection<MuseumMap> MuseumMaps { get; set; } = new List<MuseumMap>();

    public virtual ICollection<MuseumTranslation> MuseumTranslations { get; set; } = new List<MuseumTranslation>();

    public virtual ICollection<OfflinePackage> OfflinePackages { get; set; } = new List<OfflinePackage>();

    public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();

    public virtual ICollection<TourRoute> TourRoutes { get; set; } = new List<TourRoute>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<VisitedExhibit> VisitedExhibits { get; set; } = new List<VisitedExhibit>();
}
