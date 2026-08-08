using System.Threading.Tasks;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;

namespace HistoricalMuseumAudioGuide.Service.Services.Navigation;

public interface INavigationService
{
    Task<NavigationGraphDto> GetGraphByMuseumIdAsync(int museumId);
    Task<NavigationGraphDto> GetGraphByMapIdAsync(int mapId);
    Task<WaypointDto> CreateWaypointAsync(CreateWaypointDto dto);
    Task<WaypointDto?> UpdateWaypointAsync(string id, UpdateWaypointDto dto);
    Task<bool> DeleteWaypointAsync(string id);
    Task<WaypointEdgeDto> CreateEdgeAsync(CreateWaypointEdgeDto dto);
    Task<bool> DeleteEdgeAsync(int id);
    Task<NavigationRouteResponseDto?> NavigateAsync(int fromRoomId, int toRoomId);
}
