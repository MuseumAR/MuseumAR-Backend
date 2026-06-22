using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Admin;
using HistoricalMuseumAudioGuide.Service.Services.SystemConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ISystemConfigService _configService;

        public AdminController(IAdminService adminService, ISystemConfigService configService)
        {
            _adminService = adminService;
            _configService = configService;
        }

        [HttpGet("museums")]
        public async Task<IActionResult> GetMuseums()
        {
            var response = await _adminService.GetAllMuseumsAsync();
            return ResponseParser.Result(response);
        }

        [HttpPost("museums")]
        public async Task<IActionResult> CreateMuseum(CreateMuseumDto createMuseumDto)
        {
            var response = await _adminService.CreateMuseumAsync(createMuseumDto);
            return ResponseParser.Result(response);
        }

        // --- Ticket Type Management ---

        [HttpGet("ticket-types")]
        public async Task<IActionResult> GetAllTicketTypes()
        {
            var response = await _adminService.GetAllTicketTypesAsync();
            return ResponseParser.Result(response);
        }

        [HttpPost("ticket-types")]
        public async Task<IActionResult> CreateTicketType(CreateTicketTypeDto createTicketTypeDto)
        {
            var response = await _adminService.CreateTicketTypeAsync(createTicketTypeDto);
            return ResponseParser.Result(response);
        }

        [HttpGet("configs")]
        public async Task<IActionResult> GetAllConfigs()
        {
            var response = await _configService.GetAllConfigsAsync();
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("configs/{key}")]
        public async Task<IActionResult> UpdateConfig(string key, [FromBody] UpdateSystemConfigDto dto)
        {
            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                userId = id;
            }

            var response = await _configService.UpdateConfigAsync(key, dto, userId);
            return ResponseParser.Result(response);
        }
    }
}
