using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics
{
    // DTO tổng hợp cho toàn bộ Dashboard chính
    public class MuseumDashboardDto
    {
        public int TotalQrScans { get; set; }
        public double AverageListeningDurationMinutes { get; set; }
        public int TotalOfflineDownloads { get; set; }

        // Dữ liệu cho biểu đồ cột: Thống kê lượt quét QR theo từng hiện vật (FR4.1.1)
        public List<ExhibitScanStatDto> ExhibitScanStats { get; set; } = new();

        // Dữ liệu cho bảng xếp hạng: Top hiện vật phổ biến (FR4.1.2)
        public List<PopularExhibitDto> PopularExhibits { get; set; } = new();

        // Dữ liệu cho biểu đồ tròn: Tỷ lệ phân bố ngôn ngữ được sử dụng (FR4.1.4)
        public List<LanguageUsageDto> LanguageUsageStats { get; set; } = new();
    }

    public class ExhibitScanStatDto
    {
        public int ExhibitId { get; set; }
        public string ExhibitName { get; set; } = string.Empty;
        public int ScanCount { get; set; }
    }

    public class PopularExhibitDto
    {
        public int ExhibitId { get; set; }
        public string ExhibitName { get; set; } = string.Empty;
        public int TotalInteractions { get; set; } // Tổng số lượt quét + lượt nghe
        public double AvgDurationSeconds { get; set; }
    }

    public class LanguageUsageDto
    {
        public string LanguageCode { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public double Percentage { get; set; }
    }
}