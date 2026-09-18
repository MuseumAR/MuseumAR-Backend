using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;

public class CreateAnalyticsLogDto
{
    public int MuseumId { get; set; }
    public int? ExhibitId { get; set; }
    public string ActionType { get; set; } = null!; // e.g., "ScanAR", "PlayAudio", "Search"
    public int? ListeningDuration { get; set; }
    public string? LanguageUsed { get; set; }
    public string? DeviceType { get; set; }
    public string? SearchQuery { get; set; }
}

public class DashboardStatsDto
{
    public int TotalExhibits { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalArScans { get; set; }
    public int TotalAudioPlays { get; set; }
}

public class RevenueAnalyticsDto
{
    public decimal TotalGrossRevenue { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalTicketsSold { get; set; }
    public int TotalUsedTickets { get; set; }
    public int TotalRefundedTickets { get; set; }
    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<RevenueByTicketTypeDto> RevenueByTicketType { get; set; } = new();
}

public class DailySalesDto
{
    public string Date { get; set; } = null!; // "yyyy-MM-dd"
    public decimal GrossRevenue { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public int TicketsSold { get; set; }
}

public class RevenueByTicketTypeDto
{
    public int TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = null!;
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class VisitorTrafficDto
{
    public int TotalAdmittedVisitors { get; set; }
    public int TotalTicketsSold { get; set; }
    public double AttendanceRate { get; set; } // percentage e.g. 75.5%
    public List<DailyFootfallDto> DailyFootfall { get; set; } = new();
    public List<PeakHourTrafficDto> PeakHoursTraffic { get; set; } = new();
}

public class DailyFootfallDto
{
    public string Date { get; set; } = null!; // "yyyy-MM-dd"
    public int VisitorCount { get; set; }
}

public class PeakHourTrafficDto
{
    public int Hour { get; set; } // 8 to 18
    public string HourLabel { get; set; } = null!; // "08:00 - 09:00"
    public int VisitorCount { get; set; }
}
