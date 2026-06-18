using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Thêm phân quyền nếu hệ thống của bạn đã bật Identity/JWT:
    // [Authorize(Roles = "MuseumManager,SystemAdmin")]
    public class MuseumManagerController : ControllerBase
    {
        private readonly IMuseumManagerService _managerService;

        public MuseumManagerController(IMuseumManagerService managerService)
        {
            _managerService = managerService;
        }

        /// <summary>
        /// API lấy toàn bộ dữ liệu báo cáo thống kê cho Dashboard quản lý của Bảo tàng
        /// GET: api/MuseumManager/dashboard/{museumId}
        /// </summary>
        [HttpGet("dashboard/{museumId:int}")]
        public async Task<IActionResult> GetDashboardData(int museumId)
        {
            var result = await _managerService.GetMuseumDashboardDataAsync(museumId);

            // Trả về kết quả động theo StatusCode định nghĩa trong ResponseModel
            return StatusCode(result.StatusCode, result);
        }
    }
}