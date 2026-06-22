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
}
