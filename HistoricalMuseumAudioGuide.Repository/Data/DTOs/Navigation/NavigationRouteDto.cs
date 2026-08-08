using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;

public class NavigationRouteRequestDto
{
    public int FromRoomId { get; set; }
    public int ToRoomId { get; set; }
}

public class NavigationInstructionDto
{
    public int StepIndex { get; set; }
    public string Instruction { get; set; } = null!;
    public string Action { get; set; } = "STRAIGHT"; // STRAIGHT, TURN_LEFT, TURN_RIGHT, STAIR_UP, STAIR_DOWN, ELEVATOR, ARRIVE
    public double Distance { get; set; }
    public int FloorNumber { get; set; }
    public string WaypointId { get; set; } = null!;
}

public class NavigationGraphDto
{
    public int MuseumId { get; set; }
    public List<WaypointDto> Waypoints { get; set; } = new();
    public List<WaypointEdgeDto> Edges { get; set; } = new();
}

public class NavigationRouteResponseDto
{
    public int FromRoomId { get; set; }
    public string FromRoomName { get; set; } = null!;
    public int ToRoomId { get; set; }
    public string ToRoomName { get; set; } = null!;
    public double TotalDistance { get; set; }
    public List<WaypointDto> PathWaypoints { get; set; } = new();
    public List<NavigationInstructionDto> Instructions { get; set; } = new();
}
