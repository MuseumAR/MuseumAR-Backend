using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
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

        public TicketingController(ITicketingService ticketingService)
        {
            _ticketingService = ticketingService;
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
            
            int visitorId = int.Parse(userIdClaim.Value);
            var response = await _ticketingService.CreateOrderAsync(visitorId, request);
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
            
            int visitorId = int.Parse(userIdClaim.Value);
            var response = await _ticketingService.GetMyTicketsAsync(visitorId);
            return ResponseParser.Result(response);
        }
    }
}
