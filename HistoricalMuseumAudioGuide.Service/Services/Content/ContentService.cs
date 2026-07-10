using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ContentVersion;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.AgeGroup;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Media;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Content
{
    public class ContentService : IContentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediaService _mediaService;

        public ContentService(IUnitOfWork unitOfWork, IMapper mapper, IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediaService = mediaService;
        }

        /// <summary>
        /// Validates that the user belongs to the specified museum.
        /// Returns null if access is allowed, or a Forbidden ResponseModel if denied.
        /// SystemAdmin (userMuseumId == null) always has access.
        /// </summary>
        private static ResponseModel? ValidateMuseumAccess(int? userMuseumId, int? resourceMuseumId)
        {
            if (userMuseumId.HasValue)
            {
                if (!resourceMuseumId.HasValue || userMuseumId.Value != resourceMuseumId.Value)
                {
                    return ResponseModel.Forbidden("You do not have permission to manage resources of this museum.");
                }
            }
            return null;
        }

        // --- Exhibit Management ---

        public async Task<ResponseModel> GetAllExhibitsAsync(int museumId)
        {
            var exhibits = await _unitOfWork.Exhibits.GetExhibitsWithTranslationsAndMetadataAsync(museumId);
            var exhibitDtos = _mapper.Map<IEnumerable<ExhibitDto>>(exhibits);
            return ResponseModel.Success("Get all exhibits successful", exhibitDtos);
        }

        public async Task<ResponseModel> GetExhibitByIdAsync(int id)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == id,
                includeProperties: "ExhibitTranslations,ExhibitMetadatum"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var exhibitDto = _mapper.Map<ExhibitDto>(exhibit);
            return ResponseModel.Success("Get exhibit successful", exhibitDto);
        }

        public async Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto exhibitDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibitDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (exhibitDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(exhibitDto.CategoryId.Value);
                if (category == null)
                {
                    return ResponseModel.BadRequest("Category not found.");
                }
                if (category.MuseumId.HasValue && category.MuseumId.Value != exhibitDto.MuseumId)
                {
                    return ResponseModel.BadRequest("Category does not belong to the specified museum.");
                }
            }

            var exhibit = _mapper.Map<Exhibit>(exhibitDto);
            exhibit.CreatedAt = DateTime.UtcNow;
            exhibit.UpdatedAt = DateTime.UtcNow;
            exhibit.Status = "Draft";

            await _unitOfWork.Exhibits.AddAsync(exhibit);
            await _unitOfWork.CompleteAsync();

            if (exhibitDto.Translations != null && exhibitDto.Translations.Count > 0)
            {
                foreach (var transDto in exhibitDto.Translations)
                {
                    var translation = _mapper.Map<ExhibitTranslation>(transDto);
                    translation.ExhibitId = exhibit.Id;
                    await _unitOfWork.ExhibitTranslations.AddAsync(translation);
                }
                await _unitOfWork.CompleteAsync();
            }

            if (exhibitDto.ExhibitMetadata != null)
            {
                var metadata = _mapper.Map<ExhibitMetadatum>(exhibitDto.ExhibitMetadata);
                metadata.ExhibitId = exhibit.Id;
                await _unitOfWork.ExhibitMetadata.AddAsync(metadata);
                await _unitOfWork.CompleteAsync();
            }

            return ResponseModel.Success("Exhibit created successfully", exhibit.Id);
        }

        public async Task<ResponseModel> UpdateExhibitAsync(int id, CreateExhibitDto exhibitDto, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (exhibitDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(exhibitDto.CategoryId.Value);
                if (category == null)
                {
                    return ResponseModel.BadRequest("Category not found.");
                }
                if (category.MuseumId.HasValue && category.MuseumId.Value != exhibit.MuseumId)
                {
                    return ResponseModel.BadRequest("Category does not belong to the same museum.");
                }
            }

            _mapper.Map(exhibitDto, exhibit);
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            if (exhibitDto.ExhibitMetadata != null)
            {
                var existingMetadata = await _unitOfWork.ExhibitMetadata.GetByIdAsync(id);
                if (existingMetadata == null)
                {
                    var metadata = _mapper.Map<ExhibitMetadatum>(exhibitDto.ExhibitMetadata);
                    metadata.ExhibitId = id;
                    await _unitOfWork.ExhibitMetadata.AddAsync(metadata);
                }
                else
                {
                    _mapper.Map(exhibitDto.ExhibitMetadata, existingMetadata);
                    _unitOfWork.ExhibitMetadata.Update(existingMetadata);
                }
                await _unitOfWork.CompleteAsync();
            }

            return ResponseModel.Success("Exhibit updated successfully");
        }

        public async Task<ResponseModel> DeleteExhibitAsync(int id, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            exhibit.Status = "Archived";
            exhibit.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit deleted (archived) successfully");
        }

        public async Task<ResponseModel> PublishExhibitAsync(int id, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            exhibit.Status = "Published";
            exhibit.PublishedAt = DateTime.UtcNow;
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit published successfully");
        }

        public async Task<ResponseModel> UnpublishExhibitAsync(int id, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            exhibit.Status = "Unpublished";
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit unpublished successfully");
        }

        // --- Exhibition Management ---

        public async Task<ResponseModel> GetExhibitionsByMuseumIdAsync(int museumId)
        {
            var exhibitions = await _unitOfWork.Exhibitions.FindAsync(e => e.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<ExhibitionDto>>(exhibitions);
            return ResponseModel.Success("Exhibitions retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateExhibitionAsync(CreateExhibitionDto createExhibitionDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, createExhibitionDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            var exhibition = _mapper.Map<Exhibition>(createExhibitionDto);
            await _unitOfWork.Exhibitions.AddAsync(exhibition);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<ExhibitionDto>(exhibition);
            return ResponseModel.Success("Exhibition created successfully", dto);
        }

        // --- Map Management ---

        public async Task<ResponseModel> GetMuseumMapsAsync(int museumId)
        {
            var maps = await _unitOfWork.MuseumMaps.FindAsync(m => m.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<MuseumMapDto>>(maps);
            return ResponseModel.Success("Maps retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateMuseumMapAsync(CreateMuseumMapDto mapDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, mapDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (mapDto.MapImage == null || mapDto.MapImage.Length == 0)
            {
                return ResponseModel.BadRequest("Map image file is required.");
            }

            var imageUrl = await _mediaService.UploadFileAsync(mapDto.MapImage, "maps");

            var map = new MuseumMap
            {
                MuseumId = mapDto.MuseumId,
                MapImageUrl = imageUrl,
                FloorNumber = 1,
                MapName = mapDto.MapType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MuseumMaps.AddAsync(map);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<MuseumMapDto>(map);
            return ResponseModel.Success("Map created successfully", dto);
        }

        // --- Tour Route Management ---

        public async Task<ResponseModel> GetTourRoutesAsync(int museumId)
        {
            var routes = await _unitOfWork.TourRoutes.FindAsync(r => r.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<TourRouteDto>>(routes);
            return ResponseModel.Success("Tour routes retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateTourRouteAsync(CreateTourRouteDto routeDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, routeDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            var route = _mapper.Map<TourRoute>(routeDto);
            route.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.TourRoutes.AddAsync(route);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<TourRouteDto>(route);
            return ResponseModel.Success("Tour route created successfully", dto);
        }

        // --- Media Management ---

        public async Task<ResponseModel> UploadExhibitImageAsync(int exhibitId, IFormFile file, string caption, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            var fileUrl = await _mediaService.UploadFileAsync(file, "exhibits");

            var exhibitImage = new ExhibitImage
            {
                ExhibitId = exhibitId,
                ImageUrl = fileUrl,
                Caption = caption,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ExhibitImages.AddAsync(exhibitImage);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Image uploaded successfully", fileUrl);
        }

        public async Task<ResponseModel> UploadExhibitAudioAsync(int exhibitId, string languageCode, IFormFile file, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            var translation = await _unitOfWork.ExhibitTranslations.GetTranslationAsync(exhibitId, languageCode);
            if (translation == null) return ResponseModel.NotFound("Exhibit translation for this language not found");

            if (!string.IsNullOrEmpty(translation.AudioUrl))
            {
                _mediaService.DeleteFile(translation.AudioUrl);
            }

            var fileUrl = await _mediaService.UploadFileAsync(file, "audio");
            translation.AudioUrl = fileUrl;

            _unitOfWork.ExhibitTranslations.Update(translation);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Audio guide uploaded successfully", fileUrl);
        }

        public async Task<ResponseModel> GetExhibitTranslationsAsync(int exhibitId)
        {
            var translations = await _unitOfWork.ExhibitTranslations.GetTranslationsByExhibitIdAsync(exhibitId);
            var translationDtos = _mapper.Map<IEnumerable<ExhibitTranslationDto>>(translations);
            return ResponseModel.Success("Get translations successful", translationDtos);
        }

        public async Task<ResponseModel> AddOrUpdateExhibitTranslationAsync(int exhibitId, ExhibitTranslationDto translationDto, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            var existingTranslation = await _unitOfWork.ExhibitTranslations.GetTranslationAsync(exhibitId, translationDto.LanguageCode);

            if (existingTranslation == null)
            {
                var translation = _mapper.Map<ExhibitTranslation>(translationDto);
                translation.ExhibitId = exhibitId;
                await _unitOfWork.ExhibitTranslations.AddAsync(translation);
            }
            else
            {
                _mapper.Map(translationDto, existingTranslation);
                existingTranslation.ExhibitId = exhibitId;
                _unitOfWork.ExhibitTranslations.Update(existingTranslation);
            }

            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Translation added/updated successfully");
        }

        public async Task<ResponseModel> GetCategoriesAsync(int? museumId)
        {
            var categories = await _unitOfWork.Categories.GetCategoriesWithTranslationsAsync(museumId);
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return ResponseModel.Success("Get categories successful", dtos);
        }

        public async Task<ResponseModel> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithTranslationsByIdAsync(id);
            if (category == null) return ResponseModel.NotFound("Category not found");

            var dto = _mapper.Map<CategoryDto>(category);
            return ResponseModel.Success("Get category successful", dto);
        }

        public async Task<ResponseModel> CreateCategoryAsync(CreateCategoryDto categoryDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, categoryDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            var category = _mapper.Map<Category>(categoryDto);
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<CategoryDto>(category);
            return ResponseModel.Success("Category created successfully", resultDto);
        }

        public async Task<ResponseModel> UpdateCategoryAsync(int id, CreateCategoryDto categoryDto, int? userMuseumId)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithTranslationsByIdAsync(id);
            if (category == null) return ResponseModel.NotFound("Category not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, category.MuseumId);
            if (accessCheck != null) return accessCheck;

            category.ParentId = categoryDto.ParentId;
            category.SortOrder = categoryDto.SortOrder;
            category.IconUrl = categoryDto.IconUrl;
            category.Status = categoryDto.Status;
            category.UpdatedAt = DateTime.UtcNow;

            if (categoryDto.CategoryTranslations != null)
            {
                var incomingLangCodes = categoryDto.CategoryTranslations.Select(t => t.LanguageCode).ToList();
                var translationsToRemove = category.CategoryTranslations
                    .Where(t => !incomingLangCodes.Contains(t.LanguageCode)).ToList();
                foreach (var trans in translationsToRemove)
                {
                    category.CategoryTranslations.Remove(trans);
                }

                foreach (var transDto in categoryDto.CategoryTranslations)
                {
                    var existingTrans = category.CategoryTranslations
                        .FirstOrDefault(t => t.LanguageCode == transDto.LanguageCode);
                    if (existingTrans != null)
                    {
                        existingTrans.CategoryName = transDto.CategoryName;
                        existingTrans.Description = transDto.Description;
                    }
                    else
                    {
                        var newTrans = _mapper.Map<CategoryTranslation>(transDto);
                        newTrans.CategoryId = id;
                        category.CategoryTranslations.Add(newTrans);
                    }
                }
            }

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Category updated successfully");
        }

        public async Task<ResponseModel> DeleteCategoryAsync(int id, int? userMuseumId)
        {
            var category = await _unitOfWork.Categories.GetCategoryWithTranslationsByIdAsync(id);
            if (category == null) return ResponseModel.NotFound("Category not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, category.MuseumId);
            if (accessCheck != null) return accessCheck;

            category.Status = "Inactive";
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Category deleted (deactivated) successfully");
        }

        public async Task<ResponseModel> GetThemesAsync(int? museumId)
        {
            IEnumerable<Theme> themes;
            if (museumId.HasValue)
            {
                themes = await _unitOfWork.Themes.FindAsync(t => t.MuseumId == null || t.MuseumId == museumId.Value);
            }
            else
            {
                themes = await _unitOfWork.Themes.FindAsync(t => t.MuseumId == null);
            }
            var dtos = _mapper.Map<IEnumerable<ThemeDto>>(themes);
            return ResponseModel.Success("Get themes successful", dtos);
        }

        public async Task<ResponseModel> GetThemeByIdAsync(int id)
        {
            var theme = await _unitOfWork.Themes.GetByIdAsync(id);
            if (theme == null) return ResponseModel.NotFound("Theme not found");
            var dto = _mapper.Map<ThemeDto>(theme);
            return ResponseModel.Success("Get theme successful", dto);
        }

        public async Task<ResponseModel> CreateThemeAsync(CreateThemeDto themeDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, themeDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            var theme = _mapper.Map<Theme>(themeDto);
            theme.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Themes.AddAsync(theme);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<ThemeDto>(theme);
            return ResponseModel.Success("Theme created successfully", dto);
        }

        public async Task<ResponseModel> UpdateThemeAsync(int id, CreateThemeDto themeDto, int? userMuseumId)
        {
            var theme = await _unitOfWork.Themes.GetByIdAsync(id);
            if (theme == null) return ResponseModel.NotFound("Theme not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, theme.MuseumId);
            if (accessCheck != null) return accessCheck;

            _mapper.Map(themeDto, theme);
            _unitOfWork.Themes.Update(theme);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Theme updated successfully");
        }

        public async Task<ResponseModel> DeleteThemeAsync(int id, int? userMuseumId)
        {
            var theme = await _unitOfWork.Themes.GetByIdAsync(id);
            if (theme == null) return ResponseModel.NotFound("Theme not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, theme.MuseumId);
            if (accessCheck != null) return accessCheck;
            _unitOfWork.Themes.Delete(theme);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Theme deleted successfully");
        }

        public async Task<ResponseModel> GetAllAgeGroupsAsync()
        {
            var ageGroups = await _unitOfWork.AgeGroups.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<AgeGroupDto>>(ageGroups);
            return ResponseModel.Success("Get age groups successful", dtos);
        }

        public async Task<ResponseModel> GetContentVersionsAsync(int museumId)
        {
            var versions = await _unitOfWork.ContentVersions.FindAsync(v => v.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<ContentVersionDto>>(versions);
            return ResponseModel.Success("Get content versions successful", dtos);
        }

        public async Task<ResponseModel> CreateNewContentVersionAsync(int museumId, string versionNumber, string description, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, museumId);
            if (accessCheck != null) return accessCheck;

            var version = new ContentVersion
            {
                MuseumId = museumId,
                VersionNumber = versionNumber,
                ChangeDescription = description,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ContentVersions.AddAsync(version);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("New content version created successfully", version.Id);
        }

        // --- AR Asset Management ---

        public async Task<ResponseModel> GetArAssetsByExhibitIdAsync(int exhibitId)
        {
            var assets = await _unitOfWork.ExhibitArassets.GetArAssetsByExhibitIdAsync(exhibitId);
            var assetDtos = _mapper.Map<IEnumerable<ExhibitArassetDto>>(assets);
            return ResponseModel.Success("Get AR assets successful", assetDtos);
        }

        public async Task<ResponseModel> AddArAssetAsync(int exhibitId, string assetType, IFormFile file, string? description, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            var fileUrl = await _mediaService.UploadFileAsync(file, "ar");

            var asset = new ExhibitArasset
            {
                ExhibitId = exhibitId,
                AssetType = assetType,
                AssetUrl = fileUrl,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ExhibitArassets.AddAsync(asset);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("AR Asset uploaded and added successfully", asset.Id);
        }

        public async Task<ResponseModel> DeleteArAssetAsync(int id, int? userMuseumId)
        {
            var asset = await _unitOfWork.ExhibitArassets.GetFirstOrDefaultAsync(
                a => a.Id == id,
                includeProperties: "Exhibit"
            );
            if (asset == null) return ResponseModel.NotFound("AR Asset not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, asset.Exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            _mediaService.DeleteFile(asset.AssetUrl);

            _unitOfWork.ExhibitArassets.Delete(asset);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("AR Asset deleted successfully");
        }

        // --- Offline Package Management ---

        public async Task<ResponseModel> GenerateOfflinePackageAsync(int museumId, int versionId, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, museumId);
            if (accessCheck != null) return accessCheck;

            var package = new OfflinePackage
            {
                MuseumId = museumId,
                VersionId = versionId,
                Status = "Building",
                CreatedAt = DateTime.UtcNow,
                PackageSizeBytes = 0 // Mock size
            };

            await _unitOfWork.OfflinePackages.AddAsync(package);
            await _unitOfWork.CompleteAsync();

            // Mock update
            package.Status = "Available";
            package.PackageUrl = $"/uploads/packages/museum_{museumId}_v{versionId}.zip";
            package.Checksum = Guid.NewGuid().ToString("N");
            
            _unitOfWork.OfflinePackages.Update(package);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Offline package generated successfully", package.Id);
        }

        public async Task<ResponseModel> GetOfflinePackagesByMuseumIdAsync(int museumId)
        {
            var packages = await _unitOfWork.OfflinePackages.GetPackagesByMuseumIdAsync(museumId);
            var packageDtos = _mapper.Map<IEnumerable<OfflinePackageDto>>(packages);
            return ResponseModel.Success("Get offline packages successful", packageDtos);
        }

        // --- Tag Management ---

        public async Task<ResponseModel> GetTagGroupsAsync()
        {
            var tagGroups = await _unitOfWork.TagGroups.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TagGroupDto>>(tagGroups);
            return ResponseModel.Success("Get tag groups successful", dtos);
        }

        public async Task<ResponseModel> CreateTagGroupAsync(CreateTagGroupDto tagGroupDto)
        {
            var tagGroup = _mapper.Map<TagGroup>(tagGroupDto);
            tagGroup.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.TagGroups.AddAsync(tagGroup);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<TagGroupDto>(tagGroup);
            return ResponseModel.Success("Tag group created successfully", dto);
        }

        public async Task<ResponseModel> UpdateTagGroupAsync(int id, CreateTagGroupDto tagGroupDto)
        {
            var tagGroup = await _unitOfWork.TagGroups.GetByIdAsync(id);
            if (tagGroup == null) return ResponseModel.NotFound("Tag group not found");

            _mapper.Map(tagGroupDto, tagGroup);
            _unitOfWork.TagGroups.Update(tagGroup);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tag group updated successfully");
        }

        public async Task<ResponseModel> DeleteTagGroupAsync(int id)
        {
            var tagGroup = await _unitOfWork.TagGroups.GetByIdAsync(id);
            if (tagGroup == null) return ResponseModel.NotFound("Tag group not found");

            _unitOfWork.TagGroups.Delete(tagGroup);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tag group deleted successfully");
        }

        public async Task<ResponseModel> GetTagsByGroupAsync(int tagGroupId)
        {
            var tags = await _unitOfWork.Tags.FindAsync(t => t.TagGroupId == tagGroupId);
            var dtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            return ResponseModel.Success("Get tags successful", dtos);
        }

        public async Task<ResponseModel> GetAllTagsAsync()
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            return ResponseModel.Success("Get all tags successful", dtos);
        }

        public async Task<ResponseModel> CreateTagAsync(CreateTagDto tagDto)
        {
            var tagGroup = await _unitOfWork.TagGroups.GetByIdAsync(tagDto.TagGroupId);
            if (tagGroup == null) return ResponseModel.BadRequest("Tag group not found");

            var tag = _mapper.Map<Tag>(tagDto);
            tag.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<TagDto>(tag);
            return ResponseModel.Success("Tag created successfully", dto);
        }

        public async Task<ResponseModel> UpdateTagAsync(int id, CreateTagDto tagDto)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) return ResponseModel.NotFound("Tag not found");

            _mapper.Map(tagDto, tag);
            _unitOfWork.Tags.Update(tag);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tag updated successfully");
        }

        public async Task<ResponseModel> DeleteTagAsync(int id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) return ResponseModel.NotFound("Tag not found");

            _unitOfWork.Tags.Delete(tag);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tag deleted successfully");
        }

        public async Task<ResponseModel> AssignTagsToExhibitAsync(int exhibitId, List<int> tagIds, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == exhibitId,
                includeProperties: "Tags"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            foreach (var tagId in tagIds)
            {
                if (exhibit.Tags.Any(t => t.Id == tagId)) continue;
                var tag = await _unitOfWork.Tags.GetByIdAsync(tagId);
                if (tag == null) continue;
                exhibit.Tags.Add(tag);
            }

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tags assigned to exhibit successfully");
        }

        public async Task<ResponseModel> RemoveTagFromExhibitAsync(int exhibitId, int tagId, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == exhibitId,
                includeProperties: "Tags"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            var tagToRemove = exhibit.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tagToRemove == null) return ResponseModel.NotFound("Tag not found on this exhibit");

            exhibit.Tags.Remove(tagToRemove);
            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Tag removed from exhibit successfully");
        }

        public async Task<ResponseModel> GetExhibitTagsAsync(int exhibitId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == exhibitId,
                includeProperties: "Tags"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var dtos = _mapper.Map<IEnumerable<TagDto>>(exhibit.Tags);
            return ResponseModel.Success("Get exhibit tags successful", dtos);
        }
    }
}
