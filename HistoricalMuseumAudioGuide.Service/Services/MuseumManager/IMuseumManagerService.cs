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

        Task<ResponseModel> GetTicketTypeByIdAsync(int museumId, int ticketTypeId);

        Task<ResponseModel> CreateTicketTypeAsync(int museumId, CreateTicketTypeDto createDto);

        Task<ResponseModel> UpdateTicketTypeAsync(int museumId, int ticketTypeId, UpdateTicketTypeDto updateDto);

        Task<ResponseModel> PublishTicketTypeAsync(int museumId, int ticketTypeId);

        Task<ResponseModel> DeleteTicketTypeAsync(int museumId, int ticketTypeId);

        // Promotion management
        Task<ResponseModel> CreateTicketPromotionAsync(int museumId, int ticketTypeId, CreateTicketPromotionDto dto);
        Task<ResponseModel> GetTicketPromotionsByTicketTypeAsync(int museumId, int ticketTypeId);
        Task<ResponseModel> GetTicketPromotionByIdAsync(int museumId, int promotionId);
        Task<ResponseModel> UpdateTicketPromotionAsync(int museumId, int promotionId, UpdateTicketPromotionDto dto);
        Task<ResponseModel> DeleteTicketPromotionAsync(int museumId, int promotionId);
        Task<ResponseModel> ToggleTicketPromotionAsync(int museumId, int promotionId, bool isActive);
    }
}