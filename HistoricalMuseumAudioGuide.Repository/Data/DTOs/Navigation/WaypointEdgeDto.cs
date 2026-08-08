using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;

public class WaypointEdgeDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public string FromWaypointId { get; set; } = null!;
    public string ToWaypointId { get; set; } = null!;
    public double Distance { get; set; }
    public string EdgeType { get; set; } = "WALK";
    public bool IsBidirectional { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateWaypointEdgeDto
{
    public int MuseumId { get; set; }
    public string FromWaypointId { get; set; } = null!;
    public string ToWaypointId { get; set; } = null!;
    public double Distance { get; set; }
    public string EdgeType { get; set; } = "WALK";
    public bool IsBidirectional { get; set; } = true;
}
