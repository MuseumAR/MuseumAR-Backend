using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services.Visitor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public TicketingController(
            ITicketingService ticketingService,
            IVisitorService visitorService,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _ticketingService = ticketingService;
            _visitorService = visitorService;
            _environment = environment;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetTicketTypes([FromQuery] string? lang)
        {
            var response = await _ticketingService.GetTicketTypesAsync(lang);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var (visitor, errorResponse) = await GetCurrentVisitorAsync();
            if (errorResponse != null) return errorResponse;

            // Safety switch & Email verification check for ticket order
            bool requireVerification = _configuration.GetValue<bool>("Auth:RequireEmailVerification", true);
            if (requireVerification && visitor!.UserId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(visitor.UserId.Value);
                if (user != null && !user.IsEmailConfirmed)
                {
                    return BadRequest(ResponseModel.BadRequest("Vui lòng xác thực Email trước khi mua vé. Hệ thống đã gửi mã xác thực tới email của bạn."));
                }
            }

            var response = await _ticketingService.CreateOrderAsync(visitor!.Id, request);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets([FromQuery] string? lang = null)
        {
            var (visitor, errorResponse) = await GetCurrentVisitorAsync();
            if (errorResponse != null) return errorResponse;

            var response = await _ticketingService.GetMyTicketsAsync(visitor!.Id, lang);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("my-tickets/{id}")]
        public async Task<IActionResult> GetTicketDetail(int id, [FromQuery] string? lang = null)
        {
            var (visitor, errorResponse) = await GetCurrentVisitorAsync();
            if (errorResponse != null) return errorResponse;

            var response = await _ticketingService.GetTicketDetailAsync(visitor!.Id, id, lang);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("pending-order")]
        public async Task<IActionResult> GetPendingOrder([FromQuery] string? lang = null)
        {
            var (visitor, errorResponse) = await GetCurrentVisitorAsync();
            if (errorResponse != null) return errorResponse;

            var response = await _ticketingService.GetPendingOrderAsync(visitor!.Id, lang);
            return ResponseParser.Result(response);
        }


        [HttpGet("validate/{ticketCode}")]
        public async Task<IActionResult> ValidateTicket(string ticketCode)
        {
            var response = await _ticketingService.ValidateTicketAsync(ticketCode);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckInTicket([FromBody] CheckInRequestDto request)
        {
            var response = await _ticketingService.CheckInTicketAsync(request.TicketCode);
            return ResponseParser.Result(response);
        }

        private async Task<(Visitor? visitor, IActionResult? errorResponse)> GetCurrentVisitorAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return (null, Unauthorized());
            }

            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
            {
                // Trả về đúng ResponseParser để giữ nguyên cấu trúc ResponseModel
                return (null, ResponseParser.Result(visitorRes));
            }

            return (visitor, null);
        }
    }
}