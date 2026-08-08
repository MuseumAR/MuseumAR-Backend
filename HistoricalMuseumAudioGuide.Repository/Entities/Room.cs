using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Room
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int? MapId { get; set; }

    public string RoomCode { get; set; } = null!;

    public string RoomName { get; set; } = null!;

    public int FloorNumber { get; set; } = 1;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int? DoorWaypointId { get; set; }

    public string? WaypointId { get; set; }

    public double? CenterX { get; set; }

    public double? CenterY { get; set; }

    public virtual Museum Museum { get; set; } = null!;

    public virtual MuseumMap? Map { get; set; }

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();
}
