using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Exhibition
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<ExhibitionTranslation> ExhibitionTranslations { get; set; } = new List<ExhibitionTranslation>();

    public virtual Museum Museum { get; set; } = null!;

    public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();
}
