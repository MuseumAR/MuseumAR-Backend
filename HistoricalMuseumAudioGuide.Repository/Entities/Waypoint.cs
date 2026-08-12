using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Waypoint
{
    public string Id { get; set; } = null!;

    public int MuseumId { get; set; }

    public int? MapId { get; set; }

    public int FloorNumber { get; set; } = 1;

    public double X { get; set; }

    public double Y { get; set; }

    public string Type { get; set; } = "HALLWAY";

    public int? RoomId { get; set; }

    public string? Code { get; set; }

    public string? Label { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Museum Museum { get; set; } = null!;

    public virtual MuseumMap? Map { get; set; }

    public virtual Room? Room { get; set; }
}
