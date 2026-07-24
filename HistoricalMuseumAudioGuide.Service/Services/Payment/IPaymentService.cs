using PayOS.Models;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Threading.Tasks;
using PayOS.Models.Webhooks;

namespace HistoricalMuseumAudioGuide.Service.Interfaces;

public interface IPaymentService
{
    // Tạo link/mã QR thanh toán PayOS dựa theo OrderCode của hệ thống
    Task<ResponseModel> CreatePayOSPaymentLinkAsync(string orderCode);

    // Xử lý Webhook tự động từ PayOS gửi sang khi khách chuyển khoản thành công
    Task<ResponseModel> ProcessPayOSWebhookAsync(Webhook webhookBody);
}