using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-link")]
        public async Task<IActionResult> CreatePaymentLink([FromQuery] string orderCode)
        {
            var response = await _paymentService.CreatePaymentLinkAsync(orderCode);
            return ResponseParser.Result(response);
        }

        [AllowAnonymous] // Bắt buộc Public để server PayOS có thể gọi tới mà không bị chặn JWT Token
        [HttpPost("payos-webhook")]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook webhookBody)
        {
            var response = await _paymentService.ProcessPayOSWebhookAsync(webhookBody);

            // BẮT BUỘC: Luôn trả về HTTP 200 OK cho PayOS!
            // Nếu trả về lỗi (400/500), PayOS sẽ tưởng Server của bạn gặp sự cố và bắn lại Webhook nhiều lần liên tục.
            return Ok(response);
        }

        [HttpGet("check-status/{orderCode}")]
        public async Task<IActionResult> CheckPaymentStatus(string orderCode)
        {
            var response = await _paymentService.CheckPaymentStatusAsync(orderCode);
            return ResponseParser.Result(response);
        }
    }
}