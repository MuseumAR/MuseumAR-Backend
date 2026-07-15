using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Analytics
{
    public interface IMuseumManagerService
    {
        /// <summary>
        /// Lấy toàn bộ dữ liệu thống kê tổng hợp cho Dashboard của Museum Manager
        /// </summary>
        /// <param name="museumId">Id của bảo tàng cần xem báo cáo</param>
        Task<ResponseModel> GetMuseumDashboardDataAsync(int museumId);

        Task<ResponseModel> GetTicketTypesByMuseumAsync(int museumId);

        Task<ResponseModel> CreateTicketTypeAsync(int museumId, CreateTicketTypeDto createDto);
    }
}