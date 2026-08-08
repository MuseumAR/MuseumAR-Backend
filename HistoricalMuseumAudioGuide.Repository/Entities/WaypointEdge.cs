using System;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class WaypointEdge
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public string FromWaypointId { get; set; } = null!;

    public string ToWaypointId { get; set; } = null!;

    public double Distance { get; set; }

    public string EdgeType { get; set; } = "WALK";

    public bool IsBidirectional { get; set; } = true;
}
