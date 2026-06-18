using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HistoricalMuseumAudioGuide.Service.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AnalyticsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> RecordActionAsync(int? visitorId, CreateAnalyticsLogDto dto)
    {
        var log = _mapper.Map<AnalyticsLog>(dto);
        log.VisitorId = visitorId;
        log.EventTimestamp = DateTime.UtcNow;
        log.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.AnalyticsLogs.AddAsync(log);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Action recorded successfully.");
    }

    public async Task<ResponseModel> GetDashboardStatsAsync(int museumId)
    {
        var exhibits = await _unitOfWork.Exhibits.FindAsync(e => e.MuseumId == museumId);
        var totalExhibits = exhibits.Count();

        // In a real scenario, this would use specialized queries for aggregation.
        // For demonstration, we use in-memory filtering.
        var allLogs = await _unitOfWork.AnalyticsLogs.FindAsync(l => l.MuseumId == museumId);
        
        int totalArScans = allLogs.Count(l => l.ActionType == "ScanAR");
        int totalAudioPlays = allLogs.Count(l => l.ActionType == "PlayAudio");

        // Transactions logic
        // This requires accessing the DbContext directly or adding a specialized method
        // For brevity in this generic implementation:
        int totalTicketsSold = 0;
        decimal totalRevenue = 0;
        
        // Mock data for tickets if repository methods for aggregation aren't fully fleshed out yet.
        // Ideally: await _unitOfWork.Transactions.GetTotalRevenueAsync(museumId);

        var stats = new DashboardStatsDto
        {
            TotalExhibits = totalExhibits,
            TotalArScans = totalArScans,
            TotalAudioPlays = totalAudioPlays,
            TotalTicketsSold = totalTicketsSold,
            TotalRevenue = totalRevenue
        };

        return ResponseModel.Success("Dashboard stats retrieved successfully", stats);
    }
}
