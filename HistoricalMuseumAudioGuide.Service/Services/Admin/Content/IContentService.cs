using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin.Content
{
    public interface IContentService
    {
        // Exhibit Management
        Task<ResponseModel> GetAllExhibitsAsync(int museumId);
        Task<ResponseModel> GetExhibitByIdAsync(int id);
        Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto exhibitDto);
        Task<ResponseModel> UpdateExhibitAsync(int id, CreateExhibitDto exhibitDto);
        Task<ResponseModel> DeleteExhibitAsync(int id);
        Task<ResponseModel> PublishExhibitAsync(int id);
        Task<ResponseModel> UnpublishExhibitAsync(int id);

        // Media Management
        Task<ResponseModel> UploadExhibitImageAsync(int exhibitId, IFormFile file, string caption);
        Task<ResponseModel> UploadExhibitAudioAsync(int exhibitId, string languageCode, IFormFile file);

        // Translation Management
        Task<ResponseModel> GetExhibitTranslationsAsync(int exhibitId);
        Task<ResponseModel> AddOrUpdateExhibitTranslationAsync(int exhibitId, ExhibitTranslationDto translationDto);

        // Category Management
        Task<ResponseModel> GetCategoriesByMuseumIdAsync(int museumId);

        // Content Version Management
        Task<ResponseModel> GetContentVersionsAsync(int museumId);
        Task<ResponseModel> CreateNewContentVersionAsync(int museumId, string versionNumber, string description);

        // AR Asset Management
        Task<ResponseModel> GetArAssetsByExhibitIdAsync(int exhibitId);
        Task<ResponseModel> AddArAssetAsync(int exhibitId, string assetType, IFormFile file, string? description);
        Task<ResponseModel> DeleteArAssetAsync(int id);

        // Offline Package Management
        Task<ResponseModel> GenerateOfflinePackageAsync(int museumId, int versionId);
        Task<ResponseModel> GetOfflinePackagesByMuseumIdAsync(int museumId);
    }
}
