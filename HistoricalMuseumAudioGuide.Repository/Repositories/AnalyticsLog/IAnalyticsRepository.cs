using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Analytics
{
    public interface IAnalyticsRepository : IGenericRepository<Entities.AnalyticsLog>
    {
        // Thống kê lượt quét QR trên từng hiện vật của 1 bảo tàng
        Task<List<ExhibitScanStatDto>> GetQrScanStatsAsync(int museumId);

        // Lấy danh sách bảng xếp hạng hiện vật phổ biến nhất
        Task<List<PopularExhibitDto>> GetPopularExhibitsAsync(int museumId, int topCount);

        // Thống kê tỷ lệ sử dụng ngôn ngữ của khách tham quan
        Task<List<LanguageUsageDto>> GetLanguageUsageStatsAsync(int museumId);

        // Đếm tổng số lượt tải trọn gói dữ liệu offline (FR4.1.4)
        Task<int> GetTotalOfflineDownloadsAsync(int museumId);
    }
}