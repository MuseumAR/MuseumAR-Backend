using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin.Content
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

        public async Task<ResponseModel> GetAllExhibitsAsync(int museumId)
        {
            var exhibits = await _unitOfWork.Exhibits.FindAsync(e => e.MuseumId == museumId);
            var exhibitDtos = _mapper.Map<IEnumerable<ExhibitDto>>(exhibits);
            return ResponseModel.Success("Get all exhibits successful", exhibitDtos);
        }

        public async Task<ResponseModel> GetExhibitByIdAsync(int id)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var exhibitDto = _mapper.Map<ExhibitDto>(exhibit);
            return ResponseModel.Success("Get exhibit successful", exhibitDto);
        }

        public async Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto exhibitDto)
        {
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

            return ResponseModel.Success("Exhibit created successfully", exhibit.Id);
        }

        public async Task<ResponseModel> UpdateExhibitAsync(int id, CreateExhibitDto exhibitDto)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            _mapper.Map(exhibitDto, exhibit);
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit updated successfully");
        }

        public async Task<ResponseModel> DeleteExhibitAsync(int id)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            exhibit.Status = "Archived";
            exhibit.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit deleted (archived) successfully");
        }

        public async Task<ResponseModel> PublishExhibitAsync(int id)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            exhibit.Status = "Published";
            exhibit.PublishedAt = DateTime.UtcNow;
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit published successfully");
        }

        public async Task<ResponseModel> UnpublishExhibitAsync(int id)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(id);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            exhibit.Status = "Unpublished";
            exhibit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibit unpublished successfully");
        }

        // --- Media Management ---

        public async Task<ResponseModel> UploadExhibitImageAsync(int exhibitId, IFormFile file, string caption)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

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

        public async Task<ResponseModel> UploadExhibitAudioAsync(int exhibitId, string languageCode, IFormFile file)
        {
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

        public async Task<ResponseModel> AddOrUpdateExhibitTranslationAsync(int exhibitId, ExhibitTranslationDto translationDto)
        {
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

        public async Task<ResponseModel> GetCategoriesByMuseumIdAsync(int museumId)
        {
            var categories = await _unitOfWork.Categories.FindAsync(c => c.MuseumId == museumId);
            return ResponseModel.Success("Get categories successful", categories);
        }

        public async Task<ResponseModel> GetContentVersionsAsync(int museumId)
        {
            var versions = await _unitOfWork.ContentVersions.FindAsync(v => v.MuseumId == museumId);
            return ResponseModel.Success("Get content versions successful", versions);
        }

        public async Task<ResponseModel> CreateNewContentVersionAsync(int museumId, string versionNumber, string description)
        {
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

        public async Task<ResponseModel> AddArAssetAsync(int exhibitId, string assetType, IFormFile file, string? description)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

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

        public async Task<ResponseModel> DeleteArAssetAsync(int id)
        {
            var asset = await _unitOfWork.ExhibitArassets.GetByIdAsync(id);
            if (asset == null) return ResponseModel.NotFound("AR Asset not found");

            _mediaService.DeleteFile(asset.AssetUrl);

            _unitOfWork.ExhibitArassets.Delete(asset);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("AR Asset deleted successfully");
        }

        // --- Offline Package Management ---

        public async Task<ResponseModel> GenerateOfflinePackageAsync(int museumId, int versionId)
        {
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
    }
}
