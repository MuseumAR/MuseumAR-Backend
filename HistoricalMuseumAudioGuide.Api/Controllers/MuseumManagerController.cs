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
        [HttpGet("ticket-types/{id}")]
        public async Task<IActionResult> GetTicketTypeDetail(int id)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.GetTicketTypeByIdAsync(museumId, id);
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

        [Authorize(Roles = "MuseumManager")]
        [HttpPut("ticket-types/{id}")]
        public async Task<IActionResult> UpdateTicketType(int id, [FromBody] UpdateTicketTypeDto updateDto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.UpdateTicketTypeAsync(museumId, id, updateDto);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpPut("ticket-types/{id}/publish")]
        public async Task<IActionResult> PublishTicketType(int id)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.PublishTicketTypeAsync(museumId, id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpDelete("ticket-types/{id}")]
        public async Task<IActionResult> DeleteTicketType(int id)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.DeleteTicketTypeAsync(museumId, id);
            return StatusCode(result.StatusCode, result);
        }

        // ═══ PROMOTION ENDPOINTS ═══

        /// <summary>
        /// Tạo chương trình khuyến mãi cho một loại vé
        /// POST: api/MuseumManager/ticket-types/{ticketTypeId}/promotions
        /// </summary>
        [Authorize(Roles = "MuseumManager")]
        [HttpPost("ticket-types/{ticketTypeId}/promotions")]
        public async Task<IActionResult> CreateTicketPromotion(int ticketTypeId, [FromBody] CreateTicketPromotionDto dto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.CreateTicketPromotionAsync(museumId, ticketTypeId, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xem danh sách khuyến mãi của một loại vé
        /// GET: api/MuseumManager/ticket-types/{ticketTypeId}/promotions
        /// </summary>
        [HttpGet("ticket-types/{ticketTypeId}/promotions")]
        public async Task<IActionResult> GetTicketPromotions(int ticketTypeId)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.GetTicketPromotionsByTicketTypeAsync(museumId, ticketTypeId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xem chi tiết một chương trình khuyến mãi
        /// GET: api/MuseumManager/promotions/{promotionId}
        /// </summary>
        [HttpGet("promotions/{promotionId}")]
        public async Task<IActionResult> GetTicketPromotionDetail(int promotionId)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.GetTicketPromotionByIdAsync(museumId, promotionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật thông tin chương trình khuyến mãi
        /// PUT: api/MuseumManager/promotions/{promotionId}
        /// </summary>
        [Authorize(Roles = "MuseumManager")]
        [HttpPut("promotions/{promotionId}")]
        public async Task<IActionResult> UpdateTicketPromotion(int promotionId, [FromBody] UpdateTicketPromotionDto dto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.UpdateTicketPromotionAsync(museumId, promotionId, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xóa hoàn toàn một chương trình khuyến mãi
        /// DELETE: api/MuseumManager/promotions/{promotionId}
        /// </summary>
        [Authorize(Roles = "MuseumManager")]
        [HttpDelete("promotions/{promotionId}")]
        public async Task<IActionResult> DeleteTicketPromotion(int promotionId)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.DeleteTicketPromotionAsync(museumId, promotionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bật/tắt chương trình khuyến mãi
        /// PUT: api/MuseumManager/promotions/{promotionId}/toggle?isActive=true|false
        /// </summary>
        [Authorize(Roles = "MuseumManager")]
        [HttpPut("promotions/{promotionId}/toggle")]
        public async Task<IActionResult> ToggleTicketPromotion(int promotionId, [FromQuery] bool isActive)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var result = await _managerService.ToggleTicketPromotionAsync(museumId, promotionId, isActive);
            return StatusCode(result.StatusCode, result);
        }
    }
}