using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.User;
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

        [HttpGet("museum-profile")]
        public async Task<IActionResult> GetMuseumProfile()
        {
            var response = await _adminService.GetMuseumProfileAsync();
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin,MuseumManager")]
        [HttpPut("museum-profile")]
        public async Task<IActionResult> UpdateMuseumProfile(UpdateMuseumProfileDto dto)
        {
            var response = await _adminService.UpdateMuseumProfileAsync(dto);
            return ResponseParser.Result(response);
        }

        // --- Ticket Type Management ---

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("ticket-types")]
        public async Task<IActionResult> GetAllTicketTypes()
        {
            var response = await _adminService.GetAllTicketTypesAsync();
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

        // --- User Management (SystemAdmin only) ---

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? status, [FromQuery] string? search)
        {
            var response = await _adminService.GetAllUsersAsync(role, status, search);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _adminService.GetUserByIdAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var response = await _adminService.CreateUserAsync(dto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var response = await _adminService.UpdateUserAsync(id, dto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _adminService.DeleteUserAsync(id);
            return ResponseParser.Result(response);
        }

        // --- Audit Logs (SystemAdmin only) ---

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int? userId,
            [FromQuery] string? action,
            [FromQuery] System.DateTime? fromDate,
            [FromQuery] System.DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _adminService.GetAuditLogsAsync(userId, action, fromDate, toDate, page, pageSize);
            return ResponseParser.Result(response);
        }
    }
}
