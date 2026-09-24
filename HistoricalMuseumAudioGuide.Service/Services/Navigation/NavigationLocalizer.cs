using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Service.Services.Navigation;

public class NavLanguagePack
{
    public string StartFrom { get; set; } = "";
    public string ArrivedAt { get; set; } = "";
    public string SameRoomNextExhibit { get; set; } = "";
    public string NoPathFound { get; set; } = "";
    public string NoWaypointsConfigured { get; set; } = "";

    public string GoStraight { get; set; } = "";
    public string TurnLeft { get; set; } = "";
    public string TurnRight { get; set; } = "";

    public string StairsUp { get; set; } = "";
    public string StairsDown { get; set; } = "";
    public string ElevatorTo { get; set; } = "";

    public string WaypointDoor { get; set; } = "";
    public string WaypointStairs { get; set; } = "";
    public string WaypointElevator { get; set; } = "";
    public string WaypointHallway { get; set; } = "";
    public string WaypointRoomPattern { get; set; } = "";
    public string WaypointDefaultRoom { get; set; } = "";

    public string PrepositionThrough { get; set; } = "";
    public string PrepositionTowards { get; set; } = "";
    public string PrepositionAlong { get; set; } = "";
    public string PrepositionInto { get; set; } = "";
}

public static class NavigationLocalizer
{
    private static readonly Dictionary<string, NavLanguagePack> Packs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vi"] = new NavLanguagePack
        {
            StartFrom = "Bắt đầu di chuyển từ {0}",
            ArrivedAt = "Đã đến {0}",
            SameRoomNextExhibit = "Di chuyển đến hiện vật tiếp theo tại phòng {0}",
            NoPathFound = "Không tìm thấy tuyến đường nối từ {0} đến {1}.",
            NoWaypointsConfigured = "Chưa thiết lập nốt chỉ đường giữa phòng {0} và phòng {1}.",

            GoStraight = "Đi thẳng",
            TurnLeft = "Rẽ trái",
            TurnRight = "Rẽ phải",

            StairsUp = "Đi cầu thang lên Tầng {0}",
            StairsDown = "Đi cầu thang xuống Tầng {0}",
            ElevatorTo = "Đi thang máy đến Tầng {0}",

            WaypointDoor = "cửa phòng",
            WaypointStairs = "cầu thang",
            WaypointElevator = "thang máy",
            WaypointHallway = "hành lang",
            WaypointRoomPattern = "phòng {0}",
            WaypointDefaultRoom = "phòng trưng bày",

            PrepositionThrough = "qua",
            PrepositionTowards = "đến",
            PrepositionAlong = "dọc theo",
            PrepositionInto = "vào"
        },
        ["en"] = new NavLanguagePack
        {
            StartFrom = "Start from {0}",
            ArrivedAt = "Arrived at {0}",
            SameRoomNextExhibit = "Proceed to the next exhibit inside {0}",
            NoPathFound = "No path found from {0} to {1}.",
            NoWaypointsConfigured = "Navigation waypoints are not configured between {0} and {1}.",

            GoStraight = "Go straight",
            TurnLeft = "Turn left",
            TurnRight = "Turn right",

            StairsUp = "Take the stairs up to Floor {0}",
            StairsDown = "Take the stairs down to Floor {0}",
            ElevatorTo = "Take the elevator to Floor {0}",

            WaypointDoor = "the doorway",
            WaypointStairs = "the stairs",
            WaypointElevator = "the elevator",
            WaypointHallway = "the hallway",
            WaypointRoomPattern = "Room {0}",
            WaypointDefaultRoom = "the exhibition room",

            PrepositionThrough = "through",
            PrepositionTowards = "towards",
            PrepositionAlong = "along",
            PrepositionInto = "into"
        }
    };

    private static NavLanguagePack GetPack(string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang) && Packs.TryGetValue(lang, out var pack))
        {
            return pack;
        }
        return Packs["vi"];
    }

    public static string FormatStart(string fromRoomName, string? lang)
    {
        var pack = GetPack(lang);
        return string.Format(pack.StartFrom, fromRoomName);
    }

    public static string FormatArrival(string toRoomName, string? lang)
    {
        var pack = GetPack(lang);
        return string.Format(pack.ArrivedAt, toRoomName);
    }

    public static string FormatSameRoom(string roomName, string? lang)
    {
        var pack = GetPack(lang);
        return string.Format(pack.SameRoomNextExhibit, roomName);
    }

    public static string FormatNoPath(string fromRoomName, string toRoomName, string? lang)
    {
        var pack = GetPack(lang);
        return string.Format(pack.NoPathFound, fromRoomName, toRoomName);
    }

    public static string FormatNoWaypoints(string fromRoomName, string toRoomName, string? lang)
    {
        var pack = GetPack(lang);
        return string.Format(pack.NoWaypointsConfigured, fromRoomName, toRoomName);
    }

    public static string FormatFloorChange(int fromFloor, int toFloor, string? waypointType, string? lang)
    {
        var pack = GetPack(lang);
        if (string.Equals(waypointType, "ELEVATOR", StringComparison.OrdinalIgnoreCase))
        {
            return string.Format(pack.ElevatorTo, toFloor);
        }

        return toFloor >= fromFloor
            ? string.Format(pack.StairsUp, toFloor)
            : string.Format(pack.StairsDown, toFloor);
    }

    public static string FormatTurn(string action, string? waypointType, string? waypointName, string? waypointCode, string? lang)
    {
        var pack = GetPack(lang);
        var isEn = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

        // Determine action text
        string actionText = action switch
        {
            "TURN_RIGHT" => pack.TurnRight,
            "TURN_LEFT" => pack.TurnLeft,
            _ => pack.GoStraight
        };

        // Determine target destination description & preposition
        string prep;
        string target;

        var type = waypointType?.ToUpperInvariant() ?? "HALLWAY";
        switch (type)
        {
            case "DOOR":
                prep = pack.PrepositionThrough;
                target = !isEn && !string.IsNullOrWhiteSpace(waypointName) ? waypointName : pack.WaypointDoor;
                break;

            case "STAIRCASE":
                prep = pack.PrepositionTowards;
                target = !isEn && !string.IsNullOrWhiteSpace(waypointName) ? waypointName : pack.WaypointStairs;
                break;

            case "ELEVATOR":
                prep = pack.PrepositionTowards;
                target = !isEn && !string.IsNullOrWhiteSpace(waypointName) ? waypointName : pack.WaypointElevator;
                break;

            case "ROOM":
                prep = pack.PrepositionInto;
                if (!string.IsNullOrWhiteSpace(waypointCode))
                {
                    target = string.Format(pack.WaypointRoomPattern, waypointCode);
                }
                else if (!isEn && !string.IsNullOrWhiteSpace(waypointName))
                {
                    target = waypointName;
                }
                else
                {
                    target = pack.WaypointDefaultRoom;
                }
                break;

            default: // HALLWAY / INTERSECTION
                prep = pack.PrepositionAlong;
                target = !isEn && !string.IsNullOrWhiteSpace(waypointName) ? waypointName : pack.WaypointHallway;
                break;
        }

        return $"{actionText} {prep} {target}";
    }
}
