using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
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
        private readonly IMuseumResolver _museumResolver;

        public MuseumManagerController(IMuseumManagerService managerService, IMuseumResolver museumResolver)
        {
            _managerService = managerService;
            _museumResolver = museumResolver;
        }

        /// <summary>
        /// API lấy toàn bộ dữ liệu báo cáo thống kê cho Dashboard quản lý của Bảo tàng
        /// GET: api/MuseumManager/dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.GetMuseumDashboardDataAsync(museumId);

            // Trả về kết quả động theo StatusCode định nghĩa trong ResponseModel
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpGet("ticket-types")]
        public async Task<IActionResult> GetTicketTypes()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.GetTicketTypesByMuseumAsync(museumId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpPost("ticket-types")]
        public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeDto createDto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.CreateTicketTypeAsync(museumId, createDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}