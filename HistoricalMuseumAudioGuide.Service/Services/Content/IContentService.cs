using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;

namespace HistoricalMuseumAudioGuide.Service.Services.Content
{
    public interface IContentService
    {
        // Exhibit Management
        Task<ResponseModel> GetAllExhibitsAsync(int museumId);
        Task<ResponseModel> GetExhibitByIdAsync(int id);
        Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto exhibitDto, int? userMuseumId);
        Task<ResponseModel> UpdateExhibitAsync(int id, CreateExhibitDto exhibitDto, int? userMuseumId);
        Task<ResponseModel> DeleteExhibitAsync(int id, int? userMuseumId);
        Task<ResponseModel> PublishExhibitAsync(int id, int? userMuseumId);
        Task<ResponseModel> UnpublishExhibitAsync(int id, int? userMuseumId);

        // Exhibition Management
        Task<ResponseModel> GetExhibitionsByMuseumIdAsync(int museumId);
        Task<ResponseModel> CreateExhibitionAsync(CreateExhibitionDto exhibitionDto, int? userMuseumId);

        // Map Management
        Task<ResponseModel> GetMuseumMapsAsync(int museumId);
        Task<ResponseModel> CreateMuseumMapAsync(CreateMuseumMapDto mapDto, int? userMuseumId);

        // Tour Route Management
        Task<ResponseModel> GetTourRoutesAsync(int museumId);
        Task<ResponseModel> CreateTourRouteAsync(CreateTourRouteDto routeDto, int? userMuseumId);

        // Media Management
        Task<ResponseModel> UploadExhibitImageAsync(int exhibitId, IFormFile file, string caption, int? userMuseumId);
        Task<ResponseModel> UploadExhibitAudioAsync(int exhibitId, string languageCode, IFormFile file, int? userMuseumId);

        // Translation Management
        Task<ResponseModel> GetExhibitTranslationsAsync(int exhibitId);
        Task<ResponseModel> AddOrUpdateExhibitTranslationAsync(int exhibitId, ExhibitTranslationDto translationDto, int? userMuseumId);

        // Category Management
        Task<ResponseModel> GetCategoriesAsync(int? museumId);
        Task<ResponseModel> GetCategoryByIdAsync(int id);
        Task<ResponseModel> CreateCategoryAsync(CreateCategoryDto categoryDto, int? userMuseumId);
        Task<ResponseModel> UpdateCategoryAsync(int id, CreateCategoryDto categoryDto, int? userMuseumId);
        Task<ResponseModel> DeleteCategoryAsync(int id, int? userMuseumId);

        // Reference Metadata
        Task<ResponseModel> GetThemesAsync(int? museumId);
        Task<ResponseModel> GetThemeByIdAsync(int id);
        Task<ResponseModel> CreateThemeAsync(CreateThemeDto themeDto, int? userMuseumId);
        Task<ResponseModel> UpdateThemeAsync(int id, CreateThemeDto themeDto, int? userMuseumId);
        Task<ResponseModel> DeleteThemeAsync(int id, int? userMuseumId);
        Task<ResponseModel> GetAllAgeGroupsAsync();

        // Content Version Management
        Task<ResponseModel> GetContentVersionsAsync(int museumId);
        Task<ResponseModel> CreateNewContentVersionAsync(int museumId, string versionNumber, string description, int? userMuseumId);

        // AR Asset Management
        Task<ResponseModel> GetArAssetsByExhibitIdAsync(int exhibitId);
        Task<ResponseModel> AddArAssetAsync(int exhibitId, string assetType, IFormFile file, string? description, int? userMuseumId);
        Task<ResponseModel> DeleteArAssetAsync(int id, int? userMuseumId);

        // Offline Package Management
        Task<ResponseModel> GenerateOfflinePackageAsync(int museumId, int versionId, int? userMuseumId);
        Task<ResponseModel> GetOfflinePackagesByMuseumIdAsync(int museumId);
    }
}
