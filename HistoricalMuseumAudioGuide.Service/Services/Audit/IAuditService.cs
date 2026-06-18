using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Audit;

public interface IAuditService
{
    Task LogActionAsync(int? userId, string action, string entityType, string? newValues, string? ipAddress, string? userAgent);
}
