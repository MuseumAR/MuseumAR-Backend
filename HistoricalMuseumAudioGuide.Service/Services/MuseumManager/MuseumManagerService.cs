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

            // 2. Kích hoạt gọi đồng thời (Concurrent) các hàm thống kê từ Repository mới tối ưu
            var qrStatsTask = _unitOfWork.Analytics.GetQrScanStatsAsync(museumId);
            var popularExhibitsTask = _unitOfWork.Analytics.GetPopularExhibitsAsync(museumId, topCount: 5);
            var langStatsTask = _unitOfWork.Analytics.GetLanguageUsageStatsAsync(museumId);
            var offlineDownloadsTask = _unitOfWork.Analytics.GetTotalOfflineDownloadsAsync(museumId);

            // Chờ tất cả các Task xử lý SQL chạy xong hoàn toàn
            await Task.WhenAll(qrStatsTask, popularExhibitsTask, langStatsTask, offlineDownloadsTask);

            // Bóc tách kết quả từ các Task sau khi hoàn thành
            var qrStats = await qrStatsTask;
            var popularExhibits = await popularExhibitsTask;
            var langStats = await langStatsTask;
            var offlineDownloads = await offlineDownloadsTask;

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