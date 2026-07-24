using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var result = await _bookingService.GetActivePaymentMethodsAsync();
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        // Tự động giải mã VisitorId từ JWT Token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out int visitorId))
        {
            return Unauthorized("Invalid token format.");
        }

        var result = await _bookingService.CreateBookingAsync(dto, visitorId);
        return Ok(result);
    }

    [HttpGet("order/{orderCode}")]
    public async Task<IActionResult> GetOrderByCode(string orderCode)
    {
        var result = await _bookingService.GetBookingByOrderCodeAsync(orderCode);

        if (result.StatusCode != 200)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}