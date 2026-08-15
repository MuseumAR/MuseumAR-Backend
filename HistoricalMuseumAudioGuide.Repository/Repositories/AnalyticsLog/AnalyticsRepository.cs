using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Analytics
{
    public class AnalyticsRepository : GenericRepository<Entities.AnalyticsLog>, IAnalyticsRepository
    {
        public AnalyticsRepository(MuseumAudioGuideContext context) : base(context)
        {
        }

        // 1. Thống kê lượt quét QR trên từng hiện vật (FR4.1.1)
        public async Task<List<ExhibitScanStatDto>> GetQrScanStatsAsync(int museumId)
        {
            return await _context.AnalyticsLogs
                .Where(log => log.MuseumId == museumId && log.ActionType == "QR_SCAN" && log.ExhibitId != null)
                .GroupBy(log => new
                {
                    log.ExhibitId,
                    ExhibitTitle = log.Exhibit != null ? log.Exhibit.ExhibitTranslations.FirstOrDefault()!.Title : null
                })

                .Select(g => new ExhibitScanStatDto
                {
                    ExhibitId = g.Key.ExhibitId ?? 0,
                    ExhibitName = g.Key.ExhibitTitle ?? "Unnamed Exhibit",
                    ScanCount = g.Count()
                })
                .OrderByDescending(x => x.ScanCount)
                .ToListAsync();
        }

        // 2. Lấy danh sách bảng xếp hạng hiện vật phổ biến nhất (FR4.1.2 & FR4.1.3)
        public async Task<List<PopularExhibitDto>> GetPopularExhibitsAsync(int museumId, int topCount)
        {
            return await _context.AnalyticsLogs
                .Where(log => log.MuseumId == museumId && log.ExhibitId != null)
                .GroupBy(log => new
                {
                    log.ExhibitId,
                    ExhibitTitle = log.Exhibit != null ? log.Exhibit.ExhibitTranslations.FirstOrDefault()!.Title : null
                })
                .Select(g => new PopularExhibitDto
                {
                    ExhibitId = g.Key.ExhibitId ?? 0,
                    ExhibitName = g.Key.ExhibitTitle ?? "Unnamed Exhibit",
                    TotalInteractions = g.Count(),
                    // ĐÃ SỬA: Dùng ListeningDuration khớp hoàn toàn với thực thể DB của bạn
                    AvgDurationSeconds = g.Average(l => l.ListeningDuration) ?? 0
                })
                .OrderByDescending(x => x.TotalInteractions)
                .Take(topCount)
                .ToListAsync();
        }

        // 3. Thống kê tỷ lệ sử dụng ngôn ngữ của khách tham quan (FR4.1.4)
        public async Task<List<LanguageUsageDto>> GetLanguageUsageStatsAsync(int museumId)
        {
            // Lấy danh sách chứa VisitorId và LanguageUsed của action mới nhất có chỉ định ngôn ngữ cho từng user
            var latestUserLanguages = await _context.AnalyticsLogs
                .Where(log => log.MuseumId == museumId && log.VisitorId != null && log.LanguageUsed != null)
                .GroupBy(log => log.VisitorId)
                .Select(g => new
                {
                    VisitorId = g.Key,
                    LanguageUsed = g.OrderByDescending(l => l.EventTimestamp).ThenByDescending(l => l.Id).Select(l => l.LanguageUsed).FirstOrDefault()
                })
                .ToListAsync();

            if (latestUserLanguages.Count == 0) return new List<LanguageUsageDto>();

            var totalUsers = latestUserLanguages.Count;

            return latestUserLanguages
                .GroupBy(x => x.LanguageUsed)
                .Select(g => new LanguageUsageDto
                {
                    LanguageCode = g.Key!,
                    UsageCount = g.Count(),
                    Percentage = (double)g.Count() / totalUsers * 100
                })
                .OrderByDescending(x => x.UsageCount)
                .ToList();
        }

        // 4. Đếm tổng số lượt tải trọn gói dữ liệu offline (FR4.1.4)
        public async Task<int> GetTotalOfflineDownloadsAsync(int museumId)
        {
            return await _context.AnalyticsLogs
                .Where(log => log.MuseumId == museumId && log.ActionType == "PACKAGE_DOWNLOAD") // ĐÃ SỬA: Khớp CHECK constraint PACKAGE_DOWNLOAD
                .CountAsync();
        }
    }
}