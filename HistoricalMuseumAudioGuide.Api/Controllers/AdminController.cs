using HistoricalMuseumAudioGuide.Repository.Data.DTOs;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Admin;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
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

        [HttpGet("museums/{id}/exhibits")]
        public async Task<IActionResult> GetExhibits(int id)
        {
            var response = await _adminService.GetExhibitsByMuseumIdAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpGet("museums/{id}/exhibitions")]
        public async Task<IActionResult> GetExhibitions(int id)
        {
            var response = await _adminService.GetExhibitionsByMuseumIdAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpPost("exhibitions")]
        public async Task<IActionResult> CreateExhibition(CreateExhibitionDto createExhibitionDto)
        {
            var response = await _adminService.CreateExhibitionAsync(createExhibitionDto);
            return ResponseParser.Result(response);
        }
    }
}
