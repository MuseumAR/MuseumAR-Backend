    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;

namespace HistoricalMuseumAudioGuide.Service.Services.Navigation;

public class NavigationService : INavigationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NavigationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<NavigationGraphDto> GetGraphByMuseumIdAsync(int museumId)
    {
        var waypoints = (await _unitOfWork.Waypoints.FindAsync(w => w.MuseumId == museumId)).ToList();
        var edges = (await _unitOfWork.WaypointEdges.FindAsync(e => e.MuseumId == museumId)).ToList();

        return new NavigationGraphDto
        {
            MuseumId = museumId,
            Waypoints = waypoints.Select(MapToWaypointDto).ToList(),
            Edges = edges.Select(MapToEdgeDto).ToList()
        };
    }

    public async Task<NavigationGraphDto> GetGraphByMapIdAsync(int mapId)
    {
        var map = await _unitOfWork.MuseumMaps.GetByIdAsync(mapId);
        var museumId = map?.MuseumId ?? 0;

        var waypoints = (await _unitOfWork.Waypoints.FindAsync(w => w.MapId == mapId)).ToList();
        var wpIds = waypoints.Select(w => w.Id).ToHashSet();

        var edges = (await _unitOfWork.WaypointEdges.FindAsync(e => e.MuseumId == museumId && (wpIds.Contains(e.FromWaypointId) || wpIds.Contains(e.ToWaypointId)))).ToList();

        return new NavigationGraphDto
        {
            MuseumId = museumId,
            Waypoints = waypoints.Select(MapToWaypointDto).ToList(),
            Edges = edges.Select(MapToEdgeDto).ToList()
        };
    }

    public async Task<WaypointDto> CreateWaypointAsync(CreateWaypointDto dto)
    {
        var map = await _unitOfWork.MuseumMaps.GetByIdAsync(dto.MapId);
        var museumId = dto.MuseumId > 0 ? dto.MuseumId : (map?.MuseumId ?? 1);

        var wpId = !string.IsNullOrWhiteSpace(dto.Id) ? dto.Id.Trim() : $"WP_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var entity = new Waypoint
        {
            Id = wpId,
            MuseumId = museumId,
            MapId = dto.MapId > 0 ? dto.MapId : map?.Id,
            FloorNumber = dto.FloorNumber,
            X = dto.LocationX,
            Y = dto.LocationY,
            Type = dto.WaypointType ?? "HALLWAY",
            RoomId = dto.RoomId,
            Code = dto.Code ?? dto.Name,
            Label = dto.Name ?? dto.Code,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Waypoints.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        // Check if room needs WaypointId / DoorWaypointId
        if (dto.RoomId.HasValue)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value);
            if (room != null)
            {
                room.WaypointId = entity.Id;
                room.DoorWaypointId = entity.Id;
                _unitOfWork.Rooms.Update(room);
                await _unitOfWork.CompleteAsync();
            }
        }

        return MapToWaypointDto(entity);
    }

    public async Task<WaypointDto?> UpdateWaypointAsync(string id, UpdateWaypointDto dto)
    {
        var entity = await _unitOfWork.Waypoints.GetByIdAsync(id);
        if (entity == null) return null;

        entity.FloorNumber = dto.FloorNumber;
        entity.X = dto.LocationX;
        entity.Y = dto.LocationY;
        entity.Type = dto.WaypointType ?? entity.Type;
        entity.RoomId = dto.RoomId;
        entity.Code = dto.Code ?? entity.Code;
        entity.Label = dto.Name ?? dto.Code ?? entity.Label;
        entity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Waypoints.Update(entity);
        await _unitOfWork.CompleteAsync();

        return MapToWaypointDto(entity);
    }

    public async Task<bool> DeleteWaypointAsync(string id)
    {
        var entity = await _unitOfWork.Waypoints.GetByIdAsync(id);
        if (entity == null) return false;

        // Delete connected edges
        var edges = await _unitOfWork.WaypointEdges.FindAsync(e => e.FromWaypointId == id || e.ToWaypointId == id);
        foreach (var edge in edges)
        {
            _unitOfWork.WaypointEdges.Delete(edge);
        }

        _unitOfWork.Waypoints.Delete(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<WaypointEdgeDto> CreateEdgeAsync(CreateWaypointEdgeDto dto)
    {
        if (dto.FromWaypointId == dto.ToWaypointId)
        {
            throw new InvalidOperationException("Không thể tạo cạnh nối một điểm với chính nó.");
        }

        var fromWp = await _unitOfWork.Waypoints.GetByIdAsync(dto.FromWaypointId);
        var toWp = await _unitOfWork.Waypoints.GetByIdAsync(dto.ToWaypointId);

        if (fromWp == null || toWp == null)
        {
            throw new ArgumentException("Một hoặc cả hai Waypoint không tồn tại.");
        }

        // Validate: Do not allow connecting two room/door waypoints directly
        bool isFromRoom = string.Equals(fromWp.Type, "DOOR", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(fromWp.Type, "ROOM", StringComparison.OrdinalIgnoreCase)
                       || fromWp.RoomId.HasValue;
        bool isToRoom = string.Equals(toWp.Type, "DOOR", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(toWp.Type, "ROOM", StringComparison.OrdinalIgnoreCase)
                     || toWp.RoomId.HasValue;

        if (isFromRoom && isToRoom)
        {
            throw new InvalidOperationException("Không thể nối trực tiếp giữa hai phòng. Tuyến đường phải đi qua điểm hành lang (Hallway), sảnh (Lobby) hoặc cầu thang (Stairs).");
        }

        var museumId = dto.MuseumId > 0 ? dto.MuseumId : (fromWp.MuseumId > 0 ? fromWp.MuseumId : 1);

        var entity = new WaypointEdge
        {
            MuseumId = museumId,
            FromWaypointId = dto.FromWaypointId,
            ToWaypointId = dto.ToWaypointId,
            Distance = dto.Distance > 0 ? dto.Distance : 1.0,
            EdgeType = dto.EdgeType ?? (fromWp.FloorNumber != toWp.FloorNumber ? "STAIR" : "WALK"),
            IsBidirectional = dto.IsBidirectional,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.WaypointEdges.AddAsync(entity);
        await _unitOfWork.CompleteAsync();

        return MapToEdgeDto(entity);
    }

    public async Task<bool> DeleteEdgeAsync(int id)
    {
        var entity = await _unitOfWork.WaypointEdges.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.WaypointEdges.Delete(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<NavigationRouteResponseDto?> NavigateAsync(int fromRoomId, int toRoomId, string? lang = null)
    {
        var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

        var fromRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
            r => r.Id == fromRoomId,
            includeProperties: "RoomTranslations");
        var toRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
            r => r.Id == toRoomId,
            includeProperties: "RoomTranslations");
        if (fromRoom == null || toRoom == null) return null;

        var fromRoomName = ResolveRoomDisplayName(fromRoom, en);
        var toRoomName = ResolveRoomDisplayName(toRoom, en);

        var museumId = fromRoom.MuseumId;

        var waypoints = (await _unitOfWork.Waypoints.FindAsync(w => w.MuseumId == museumId)).ToList();
        var waypointDict = waypoints.ToDictionary(w => w.Id);

        // Find Start & End Waypoints
        Waypoint? startWp = null;
        if (!string.IsNullOrEmpty(fromRoom.WaypointId) && waypointDict.ContainsKey(fromRoom.WaypointId))
        {
            startWp = waypointDict[fromRoom.WaypointId];
        }
        else
        {
            startWp = waypoints.FirstOrDefault(w => w.RoomId == fromRoomId);
        }

        Waypoint? endWp = null;
        if (!string.IsNullOrEmpty(toRoom.WaypointId) && waypointDict.ContainsKey(toRoom.WaypointId))
        {
            endWp = waypointDict[toRoom.WaypointId];
        }
        else
        {
            endWp = waypoints.FirstOrDefault(w => w.RoomId == toRoomId);
        }

        if (startWp == null || endWp == null)
        {
            return new NavigationRouteResponseDto
            {
                FromRoomId = fromRoomId,
                FromRoomName = fromRoomName,
                ToRoomId = toRoomId,
                ToRoomName = toRoomName,
                TotalDistance = 0,
                PathWaypoints = new List<WaypointDto>(),
                Instructions = new List<NavigationInstructionDto>
                {
                    new NavigationInstructionDto
                    {
                        StepIndex = 1,
                        Instruction = en
                            ? $"Navigation waypoints are not set up for {fromRoomName} or {toRoomName}."
                            : $"Chưa thiết lập nốt chỉ đường cho phòng {fromRoomName} hoặc {toRoomName}.",
                        Action = "ARRIVE",
                        WaypointId = ""
                    }
                }
            };
        }

        var edges = (await _unitOfWork.WaypointEdges.FindAsync(e => e.MuseumId == museumId)).ToList();

        // Build Graph Adjacency List
        var adj = new Dictionary<string, List<(string toId, double dist, string edgeType)>>();
        foreach (var wp in waypoints)
        {
            adj[wp.Id] = new List<(string, double, string)>();
        }

        foreach (var edge in edges)
        {
            if (adj.ContainsKey(edge.FromWaypointId) && adj.ContainsKey(edge.ToWaypointId))
            {
                adj[edge.FromWaypointId].Add((edge.ToWaypointId, edge.Distance, edge.EdgeType));
                if (edge.IsBidirectional)
                {
                    adj[edge.ToWaypointId].Add((edge.FromWaypointId, edge.Distance, edge.EdgeType));
                }
            }
        }

        // Dijkstra
        var dist = new Dictionary<string, double>();
        var prev = new Dictionary<string, string>();
        var pq = new SortedSet<(double distance, string wpId)>(Comparer<(double, string)>.Create((a, b) => a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : string.Compare(a.Item2, b.Item2, StringComparison.Ordinal)));

        foreach (var wp in waypoints)
        {
            dist[wp.Id] = double.MaxValue;
        }

        dist[startWp.Id] = 0;
        pq.Add((0, startWp.Id));

        while (pq.Count > 0)
        {
            var current = pq.Min;
            pq.Remove(current);

            var currId = current.wpId;
            if (currId == endWp.Id) break;

            foreach (var neighbor in adj[currId])
            {
                var newDist = dist[currId] + neighbor.dist;
                if (newDist < dist[neighbor.toId])
                {
                    pq.Remove((dist[neighbor.toId], neighbor.toId));
                    dist[neighbor.toId] = newDist;
                    prev[neighbor.toId] = currId;
                    pq.Add((newDist, neighbor.toId));
                }
            }
        }

        if (dist[endWp.Id] == double.MaxValue)
        {
            return new NavigationRouteResponseDto
            {
                FromRoomId = fromRoomId,
                FromRoomName = fromRoomName,
                ToRoomId = toRoomId,
                ToRoomName = toRoomName,
                TotalDistance = 0,
                PathWaypoints = new List<WaypointDto>(),
                Instructions = new List<NavigationInstructionDto>
                {
                    new NavigationInstructionDto
                    {
                        StepIndex = 1,
                        Instruction = en
                            ? $"No path found from {fromRoomName} to {toRoomName}."
                            : $"Không tìm thấy tuyến đường nối từ {fromRoomName} đến {toRoomName}.",
                        Action = "ARRIVE",
                        WaypointId = startWp.Id
                    }
                }
            };
        }

        // Reconstruct path
        var pathIds = new List<string>();
        var curr = endWp.Id;
        while (curr != startWp.Id)
        {
            pathIds.Add(curr);
            curr = prev[curr];
        }
        pathIds.Add(startWp.Id);
        pathIds.Reverse();

        var pathWaypoints = pathIds.Select(id => MapToWaypointDto(waypointDict[id])).ToList();

        // Check for floor transitions
        int firstTransitionIndex = -1;
        for (int i = 0; i < pathWaypoints.Count - 1; i++)
        {
            if (pathWaypoints[i].FloorNumber != pathWaypoints[i + 1].FloorNumber)
            {
                firstTransitionIndex = i;
                break;
            }
        }

        List<WaypointDto> finalPath;
        double finalDistance;
        bool hasTransition = false;
        int transitionTargetFloor = 1;
        string transitionWaypointType = "STAIRS";
        string transitionAction = "STAIR_UP";

        if (firstTransitionIndex != -1)
        {
            var w1 = pathWaypoints[firstTransitionIndex];
            var w2 = pathWaypoints[firstTransitionIndex + 1];
            finalPath = pathWaypoints.Take(firstTransitionIndex + 1).ToList();
            finalDistance = dist[w1.Id];
            hasTransition = true;
            transitionTargetFloor = w2.FloorNumber;
            transitionWaypointType = w2.WaypointType ?? "STAIRS";
            transitionAction = w2.FloorNumber > w1.FloorNumber ? "STAIR_UP" : "STAIR_DOWN";
        }
        else
        {
            finalPath = pathWaypoints;
            finalDistance = dist[endWp.Id];
        }

        var instructions = GenerateInstructions(finalPath, fromRoomName, toRoomName, en);

        if (hasTransition && instructions.Count > 0)
        {
            var lastInst = instructions.Last();
            var isElevator = transitionWaypointType == "ELEVATOR";
            var isUp = transitionAction == "STAIR_UP";

            string actionText = isElevator
                ? (en ? "Take the elevator" : "Đi thang máy")
                : (en ? "Take the stairs" : "Đi cầu thang bộ");

            string dirText = isUp
                ? (en ? "up to" : "lên")
                : (en ? "down to" : "xuống");

            string floorWord = en ? "Floor" : "Tầng";

            lastInst.Instruction = en
                ? $"{actionText} {dirText} {floorWord} {transitionTargetFloor}, then select your next destination."
                : $"{actionText} {dirText} {floorWord} {transitionTargetFloor} rồi chọn tiếp phòng cần đến.";

            lastInst.Action = isElevator ? "ELEVATOR" : transitionAction;
        }

        return new NavigationRouteResponseDto
        {
            FromRoomId = fromRoomId,
            FromRoomName = fromRoomName,
            ToRoomId = toRoomId,
            ToRoomName = toRoomName,
            TotalDistance = Math.Round(finalDistance, 1),
            PathWaypoints = finalPath,
            Instructions = instructions
        };
    }

    private static string ResolveRoomDisplayName(Room room, bool en)
    {
        if (en)
        {
            var enName = room.RoomTranslations?
                .FirstOrDefault(t => t.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase))
                ?.RoomName;
            if (!string.IsNullOrWhiteSpace(enName)) return enName;
            if (!string.IsNullOrWhiteSpace(room.RoomCode)) return $"Room {room.RoomCode}";
        }
        return room.RoomName;
    }

    private List<NavigationInstructionDto> GenerateInstructions(
        List<WaypointDto> path,
        string fromRoomName,
        string toRoomName,
        bool en = false)
    {
        var instructions = new List<NavigationInstructionDto>();
        if (path == null || path.Count == 0) return instructions;

        int stepIndex = 1;
        instructions.Add(new NavigationInstructionDto
        {
            StepIndex = stepIndex++,
            Instruction = en
                ? $"Start from {fromRoomName}"
                : $"Bắt đầu di chuyển từ {fromRoomName}",
            Action = "STRAIGHT",
            Distance = 0,
            FloorNumber = path[0].FloorNumber,
            WaypointId = path[0].Id
        });

        for (int i = 0; i < path.Count - 1; i++)
        {
            var w1 = path[i];
            var w2 = path[i + 1];

            if (w1.FloorNumber != w2.FloorNumber)
            {
                var action = w2.FloorNumber > w1.FloorNumber ? "STAIR_UP" : "STAIR_DOWN";
                var actionText = w2.WaypointType == "ELEVATOR"
                    ? (en ? "Take the elevator" : "Đi thang máy")
                    : (en ? "Take the stairs" : "Đi cầu thang");
                var floorWord = en ? "Floor" : "Tầng";
                var toWord = en ? "to" : "lên";
                instructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = $"{actionText} {toWord} {floorWord} {w2.FloorNumber}",
                    Action = action,
                    Distance = 1.0,
                    FloorNumber = w2.FloorNumber,
                    WaypointId = w2.Id
                });
            }
            else
            {
                double dx = w2.LocationX - w1.LocationX;
                double dy = w2.LocationY - w1.LocationY;
                double distVal = Math.Round(Math.Sqrt(dx * dx + dy * dy), 1);

                string turnText = en ? "Go straight" : "Đi thẳng";
                string action = "STRAIGHT";

                if (i > 0)
                {
                    var w0 = path[i - 1];
                    if (w0.FloorNumber == w1.FloorNumber)
                    {
                        double v1x = w1.LocationX - w0.LocationX;
                        double v1y = w1.LocationY - w0.LocationY;
                        double v2x = w2.LocationX - w1.LocationX;
                        double v2y = w2.LocationY - w1.LocationY;

                        double crossProduct = v1x * v2y - v1y * v2x;
                        if (crossProduct > 10)
                        {
                            turnText = en ? "Turn right" : "Rẽ phải";
                            action = "TURN_RIGHT";
                        }
                        else if (crossProduct < -10)
                        {
                            turnText = en ? "Turn left" : "Rẽ trái";
                            action = "TURN_LEFT";
                        }
                    }
                }

                var via = w2.Name ?? (w2.WaypointType == "DOOR"
                    ? (en ? "the doorway" : "cửa phòng")
                    : (en ? "the hallway" : "hành lang"));
                var through = en ? "through" : "qua";

                instructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = $"{turnText} {through} {via}",
                    Action = action,
                    Distance = distVal,
                    FloorNumber = w2.FloorNumber,
                    WaypointId = w2.Id
                });
            }
        }

        var lastWp = path.Last();
        instructions.Add(new NavigationInstructionDto
        {
            StepIndex = stepIndex++,
            Instruction = en ? $"Arrived at {toRoomName}" : $"Đã đến {toRoomName}",
            Action = "ARRIVE",
            Distance = 0,
            FloorNumber = lastWp.FloorNumber,
            WaypointId = lastWp.Id
        });

        return instructions;
    }

    private WaypointDto MapToWaypointDto(Waypoint w) => new WaypointDto
    {
        Id = w.Id,
        MuseumId = w.MuseumId,
        MapId = w.MapId ?? 0,
        FloorNumber = w.FloorNumber,
        LocationX = w.X,
        LocationY = w.Y,
        WaypointType = w.Type,
        RoomId = w.RoomId,
        Code = w.Code ?? w.Label,
        Name = w.Label ?? w.Code,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };

    private WaypointEdgeDto MapToEdgeDto(WaypointEdge e) => new WaypointEdgeDto
    {
        Id = e.Id,
        MuseumId = e.MuseumId,
        FromWaypointId = e.FromWaypointId,
        ToWaypointId = e.ToWaypointId,
        Distance = e.Distance,
        EdgeType = e.EdgeType,
        IsBidirectional = e.IsBidirectional,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    public async Task<NavigationRouteResponseDto?> NavigateTourRouteAsync(int tourRouteId, string? lang = null)
    {
        var en = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);

        var tourRoute = await _unitOfWork.TourRoutes.GetFirstOrDefaultAsync(
            tr => tr.Id == tourRouteId,
            includeProperties: "TourRouteExhibits.Exhibit.Room.RoomTranslations,TourRouteExhibits.Exhibit.ExhibitTranslations,TourRouteTranslations"
        );

        if (tourRoute == null) return null;

        var orderedStops = tourRoute.TourRouteExhibits
            .OrderBy(s => s.StopOrder)
            .ToList();

        if (orderedStops.Count == 0)
        {
            return new NavigationRouteResponseDto
            {
                FromRoomId = 0,
                FromRoomName = "N/A",
                ToRoomId = 0,
                ToRoomName = "N/A",
                TotalDistance = 0,
                PathWaypoints = new List<WaypointDto>(),
                Instructions = new List<NavigationInstructionDto>()
            };
        }

        var stopsWithRooms = orderedStops
            .Where(s => s.Exhibit != null && s.Exhibit.RoomId.HasValue)
            .Select(s => new {
                ExhibitName = en 
                    ? s.Exhibit.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase))?.Title ?? s.Exhibit.ExhibitCode
                    : s.Exhibit.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase))?.Title ?? s.Exhibit.ExhibitCode,
                RoomId = s.Exhibit.RoomId!.Value,
                Room = s.Exhibit.Room!
            })
            .ToList();

        if (stopsWithRooms.Count == 0)
        {
            return new NavigationRouteResponseDto
            {
                FromRoomId = 0,
                FromRoomName = "N/A",
                ToRoomId = 0,
                ToRoomName = "N/A",
                TotalDistance = 0,
                PathWaypoints = new List<WaypointDto>(),
                Instructions = new List<NavigationInstructionDto>()
            };
        }

        var museumId = tourRoute.MuseumId;
        var waypoints = (await _unitOfWork.Waypoints.FindAsync(w => w.MuseumId == museumId)).ToList();
        var waypointDict = waypoints.ToDictionary(w => w.Id);
        var edges = (await _unitOfWork.WaypointEdges.FindAsync(e => e.MuseumId == museumId)).ToList();

        var adj = new Dictionary<string, List<(string toId, double dist, string edgeType)>>();
        foreach (var wp in waypoints)
        {
            adj[wp.Id] = new List<(string, double, string)>();
        }

        foreach (var edge in edges)
        {
            if (adj.ContainsKey(edge.FromWaypointId) && adj.ContainsKey(edge.ToWaypointId))
            {
                adj[edge.FromWaypointId].Add((edge.ToWaypointId, edge.Distance, edge.EdgeType));
                if (edge.IsBidirectional)
                {
                    adj[edge.ToWaypointId].Add((edge.FromWaypointId, edge.Distance, edge.EdgeType));
                }
            }
        }

        var mergedWaypoints = new List<WaypointDto>();
        var mergedInstructions = new List<NavigationInstructionDto>();
        double totalDistance = 0;
        int stepIndex = 1;

        for (int i = 0; i < stopsWithRooms.Count - 1; i++)
        {
            var fromStop = stopsWithRooms[i];
            var toStop = stopsWithRooms[i + 1];

            var fromRoomName = ResolveRoomDisplayName(fromStop.Room, en);
            var toRoomName = ResolveRoomDisplayName(toStop.Room, en);

            if (fromStop.RoomId == toStop.RoomId)
            {
                var sameRoomInstruction = en
                    ? $"Proceed to the next exhibit inside {fromRoomName}"
                    : $"Di chuyển đến hiện vật tiếp theo tại phòng {fromRoomName}";

                mergedInstructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = sameRoomInstruction,
                    Action = "ARRIVE",
                    Distance = 0,
                    FloorNumber = fromStop.Room.FloorNumber,
                    WaypointId = fromStop.Room.WaypointId ?? ""
                });

                if (i == 0)
                {
                    Waypoint? startWp = null;
                    if (!string.IsNullOrEmpty(fromStop.Room.WaypointId) && waypointDict.ContainsKey(fromStop.Room.WaypointId))
                    {
                        startWp = waypointDict[fromStop.Room.WaypointId];
                    }
                    else
                    {
                        startWp = waypoints.FirstOrDefault(w => w.RoomId == fromStop.RoomId);
                    }
                    if (startWp != null)
                    {
                        mergedWaypoints.Add(MapToWaypointDto(startWp));
                    }
                }
                continue;
            }

            Waypoint? startWpSeg = null;
            if (!string.IsNullOrEmpty(fromStop.Room.WaypointId) && waypointDict.ContainsKey(fromStop.Room.WaypointId))
            {
                startWpSeg = waypointDict[fromStop.Room.WaypointId];
            }
            else
            {
                startWpSeg = waypoints.FirstOrDefault(w => w.RoomId == fromStop.RoomId);
            }

            Waypoint? endWpSeg = null;
            if (!string.IsNullOrEmpty(toStop.Room.WaypointId) && waypointDict.ContainsKey(toStop.Room.WaypointId))
            {
                endWpSeg = waypointDict[toStop.Room.WaypointId];
            }
            else
            {
                endWpSeg = waypoints.FirstOrDefault(w => w.RoomId == toStop.RoomId);
            }

            if (startWpSeg == null || endWpSeg == null)
            {
                mergedInstructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = en
                        ? $"Navigation waypoints are not set up between {fromRoomName} and {toRoomName}."
                        : $"Chưa thiết lập nốt chỉ đường giữa phòng {fromRoomName} và phòng {toRoomName}.",
                    Action = "ARRIVE",
                    WaypointId = ""
                });
                continue;
            }

            var dist = new Dictionary<string, double>();
            var prev = new Dictionary<string, string>();
            var pq = new SortedSet<(double distance, string wpId)>(Comparer<(double, string)>.Create((a, b) => a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : string.Compare(a.Item2, b.Item2, StringComparison.Ordinal)));

            foreach (var wp in waypoints)
            {
                dist[wp.Id] = double.MaxValue;
            }

            dist[startWpSeg.Id] = 0;
            pq.Add((0, startWpSeg.Id));

            while (pq.Count > 0)
            {
                var current = pq.Min;
                pq.Remove(current);

                var currId = current.wpId;
                if (currId == endWpSeg.Id) break;

                if (!adj.ContainsKey(currId)) continue;

                foreach (var neighbor in adj[currId])
                {
                    var newDist = dist[currId] + neighbor.dist;
                    if (newDist < dist[neighbor.toId])
                    {
                        pq.Remove((dist[neighbor.toId], neighbor.toId));
                        dist[neighbor.toId] = newDist;
                        prev[neighbor.toId] = currId;
                        pq.Add((newDist, neighbor.toId));
                    }
                }
            }

            if (dist[endWpSeg.Id] == double.MaxValue)
            {
                mergedInstructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = en
                        ? $"No path found from {fromRoomName} to {toRoomName}."
                        : $"Không tìm thấy tuyến đường nối từ {fromRoomName} đến {toRoomName}.",
                    Action = "ARRIVE",
                    WaypointId = startWpSeg.Id
                });
                continue;
            }

            var pathIds = new List<string>();
            var curr = endWpSeg.Id;
            while (curr != startWpSeg.Id)
            {
                pathIds.Add(curr);
                curr = prev[curr];
            }
            pathIds.Add(startWpSeg.Id);
            pathIds.Reverse();

            var pathWaypoints = pathIds.Select(id => MapToWaypointDto(waypointDict[id])).ToList();
            var segmentInstructions = GenerateInstructions(pathWaypoints, fromRoomName, toRoomName, en);

            int startIdx = 0;
            if (mergedWaypoints.Count > 0 && mergedWaypoints.Last().Id == pathWaypoints[0].Id)
            {
                startIdx = 1;
            }

            for (int j = startIdx; j < pathWaypoints.Count; j++)
            {
                mergedWaypoints.Add(pathWaypoints[j]);
            }

            foreach (var inst in segmentInstructions)
            {
                mergedInstructions.Add(new NavigationInstructionDto
                {
                    StepIndex = stepIndex++,
                    Instruction = inst.Instruction,
                    Action = inst.Action,
                    Distance = inst.Distance,
                    FloorNumber = inst.FloorNumber,
                    WaypointId = inst.WaypointId
                });
            }

            totalDistance += dist[endWpSeg.Id];
        }

        var firstStop = stopsWithRooms.First();
        var lastStop = stopsWithRooms.Last();

        return new NavigationRouteResponseDto
        {
            FromRoomId = firstStop.RoomId,
            FromRoomName = ResolveRoomDisplayName(firstStop.Room, en),
            ToRoomId = lastStop.RoomId,
            ToRoomName = ResolveRoomDisplayName(lastStop.Room, en),
            TotalDistance = Math.Round(totalDistance, 1),
            PathWaypoints = mergedWaypoints,
            Instructions = mergedInstructions
        };
    }
}
