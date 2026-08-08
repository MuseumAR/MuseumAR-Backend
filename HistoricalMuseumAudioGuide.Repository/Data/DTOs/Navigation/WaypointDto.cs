using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;

public class WaypointDto
{
    public string Id { get; set; } = null!;
    public int MuseumId { get; set; }
    public int MapId { get; set; }
    public int FloorNumber { get; set; }
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string WaypointType { get; set; } = "HALLWAY";
    public int? RoomId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWaypointDto
{
    public string? Id { get; set; }
    public int MuseumId { get; set; }
    public int MapId { get; set; }
    public int FloorNumber { get; set; } = 1;
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string WaypointType { get; set; } = "HALLWAY";
    public int? RoomId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public class UpdateWaypointDto
{
    public int FloorNumber { get; set; }
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string WaypointType { get; set; } = "HALLWAY";
    public int? RoomId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}
