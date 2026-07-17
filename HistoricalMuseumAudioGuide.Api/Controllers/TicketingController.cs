using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services.Visitor;
using HistoricalMuseumAudioGuide.Repository.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketingController : ControllerBase
    {
        private readonly ITicketingService _ticketingService;
        private readonly IVisitorService _visitorService;

        public TicketingController(ITicketingService ticketingService, IVisitorService visitorService)
        {
            _ticketingService = ticketingService;
            _visitorService = visitorService;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTicketTypes()
        {
            var response = await _ticketingService.GetTicketTypesAsync();
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _ticketingService.CreateOrderAsync(visitor.Id, request);
            return ResponseParser.Result(response);
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var queryDictionary = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            var response = await _ticketingService.HandleVnPayIpnAsync(queryDictionary);
            // VNPay requires a specific format for IPN response, simplified here.
            return Ok(new { RspCode = response.StatusCode == 200 ? "00" : "99", Message = response.Message });
        }

        [HttpGet("mock-confirm")]
        public async Task<IActionResult> MockConfirm([FromQuery] string orderCode)
        {
            var response = await _ticketingService.ConfirmMockPaymentAsync(orderCode);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _ticketingService.GetMyTicketsAsync(visitor.Id);
            return ResponseParser.Result(response);
        }
    }
}
