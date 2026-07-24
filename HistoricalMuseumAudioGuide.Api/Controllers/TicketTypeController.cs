using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services.Visitor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketTypeController : ControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypeController(ITicketTypeService ticketTypeService)
        {
            _ticketTypeService = ticketTypeService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllTypes([FromQuery] int? museumId, [FromQuery] bool activeOnly = true)
        {
            var response = await _ticketTypeService.GetTicketTypesAsync(museumId, activeOnly);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager")]
        [HttpGet("types/all")]
        public async Task<IActionResult> GetAllForCMS([FromQuery] int? museumId)
        {
            var response = await _ticketTypeService.GetAllForCMSAsync(museumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpGet("types/pending")]
        public async Task<IActionResult> GetPendingByMuseum()
        {
            var response = await _ticketTypeService.GetPendingTicketTypesAsync();
            return ResponseParser.Result(response);
        }

        [HttpGet("types/{id:int}")]
        public async Task<IActionResult> GetTypeById(int id)
        {
            var response = await _ticketTypeService.GetByIdAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPost("types")]
        public async Task<IActionResult> CreateType([FromBody] CreateTicketTypeDto dto)
        {
            var response = await _ticketTypeService.CreateAsync(dto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPut("types/{id:int}")]
        public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateTicketTypeDto dto)
        {
            var response = await _ticketTypeService.UpdateAsync(id, dto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager")]
        [HttpPatch("types/{id:int}/approval")]
        public async Task<IActionResult> ChangeTypeStatus(int id, [FromBody] ApprovalTicketTypeDto dto)
        {
            var response = await _ticketTypeService.ApproveOrRejectAsync(id, dto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager")]
        [HttpDelete("types/{id:int}")]
        public async Task<IActionResult> DeleteType(int id)
        {
            var response = await _ticketTypeService.DeleteAsync(id);
            return ResponseParser.Result(response);
        }
    }
}