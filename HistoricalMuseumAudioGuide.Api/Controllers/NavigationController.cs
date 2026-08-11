using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Navigation;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Navigation;

namespace HistoricalMuseumAudioGuide.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public NavigationController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [HttpGet("museum/{museumId}/graph")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMuseumGraph(int museumId)
    {
        var graph = await _navigationService.GetGraphByMuseumIdAsync(museumId);
        return Ok(ResponseModel.Success("Lấy đồ thị chỉ đường thành công", graph));
    }

    [HttpGet("map/{mapId}/graph")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMapGraph(int mapId)
    {
        var graph = await _navigationService.GetGraphByMapIdAsync(mapId);
        return Ok(ResponseModel.Success("Lấy đồ thị chỉ đường thành công", graph));
    }

    [HttpPost("waypoints")]
    [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
    public async Task<IActionResult> CreateWaypoint([FromBody] CreateWaypointDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ResponseModel.BadRequest("Dữ liệu không hợp lệ", ModelState));
        var result = await _navigationService.CreateWaypointAsync(dto);
        return Ok(ResponseModel.Success("Tạo Waypoint thành công", result));
    }

    [HttpPut("waypoints/{id}")]
    [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
    public async Task<IActionResult> UpdateWaypoint(string id, [FromBody] UpdateWaypointDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ResponseModel.BadRequest("Dữ liệu không hợp lệ", ModelState));
        var result = await _navigationService.UpdateWaypointAsync(id, dto);
        if (result == null) return NotFound(ResponseModel.NotFound("Không tìm thấy Waypoint"));
        return Ok(ResponseModel.Success("Cập nhật Waypoint thành công", result));
    }

    [HttpDelete("waypoints/{id}")]
    [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
    public async Task<IActionResult> DeleteWaypoint(string id)
    {
        var success = await _navigationService.DeleteWaypointAsync(id);
        if (!success) return NotFound(ResponseModel.NotFound("Không tìm thấy Waypoint"));
        return Ok(ResponseModel.Success("Xóa Waypoint thành công"));
    }

    [HttpPost("edges")]
    [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
    public async Task<IActionResult> CreateEdge([FromBody] CreateWaypointEdgeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ResponseModel.BadRequest("Dữ liệu không hợp lệ", ModelState));
        var result = await _navigationService.CreateEdgeAsync(dto);
        return Ok(ResponseModel.Success("Tạo WaypointEdge thành công", result));
    }

    [HttpDelete("edges/{id}")]
    [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
    public async Task<IActionResult> DeleteEdge(int id)
    {
        var success = await _navigationService.DeleteEdgeAsync(id);
        if (!success) return NotFound(ResponseModel.NotFound("Không tìm thấy WaypointEdge"));
        return Ok(ResponseModel.Success("Xóa WaypointEdge thành công"));
    }

    [HttpGet("route")]
    [AllowAnonymous]
    public async Task<IActionResult> Navigate([FromQuery] int fromRoomId, [FromQuery] int toRoomId)
    {
        if (fromRoomId <= 0 || toRoomId <= 0)
        {
            return BadRequest(ResponseModel.BadRequest("Phòng đi và phòng đến không hợp lệ."));
        }

        var result = await _navigationService.NavigateAsync(fromRoomId, toRoomId);
        if (result == null)
        {
            return NotFound(ResponseModel.NotFound("Không tìm thấy thông tin phòng."));
        }

        return Ok(ResponseModel.Success("Tính tuyến đường thành công", result));
    }
}
