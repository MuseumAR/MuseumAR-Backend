using HistoricalMuseumAudioGuide.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.API.Controllers;

[ApiController]
[Route("api/payment")] // 🟢 Đổi Route cha thành /api/payment cho đúng ngữ nghĩa
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // 1. Endpoint cho Frontend gọi tạo Link thanh toán
    // Đường dẫn: POST /api/payment/create-payos-link?orderCode=xxx
    [HttpPost("create-payos-link")]
    public async Task<IActionResult> CreatePayOSLink([FromQuery] string orderCode)
    {
        var result = await _paymentService.CreatePayOSPaymentLinkAsync(orderCode);

        if (result.StatusCode == 404)
            return NotFound(result);

        if (result.StatusCode == 400)
            return BadRequest(result);

        return Ok(result);
    }

    // 2. Endpoint cho PayOS bắn Webhook sang khi chuyển khoản thành công
    // Đường dẫn: POST /api/payment/webhook
    [HttpPost("webhook")]
    public async Task<IActionResult> HandlePayOSWebhook([FromBody] Webhook webhookBody)
    {
        var result = await _paymentService.ProcessPayOSWebhookAsync(webhookBody);

        // 🟢 LUÔN LUÔN trả về HTTP 200 OK để PayOS xác nhận đã giao nhận Webhook thành công
        return Ok(result);
    }
}