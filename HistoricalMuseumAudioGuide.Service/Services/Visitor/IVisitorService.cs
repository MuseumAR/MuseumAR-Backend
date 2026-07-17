using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Visitor;

public interface IVisitorService
{
    Task<ResponseModel> GetLatestOfflinePackageAsync(int museumId);
    Task<ResponseModel> GetVisitorProfileAsync(int visitorId);
    Task<ResponseModel> GetBookmarksAsync(int visitorId);
    Task<ResponseModel> AddBookmarkAsync(int visitorId, CreateBookmarkDto dto);
    Task<ResponseModel> RemoveBookmarkAsync(int visitorId, int exhibitId);
    Task<ResponseModel> GetVisitedExhibitsAsync(int visitorId);
    Task<ResponseModel> TrackVisitedExhibitAsync(int visitorId, CreateVisitedExhibitDto dto);
    Task<ResponseModel> SyncVisitorAsync(VisitorSyncDto dto, int? userId);
    Task<ResponseModel> GetVisitorByUserIdAsync(int userId);
}
