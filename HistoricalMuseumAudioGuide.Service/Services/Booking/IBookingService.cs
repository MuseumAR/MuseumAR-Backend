using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Interfaces;

public interface IBookingService
{
    // 1. Lấy danh sách phương thức thanh toán đang hoạt động (MoMo, VNPay, Tiền mặt...)
    Task<ResponseModel> GetActivePaymentMethodsAsync();

    // 2. Tạo đơn đặt vé mới (Tạo Transaction & Tickets ở trạng thái Pending, chưa thanh toán)
    Task<ResponseModel> CreateBookingAsync(CreateBookingDto dto, int userId);

    // 3. Tra cứu chi tiết đơn hàng đặt vé theo Mã đơn hàng (OrderCode)
    Task<ResponseModel> GetBookingByOrderCodeAsync(string orderCode);
}