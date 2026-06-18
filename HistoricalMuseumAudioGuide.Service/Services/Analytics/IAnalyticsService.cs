using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Analytics;

public interface IAnalyticsService
{
    Task<ResponseModel> RecordActionAsync(int? visitorId, CreateAnalyticsLogDto dto);
    Task<ResponseModel> GetDashboardStatsAsync(int museumId);
}
