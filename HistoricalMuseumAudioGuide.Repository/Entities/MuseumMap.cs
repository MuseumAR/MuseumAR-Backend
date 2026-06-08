using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class MuseumMap
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int FloorNumber { get; set; }

    public string? MapName { get; set; }

    public string MapImageUrl { get; set; } = null!;

    public int? Width { get; set; }

    public int? Height { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();

    public virtual ICollection<MapPoi> MapPois { get; set; } = new List<MapPoi>();

    public virtual Museum Museum { get; set; } = null!;
}
