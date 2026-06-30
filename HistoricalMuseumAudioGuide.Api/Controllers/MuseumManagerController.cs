using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "MuseumManager,SystemAdmin")]
    public class MuseumManagerController : ControllerBase
    {
        private readonly IMuseumManagerService _managerService;

        public MuseumManagerController(IMuseumManagerService managerService)
        {
            _managerService = managerService;
        }

        /// <summary>
        /// Extracts the MuseumId from the current user's JWT claims.
        /// Returns null for SystemAdmin (unrestricted access).
        /// </summary>
        private int? GetCurrentUserMuseumId()
        {
            var museumIdClaim = User.FindFirst("MuseumId");
            if (museumIdClaim != null && int.TryParse(museumIdClaim.Value, out int museumId))
            {
                return museumId;
            }
            return null;
        }

        /// <summary>
        /// API lấy toàn bộ dữ liệu báo cáo thống kê cho Dashboard quản lý của Bảo tàng
        /// GET: api/MuseumManager/dashboard/{museumId}
        /// Museum Managers can only access their own museum's dashboard.
        /// SystemAdmin can access any museum's dashboard.
        /// </summary>
        [HttpGet("dashboard/{museumId:int}")]
        public async Task<IActionResult> GetDashboardData(int museumId)
        {
            var userMuseumId = GetCurrentUserMuseumId();

            // Museum-scoped access check: non-SystemAdmin users can only access their own museum
            if (userMuseumId.HasValue && userMuseumId.Value != museumId)
            {
                return StatusCode(403, new { StatusCode = 403, Status = "Forbidden", Message = "You do not have permission to access this museum's dashboard." });
            }

            var result = await _managerService.GetMuseumDashboardDataAsync(museumId);

            // Trả về kết quả động theo StatusCode định nghĩa trong ResponseModel
            return StatusCode(result.StatusCode, result);
        }
    }
}