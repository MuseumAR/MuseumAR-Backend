using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Analytics
{
    public class MuseumManagerService : IMuseumManagerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MuseumManagerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> GetMuseumDashboardDataAsync(int museumId)
        {
            // 1. Kiểm tra xem bảo tàng có tồn tại thực tế dưới DB không
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            if (museum == null)
                return ResponseModel.NotFound($"Museum with ID {museumId} not found.");

            // 2. Gọi tuần tự các hàm thống kê từ Repository để tránh lỗi Concurrency trên DbContext
            var qrStats = await _unitOfWork.Analytics.GetQrScanStatsAsync(museumId);
            var popularExhibits = await _unitOfWork.Analytics.GetPopularExhibitsAsync(museumId, topCount: 5);
            var langStats = await _unitOfWork.Analytics.GetLanguageUsageStatsAsync(museumId);
            var offlineDownloads = await _unitOfWork.Analytics.GetTotalOfflineDownloadsAsync(museumId);

            // 3. Tính toán các chỉ số KPI tổng hợp (Summary Metrics) hiển thị trên đầu Dashboard
            // Tính tổng số lượt quét QR dựa trên tổng dữ liệu quét của từng hiện vật
            int totalQrScans = qrStats.Sum(x => x.ScanCount);

            // Tính thời lượng nghe Audio trung bình (Đổi từ Giây sang Phút để thân thiện với người xem)
            double avgListeningDurationMinutes = 0;
            if (popularExhibits.Any())
            {
                double avgSeconds = popularExhibits.Average(x => x.AvgDurationSeconds);
                avgListeningDurationMinutes = Math.Round(avgSeconds / 60.0, 2);
            }

            // 4. Đóng gói toàn bộ cấu trúc dữ liệu vào DTO tổng hợp
            var dashboardData = new MuseumDashboardDto
            {
                TotalQrScans = totalQrScans,
                AverageListeningDurationMinutes = avgListeningDurationMinutes,
                TotalOfflineDownloads = offlineDownloads,
                ExhibitScanStats = qrStats,
                PopularExhibits = popularExhibits,
                LanguageUsageStats = langStats
            };

            return ResponseModel.Success("Get museum dashboard analytics successfully.", dashboardData);
        }
    }
}