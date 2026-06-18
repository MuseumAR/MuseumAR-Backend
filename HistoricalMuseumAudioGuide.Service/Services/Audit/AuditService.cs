using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Audit;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogActionAsync(int? userId, string action, string entityType, string? details, string? ipAddress, string? userAgent)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogs.AddAsync(log);
        await _unitOfWork.CompleteAsync();
    }
}
