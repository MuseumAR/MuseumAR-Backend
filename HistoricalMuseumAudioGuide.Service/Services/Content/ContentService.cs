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
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Media;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Net.Http;
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

        public async Task<ResponseModel> GetAllExhibitsAsync(int museumId, bool includeUnpublished = false, string? lang = null)
        {
            var exhibits = await _unitOfWork.Exhibits.GetExhibitsWithTranslationsAndMetadataAsync(museumId);
            if (!includeUnpublished)
            {
                exhibits = exhibits.Where(e => e.Status == "Published");
            }
            var exhibitDtos = _mapper.Map<IEnumerable<ExhibitDto>>(exhibits).ToList();
            ApplyExhibitMetadataLanguage(exhibitDtos, lang);
            return ResponseModel.Success("Get all exhibits successful", exhibitDtos);
        }

        public async Task<ResponseModel> GetExhibitsPagedAsync(int museumId, int page, int pageSize, bool includeUnpublished, string? search, string? status, string? lang)
        {
            if (page < 1) page = 1;
            if (pageSize < 8) pageSize = 8;
            if (pageSize > 50) pageSize = 50;

            var (exhibits, totalCount) = await _unitOfWork.Exhibits.GetExhibitsPagedAsync(
                museumId, page, pageSize, includeUnpublished, search, status);

            string targetLang = string.IsNullOrWhiteSpace(lang) ? "vi" : lang.Trim().ToLower();

            var items = exhibits.Select(e =>
            {
                var translation = e.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.ToLower() == targetLang)
                    ?? e.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.ToLower() == "vi")
                    ?? e.ExhibitTranslations?.FirstOrDefault();

                bool hasArModel = (e.ExhibitArassets != null && e.ExhibitArassets.Any(a =>
                    string.Equals(a.AssetType, "Model3D", StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(a.AssetUrl) && a.AssetUrl.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))))
                    || (!string.IsNullOrEmpty(e.AroverlayUrl) && e.AroverlayUrl.EndsWith(".glb", StringComparison.OrdinalIgnoreCase));

                int arModelCount = e.ExhibitArassets?.Count(a =>
                    string.Equals(a.AssetType, "Model3D", StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(a.AssetUrl) && a.AssetUrl.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))) ?? 0;

                return new ExhibitListItemDto
                {
                    Id = e.Id,
                    ExhibitCode = e.ExhibitCode,
                    Status = e.Status,
                    Title = translation?.Title ?? e.ExhibitCode ?? $"Exhibit #{e.Id}",
                    ThumbnailUrl = e.ThumbnailUrl,
                    HasArModel = hasArModel,
                    ArModelCount = arModelCount,
                    HasAudio = e.ExhibitTranslations?.Any(t => !string.IsNullOrEmpty(t.AudioUrl)) == true,
                    HasQr = !string.IsNullOrEmpty(e.QrcodeData),
                    RoomId = e.RoomId,
                    RoomName = e.Room?.RoomName,
                    MapId = e.MapId,
                    FloorNumber = e.Room?.FloorNumber ?? e.Map?.FloorNumber
                };
            }).ToList();

            var result = new Repository.Data.DTOs.PagedResultDto<ExhibitListItemDto>
            {
                TotalItems = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Items = items
            };

            return ResponseModel.Success("Get exhibits successful", result);
        }

        public async Task<ResponseModel> GetExhibitByIdAsync(int id, bool includeUnpublished = false, string? lang = null)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == id,
                includeProperties: "ExhibitTranslations,ExhibitMetadatum,ExhibitArassets,Map,Room"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");
            if (!includeUnpublished && exhibit.Status != "Published")
            {
                return ResponseModel.NotFound("Exhibit not found");
            }

            var exhibitDto = _mapper.Map<ExhibitDto>(exhibit);
            ApplyExhibitMetadataLanguage(new[] { exhibitDto }, lang);
            return ResponseModel.Success("Get exhibit successful", exhibitDto);
        }

        public async Task<ResponseModel> GetExhibitByCodeAsync(string code, bool includeUnpublished = false, string? lang = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return ResponseModel.NotFound("Exhibit code is required");
            var exhibit = await _unitOfWork.Exhibits.GetExhibitByCodeAsync(code);
            if (exhibit == null) return ResponseModel.NotFound($"Exhibit with code '{code}' not found");
            if (!includeUnpublished && exhibit.Status != "Published")
            {
                return ResponseModel.NotFound($"Exhibit with code '{code}' not found");
            }

            var exhibitDto = _mapper.Map<ExhibitDto>(exhibit);
            ApplyExhibitMetadataLanguage(new[] { exhibitDto }, lang);
            return ResponseModel.Success("Get exhibit by code successful", exhibitDto);
        }

        public async Task<ResponseModel> GetExhibitStatsAsync(int museumId)
        {
            var exhibits = (await _unitOfWork.Exhibits.FindAsync(
                e => e.MuseumId == museumId,
                includeProperties: "ExhibitArassets"
            )).ToList();

            var stats = new Repository.Data.DTOs.Exhibit.ExhibitStatsDto
            {
                Total = exhibits.Count,
                Published = exhibits.Count(e => string.Equals(e.Status, "Published", StringComparison.OrdinalIgnoreCase)),
                Draft = exhibits.Count(e => string.Equals(e.Status, "Draft", StringComparison.OrdinalIgnoreCase)),
                WithArModel = exhibits.Count(e =>
                    (e.ExhibitArassets != null && e.ExhibitArassets.Any(a =>
                        string.Equals(a.AssetType, "Model3D", StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(a.AssetUrl) && a.AssetUrl.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))))
                    || (!string.IsNullOrEmpty(e.AroverlayUrl) && e.AroverlayUrl.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))),
                WithQr = exhibits.Count(e => !string.IsNullOrWhiteSpace(e.QrcodeData))
            };

            return ResponseModel.Success("Get exhibit stats successful", stats);
        }

        public async Task<ResponseModel> ScanExhibitQrAsync(string qrData, string? lang = "vi", int? visitorId = null)
        {
            if (string.IsNullOrWhiteSpace(qrData))
            {
                return ResponseModel.BadRequest("Mã QR dữ liệu không được để trống.");
            }

            var exhibit = await _unitOfWork.Exhibits.GetExhibitByQrDataAsync(qrData.Trim());
            if (exhibit == null || exhibit.Status != "Published")
            {
                return ResponseModel.NotFound("Không tìm thấy hiện vật phù hợp với mã QR này.");
            }

            string targetLang = string.IsNullOrWhiteSpace(lang) ? "vi" : lang.Trim().ToLower();

            var translation = exhibit.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.ToLower() == targetLang)
                ?? exhibit.ExhibitTranslations?.FirstOrDefault(t => t.LanguageCode.ToLower() == "vi")
                ?? exhibit.ExhibitTranslations?.FirstOrDefault();

            string title = translation?.Title ?? exhibit.ExhibitCode ?? $"Hiện vật #{exhibit.Id}";
            string description = translation?.Description ?? "Chưa có bài thuyết minh cho hiện vật này.";
            string? audioUrl = translation?.AudioUrl;

            var images = exhibit.ExhibitImages?.Select(i => i.ImageUrl).Where(u => !string.IsNullOrEmpty(u)).ToList() ?? new List<string>();
            if (images.Count == 0 && !string.IsNullOrEmpty(exhibit.ThumbnailUrl))
            {
                images.Add(exhibit.ThumbnailUrl);
            }

            var arAssets = exhibit.ExhibitArassets?.Select(a => new ArAssetScanDto
            {
                AssetId = a.Id,
                AssetType = a.AssetType,
                AssetUrl = a.AssetUrl,
                FileSizeBytes = a.FileSizeBytes,
                Description = a.Description
            }).ToList() ?? new List<ArAssetScanDto>();

            string? categoryName = exhibit.Category?.CategoryTranslations?.FirstOrDefault(ct => ct.LanguageCode.ToLower() == targetLang)?.CategoryName
                ?? exhibit.Category?.CategoryTranslations?.FirstOrDefault()?.CategoryName;

            var result = new ExhibitScanResultDto
            {
                ExhibitId = exhibit.Id,
                ExhibitCode = exhibit.ExhibitCode ?? $"EX{exhibit.Id:D3}",
                QrcodeData = exhibit.QrcodeData ?? qrData,
                Title = title,
                Description = description,
                AudioUrl = audioUrl,
                LanguageCode = translation?.LanguageCode ?? targetLang,
                CategoryName = categoryName,
                RoomName = exhibit.Room?.RoomName,
                ThumbnailUrl = exhibit.ThumbnailUrl,
                AroverlayUrl = exhibit.AroverlayUrl,
                ArmarkerUrl = exhibit.ArmarkerUrl,
                Images = images,
                ArAssets = arAssets,
                HasArModel = arAssets.Any(a => a.AssetType == "Model3D")
            };

            if (visitorId.HasValue && visitorId.Value > 0)
            {
                try
                {
                    await _unitOfWork.VisitedExhibits.AddAsync(new Repository.Entities.VisitedExhibit
                    {
                        VisitorId = visitorId.Value,
                        ExhibitId = exhibit.Id,
                        VisitedAt = DateTime.UtcNow.AddHours(7)
                    });
                    await _unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ScanExhibitQr Warning]: Could not track visited exhibit: {ex.Message}");
                }
            }

            return ResponseModel.Success("Scan exhibit QR successful", result);
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

            // Check for duplicate ExhibitCode
            if (!string.IsNullOrEmpty(exhibitDto.ExhibitCode))
            {
                var existingExhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                    e => e.ExhibitCode == exhibitDto.ExhibitCode && e.Status != "Archived");
                if (existingExhibit != null)
                {
                    return ResponseModel.BadRequest($"Exhibit code '{exhibitDto.ExhibitCode}' already exists.");
                }
            }

            var exhibit = _mapper.Map<Exhibit>(exhibitDto);
            exhibit.CreatedAt = DateTime.UtcNow;
            exhibit.UpdatedAt = DateTime.UtcNow;
            exhibit.Status = "Draft";

            await _unitOfWork.Exhibits.AddAsync(exhibit);
            await _unitOfWork.CompleteAsync();

            if (string.IsNullOrEmpty(exhibit.QrcodeData))
            {
                string code = !string.IsNullOrEmpty(exhibit.ExhibitCode) ? exhibit.ExhibitCode : $"EX-M{exhibit.MuseumId}-{exhibit.Id}";
                exhibit.QrcodeData = $"MUSEUM_EX_{exhibit.Id}_{code.ToUpper()}";
                exhibit.QrcodeImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(exhibit.QrcodeData)}";
                
                _unitOfWork.Exhibits.Update(exhibit);
                await _unitOfWork.CompleteAsync();
            }

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

            // Check for duplicate ExhibitCode (exclude current exhibit)
            if (!string.IsNullOrEmpty(exhibitDto.ExhibitCode))
            {
                var existingExhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                    e => e.ExhibitCode == exhibitDto.ExhibitCode && e.Id != id && e.Status != "Archived");
                if (existingExhibit != null)
                {
                    return ResponseModel.BadRequest($"Exhibit code '{exhibitDto.ExhibitCode}' already exists.");
                }
            }

            _mapper.Map(exhibitDto, exhibit);
            exhibit.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(exhibit.QrcodeData))
            {
                string code = !string.IsNullOrEmpty(exhibit.ExhibitCode) ? exhibit.ExhibitCode : $"EX-M{exhibit.MuseumId}-{exhibit.Id}";
                exhibit.QrcodeData = $"MUSEUM_EX_{exhibit.Id}_{code.ToUpper()}";
                exhibit.QrcodeImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={Uri.EscapeDataString(exhibit.QrcodeData)}";
            }

            _unitOfWork.Exhibits.Update(exhibit);
            await _unitOfWork.CompleteAsync();

            // Update translations (e.g. title, description, preserving audio guide if not provided)
            if (exhibitDto.Translations != null && exhibitDto.Translations.Count > 0)
            {
                foreach (var transDto in exhibitDto.Translations)
                {
                    var existingTranslation = await _unitOfWork.ExhibitTranslations.GetTranslationAsync(id, transDto.LanguageCode);
                    if (existingTranslation == null)
                    {
                        var translation = _mapper.Map<ExhibitTranslation>(transDto);
                        translation.ExhibitId = id;
                        await _unitOfWork.ExhibitTranslations.AddAsync(translation);
                    }
                    else
                    {
                        existingTranslation.Title = transDto.Title;
                        if (!string.IsNullOrEmpty(transDto.Description))
                        {
                            existingTranslation.Description = transDto.Description;
                        }
                        
                        // Preserve existing audioUrl/Duration if not provided
                        if (!string.IsNullOrEmpty(transDto.AudioUrl))
                        {
                            existingTranslation.AudioUrl = transDto.AudioUrl;
                        }
                        if (transDto.AudioDuration.HasValue)
                        {
                            existingTranslation.AudioDuration = transDto.AudioDuration;
                        }

                        _unitOfWork.ExhibitTranslations.Update(existingTranslation);
                    }
                }
                await _unitOfWork.CompleteAsync();
            }

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

        // --- Room Management ---

        public async Task<ResponseModel> GetRoomsByMuseumIdAsync(int museumId, string? lang = null, int? mapId = null)
        {
            IEnumerable<Room> rooms;
            if (mapId.HasValue)
            {
                rooms = await _unitOfWork.Rooms.FindAsync(
                    r => r.MuseumId == museumId && r.MapId == mapId.Value,
                    includeProperties: "Map,RoomTranslations"
                );
            }
            else
            {
                rooms = await _unitOfWork.Rooms.FindAsync(
                    r => r.MuseumId == museumId,
                    includeProperties: "Map,RoomTranslations"
                );
            }
            var roomDtos = _mapper.Map<IEnumerable<RoomDto>>(rooms).ToList();
            ApplyRoomLanguage(roomDtos, lang);
            return ResponseModel.Success("Get rooms successful", roomDtos);
        }

        public async Task<ResponseModel> GetRoomByIdAsync(int id, string? lang = null)
        {
            var room = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                r => r.Id == id,
                includeProperties: "Map,RoomTranslations"
            );
            if (room == null) return ResponseModel.NotFound("Room not found");
            var roomDto = _mapper.Map<RoomDto>(room);
            ApplyRoomLanguage(new[] { roomDto }, lang);
            return ResponseModel.Success("Get room successful", roomDto);
        }

        public async Task<ResponseModel> CreateRoomAsync(CreateRoomDto roomDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, roomDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (string.IsNullOrWhiteSpace(roomDto.RoomCode) || string.IsNullOrWhiteSpace(roomDto.RoomName))
            {
                return ResponseModel.BadRequest("Room code and room name are required.");
            }

            var existingRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                r => r.MuseumId == roomDto.MuseumId && r.RoomCode.ToLower() == roomDto.RoomCode.Trim().ToLower());
            if (existingRoom != null)
            {
                return ResponseModel.Conflict($"Room with code '{roomDto.RoomCode}' already exists in this museum.");
            }

            var room = _mapper.Map<Room>(roomDto);
            room.RoomCode = roomDto.RoomCode.Trim();
            room.RoomName = roomDto.RoomName.Trim();
            room.CreatedAt = DateTime.UtcNow;
            room.UpdatedAt = DateTime.UtcNow;

            // Copy floorNumber from map if mapId is provided
            if (roomDto.MapId.HasValue)
            {
                var map = await _unitOfWork.MuseumMaps.GetByIdAsync(roomDto.MapId.Value);
                if (map == null) return ResponseModel.NotFound("Linked map not found.");
                room.MapId = map.Id;
                room.FloorNumber = map.FloorNumber; // always copy from map, ignore client
            }
            else
            {
                room.FloorNumber = roomDto.FloorNumber ?? 1;
            }

            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.CompleteAsync();

            await UpsertRoomTranslationAsync(room.Id, "vi", room.RoomName, roomDto.Description);
            if (!string.IsNullOrWhiteSpace(roomDto.RoomNameEn))
            {
                await UpsertRoomTranslationAsync(room.Id, "en", roomDto.RoomNameEn.Trim(), roomDto.DescriptionEn);
            }
            await _unitOfWork.CompleteAsync();

            // Return full RoomDto (not just id) so FE has the complete object
            var createdRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                r => r.Id == room.Id,
                includeProperties: "Map,RoomTranslations"
            );
            var dto = _mapper.Map<RoomDto>(createdRoom);
            return ResponseModel.Success("Room created successfully", dto);
        }

        public async Task<ResponseModel> UpdateRoomAsync(int id, UpdateRoomDto roomDto, int? userMuseumId)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null) return ResponseModel.NotFound("Room not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, room.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (!string.IsNullOrWhiteSpace(roomDto.RoomCode))
            {
                var existingRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                    r => r.MuseumId == room.MuseumId && r.Id != id && r.RoomCode.ToLower() == roomDto.RoomCode.Trim().ToLower());
                if (existingRoom != null)
                {
                    return ResponseModel.Conflict($"Room with code '{roomDto.RoomCode}' already exists.");
                }
                room.RoomCode = roomDto.RoomCode.Trim();
            }

            if (!string.IsNullOrWhiteSpace(roomDto.RoomName))
            {
                room.RoomName = roomDto.RoomName.Trim();
            }

            // Copy floorNumber from map if mapId is provided — don't trust client floorNumber
            if (roomDto.MapId.HasValue)
            {
                var map = await _unitOfWork.MuseumMaps.GetByIdAsync(roomDto.MapId.Value);
                if (map == null) return ResponseModel.NotFound("Linked map not found.");
                room.MapId = map.Id;
                room.FloorNumber = map.FloorNumber; // always copy from map
            }
            else if (roomDto.FloorNumber.HasValue)
            {
                // Only use client floorNumber when no map is linked
                room.FloorNumber = roomDto.FloorNumber.Value;
            }

            if (roomDto.Description != null) room.Description = roomDto.Description;

            room.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Rooms.Update(room);

            if (!string.IsNullOrWhiteSpace(roomDto.RoomName) || roomDto.Description != null)
            {
                await UpsertRoomTranslationAsync(
                    room.Id,
                    "vi",
                    room.RoomName,
                    roomDto.Description ?? room.Description);
            }
            if (!string.IsNullOrWhiteSpace(roomDto.RoomNameEn) || roomDto.DescriptionEn != null)
            {
                var enName = !string.IsNullOrWhiteSpace(roomDto.RoomNameEn)
                    ? roomDto.RoomNameEn.Trim()
                    : room.RoomName;
                await UpsertRoomTranslationAsync(room.Id, "en", enName, roomDto.DescriptionEn);
            }

            await _unitOfWork.CompleteAsync();

            // Return full RoomDto after update
            var updatedRoom = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                r => r.Id == room.Id,
                includeProperties: "Map,RoomTranslations"
            );
            var dto = _mapper.Map<RoomDto>(updatedRoom);
            return ResponseModel.Success("Room updated successfully", dto);
        }

        public async Task<ResponseModel> GetRoomTranslationsAsync(int roomId)
        {
            var room = await _unitOfWork.Rooms.GetFirstOrDefaultAsync(
                r => r.Id == roomId,
                includeProperties: "RoomTranslations");
            if (room == null) return ResponseModel.NotFound("Room not found");
            var dtos = _mapper.Map<IEnumerable<RoomTranslationDto>>(room.RoomTranslations);
            return ResponseModel.Success("Room translations retrieved successfully", dtos);
        }

        public async Task<ResponseModel> AddOrUpdateRoomTranslationAsync(int roomId, RoomTranslationDto dto, int? userMuseumId)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(roomId);
            if (room == null) return ResponseModel.NotFound("Room not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, room.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (string.IsNullOrWhiteSpace(dto.LanguageCode) || string.IsNullOrWhiteSpace(dto.RoomName))
                return ResponseModel.BadRequest("LanguageCode and RoomName are required.");

            await UpsertRoomTranslationAsync(roomId, dto.LanguageCode.Trim().ToLower(), dto.RoomName.Trim(), dto.Description);

            if (dto.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase))
            {
                room.RoomName = dto.RoomName.Trim();
                room.Description = dto.Description;
                room.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Rooms.Update(room);
            }

            await _unitOfWork.CompleteAsync();
            return await GetRoomTranslationsAsync(roomId);
        }

        private async Task UpsertRoomTranslationAsync(int roomId, string languageCode, string roomName, string? description)
        {
            var existing = await _unitOfWork.RoomTranslations.GetFirstOrDefaultAsync(
                t => t.RoomId == roomId && t.LanguageCode == languageCode);
            if (existing != null)
            {
                existing.RoomName = roomName;
                existing.Description = description;
                _unitOfWork.RoomTranslations.Update(existing);
            }
            else
            {
                await _unitOfWork.RoomTranslations.AddAsync(new RoomTranslation
                {
                    RoomId = roomId,
                    LanguageCode = languageCode,
                    RoomName = roomName,
                    Description = description
                });
            }
        }

        private static void ApplyRoomLanguage(IEnumerable<RoomDto> rooms, string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return;
            var code = lang.Trim().ToLower();
            foreach (var room in rooms)
            {
                var match = room.Translations?.FirstOrDefault(t =>
                    t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (match == null) continue;
                room.RoomName = match.RoomName;
                room.Description = match.Description;
            }
        }

        public async Task<ResponseModel> DeleteRoomAsync(int id, int? userMuseumId)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            if (room == null) return ResponseModel.NotFound("Room not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, room.MuseumId);
            if (accessCheck != null) return accessCheck;

            // Check exhibits
            var exhibitCount = (await _unitOfWork.Exhibits.FindAsync(e => e.RoomId == id)).Count();
            if (exhibitCount > 0)
            {
                return ResponseModel.Conflict($"Room has {exhibitCount} exhibit(s). Reassign or clear room first.");
            }

            // Check navigation waypoints
            var waypointCount = (await _unitOfWork.Waypoints.FindAsync(w => w.RoomId == id)).Count();
            if (waypointCount > 0)
            {
                return ResponseModel.Conflict($"Room has {waypointCount} navigation waypoint(s). Remove waypoints first.");
            }

            _unitOfWork.Rooms.Delete(room);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Room deleted successfully");
        }

        // --- Exhibition Management ---

        public async Task<ResponseModel> GetExhibitionsByMuseumIdAsync(int museumId, string? lang = null)
        {
            var exhibitions = await _unitOfWork.Exhibitions.FindAsync(e => e.MuseumId == museumId, includeProperties: "ExhibitionTranslations,Theme");
            var dtos = _mapper.Map<IEnumerable<ExhibitionDto>>(exhibitions).ToList();
            ApplyExhibitionLanguage(dtos, lang);
            return ResponseModel.Success("Exhibitions retrieved successfully", dtos);
        }

        public async Task<ResponseModel> GetExhibitionByIdAsync(int id)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(e => e.Id == id, includeProperties: "ExhibitionTranslations,Exhibits,Theme");
            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");
            var dto = _mapper.Map<ExhibitionDto>(exhibition);
            return ResponseModel.Success("Exhibition retrieved successfully", dto);
        }

        private static void ApplyExhibitionLanguage(IEnumerable<ExhibitionDto> exhibitions, string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return;
            var code = lang.Trim().ToLower();
            foreach (var exhibition in exhibitions)
            {
                var match = exhibition.Translations?.FirstOrDefault(t =>
                    t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (match == null) continue;
                exhibition.Name = match.Name;
                exhibition.Description = match.Description;
            }
        }

        private static void ApplyExhibitMetadataLanguage(IEnumerable<ExhibitDto> exhibits, string? lang)
        {
            if (!string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase)) return;
            foreach (var exhibit in exhibits)
            {
                var meta = exhibit.ExhibitMetadata;
                if (meta == null) continue;
                if (!string.IsNullOrWhiteSpace(meta.EraEn)) meta.Era = meta.EraEn;
                if (!string.IsNullOrWhiteSpace(meta.HistoricalEventEn))
                    meta.HistoricalEvent = meta.HistoricalEventEn;
            }
        }

        private static void ApplyThemeLanguage(IEnumerable<ThemeDto> themes, string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return;
            var code = lang.Trim().ToLower();
            foreach (var theme in themes)
            {
                var match = theme.Translations?.FirstOrDefault(t =>
                    t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (match == null) continue;
                theme.ThemeName = match.ThemeName;
                theme.Description = match.Description;
            }
        }

        private static void ApplyTagLanguage(IEnumerable<TagDto> tags, string? lang)
        {
            if (string.IsNullOrWhiteSpace(lang)) return;
            var code = lang.Trim().ToLower();
            foreach (var tag in tags)
            {
                var match = tag.Translations?.FirstOrDefault(t =>
                    t.LanguageCode.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (match == null) continue;
                tag.TagName = match.TagName;
            }
        }

        public async Task<ResponseModel> CreateExhibitionAsync(CreateExhibitionDto createExhibitionDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, createExhibitionDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (createExhibitionDto.StartDate.HasValue && createExhibitionDto.EndDate.HasValue)
            {
                if (createExhibitionDto.StartDate.Value > createExhibitionDto.EndDate.Value)
                {
                    return ResponseModel.BadRequest("End date must be after or equal to start date.");
                }
            }

            var exhibition = _mapper.Map<Exhibition>(createExhibitionDto);
            
            exhibition.ExhibitionTranslations = new List<ExhibitionTranslation>
            {
                new ExhibitionTranslation { LanguageCode = "vi", Name = createExhibitionDto.Name, Description = createExhibitionDto.Description },
                new ExhibitionTranslation { LanguageCode = "en", Name = createExhibitionDto.Name, Description = createExhibitionDto.Description }
            };

            await _unitOfWork.Exhibitions.AddAsync(exhibition);
            await _unitOfWork.CompleteAsync();

            if (exhibition.ThemeId.HasValue)
            {
                exhibition.Theme = await _unitOfWork.Themes.GetFirstOrDefaultAsync(t => t.Id == exhibition.ThemeId.Value);
            }

            var dto = _mapper.Map<ExhibitionDto>(exhibition);
            return ResponseModel.Success("Exhibition created successfully", dto);
        }

        public async Task<ResponseModel> UpdateExhibitionAsync(int id, CreateExhibitionDto exhibitionDto, int? userMuseumId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == id,
                includeProperties: "ExhibitionTranslations"
            );
            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (exhibitionDto.StartDate.HasValue && exhibitionDto.EndDate.HasValue)
            {
                if (exhibitionDto.StartDate.Value > exhibitionDto.EndDate.Value)
                {
                    return ResponseModel.BadRequest("End date must be after or equal to start date.");
                }
            }

            _mapper.Map(exhibitionDto, exhibition);
            exhibition.UpdatedAt = DateTime.UtcNow;

            var viTrans = exhibition.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "vi");
            if (viTrans != null)
            {
                viTrans.Name = exhibitionDto.Name;
                viTrans.Description = exhibitionDto.Description;
            }
            else
            {
                exhibition.ExhibitionTranslations.Add(new ExhibitionTranslation { LanguageCode = "vi", Name = exhibitionDto.Name, Description = exhibitionDto.Description });
            }

            var enTrans = exhibition.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en");
            if (enTrans != null)
            {
                enTrans.Name = exhibitionDto.Name;
                enTrans.Description = exhibitionDto.Description;
            }
            else
            {
                exhibition.ExhibitionTranslations.Add(new ExhibitionTranslation { LanguageCode = "en", Name = exhibitionDto.Name, Description = exhibitionDto.Description });
            }

            _unitOfWork.Exhibitions.Update(exhibition);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibition updated successfully");
        }

        public async Task<ResponseModel> DeleteExhibitionAsync(int id, int? userMuseumId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetByIdAsync(id);
            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            // Decouple TourRoutes referencing this exhibition
            var routes = await _unitOfWork.TourRoutes.FindAsync(r => r.ExhibitionId == id);
            foreach (var route in routes)
            {
                route.ExhibitionId = null;
                _unitOfWork.TourRoutes.Update(route);
            }

            // Decouple TicketTypes referencing this exhibition
            var ticketTypes = await _unitOfWork.TicketTypes.FindAsync(t => t.ExhibitionId == id);
            foreach (var tt in ticketTypes)
            {
                tt.ExhibitionId = null;
                _unitOfWork.TicketTypes.Update(tt);
            }

            _unitOfWork.Exhibitions.Delete(exhibition);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibition deleted successfully");
        }

        public async Task<ResponseModel> GetExhibitsByExhibitionIdAsync(int exhibitionId, string? lang = null)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == exhibitionId,
                includeProperties: "Exhibits.ExhibitTranslations,Exhibits.ExhibitMetadatum,Exhibits.Room"
            );

            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var exhibitDtos = _mapper.Map<IEnumerable<ExhibitDto>>(exhibition.Exhibits).ToList();
            ApplyExhibitMetadataLanguage(exhibitDtos, lang);
            return ResponseModel.Success("Exhibits for exhibition retrieved successfully", exhibitDtos);
        }

        public async Task<ResponseModel> AssignExhibitsToExhibitionAsync(int exhibitionId, List<int> exhibitIds, int? userMuseumId)
        {
            if (exhibitIds == null || exhibitIds.Count == 0)
            {
                return ResponseModel.BadRequest("Danh sách ID hiện vật không được để trống.");
            }

            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == exhibitionId,
                includeProperties: "Exhibits"
            );

            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            var exhibitsToAdd = await _unitOfWork.Exhibits.FindAsync(e => exhibitIds.Contains(e.Id) && e.MuseumId == exhibition.MuseumId);
            int addedCount = 0;

            foreach (var exhibit in exhibitsToAdd)
            {
                if (!exhibition.Exhibits.Any(e => e.Id == exhibit.Id))
                {
                    exhibition.Exhibits.Add(exhibit);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                _unitOfWork.Exhibitions.Update(exhibition);
                await _unitOfWork.CompleteAsync();
            }

            return ResponseModel.Success($"Đã gán thành công {addedCount} hiện vật vào triển lãm.");
        }

        public async Task<ResponseModel> RemoveExhibitFromExhibitionAsync(int exhibitionId, int exhibitId, int? userMuseumId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == exhibitionId,
                includeProperties: "Exhibits"
            );

            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            var exhibitToRemove = exhibition.Exhibits.FirstOrDefault(e => e.Id == exhibitId);
            if (exhibitToRemove == null)
            {
                return ResponseModel.NotFound("Hiện vật không có trong triển lãm này.");
            }

            exhibition.Exhibits.Remove(exhibitToRemove);
            _unitOfWork.Exhibitions.Update(exhibition);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Đã gỡ hiện vật khỏi triển lãm thành công.");
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
                FloorNumber = mapDto.FloorNumber,
                MapName = mapDto.MapName,
                MapType = mapDto.MapType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MuseumMaps.AddAsync(map);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<MuseumMapDto>(map);
            return ResponseModel.Success("Map created successfully", dto);
        }

        public async Task<ResponseModel> UpdateMuseumMapAsync(int id, UpdateMuseumMapDto mapDto, int? userMuseumId)
        {
            var map = await _unitOfWork.MuseumMaps.GetByIdAsync(id);
            if (map == null) return ResponseModel.NotFound("Map not found.");

            var accessCheck = ValidateMuseumAccess(userMuseumId, map.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (!string.IsNullOrWhiteSpace(mapDto.MapName))
            {
                map.MapName = mapDto.MapName;
            }
            if (!string.IsNullOrWhiteSpace(mapDto.MapType))
            {
                map.MapType = mapDto.MapType;
            }
            if (mapDto.FloorNumber.HasValue)
            {
                map.FloorNumber = mapDto.FloorNumber.Value;
            }
            if (mapDto.MapImage != null && mapDto.MapImage.Length > 0)
            {
                var newImageUrl = await _mediaService.UploadFileAsync(mapDto.MapImage, "maps");
                map.MapImageUrl = newImageUrl;
            }

            map.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.MuseumMaps.Update(map);
            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<MuseumMapDto>(map);
            return ResponseModel.Success("Map updated successfully", dto);
        }

        public async Task<ResponseModel> DeleteMuseumMapAsync(int id, int? userMuseumId)
        {
            var map = await _unitOfWork.MuseumMaps.GetByIdAsync(id);
            if (map == null) return ResponseModel.NotFound("Map not found.");

            var accessCheck = ValidateMuseumAccess(userMuseumId, map.MuseumId);
            if (accessCheck != null) return accessCheck;

            _unitOfWork.MuseumMaps.Delete(map);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Map deleted successfully.");
        }

        // --- Map POI Management ---

        public async Task<ResponseModel> GetMapPoisAsync(int mapId)
        {
            var pois = await _unitOfWork.MapPois.FindAsync(p => p.MapId == mapId);
            var dtos = _mapper.Map<IEnumerable<MapPoiDto>>(pois);
            return ResponseModel.Success("Map POIs retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateMapPoiAsync(CreateMapPoiDto dto, int? userMuseumId)
        {
            var map = await _unitOfWork.MuseumMaps.GetByIdAsync(dto.MapId);
            if (map == null) return ResponseModel.NotFound("Map not found.");

            var accessCheck = ValidateMuseumAccess(userMuseumId, map.MuseumId);
            if (accessCheck != null) return accessCheck;

            var entity = _mapper.Map<MapPoi>(dto);
            await _unitOfWork.MapPois.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            var poiDto = _mapper.Map<MapPoiDto>(entity);
            return ResponseModel.Success("Map POI created successfully", poiDto);
        }

        public async Task<ResponseModel> UpdateMapPoiAsync(int id, UpdateMapPoiDto dto, int? userMuseumId)
        {
            var poi = await _unitOfWork.MapPois.GetByIdAsync(id);
            if (poi == null) return ResponseModel.NotFound("Map POI not found.");

            var map = await _unitOfWork.MuseumMaps.GetByIdAsync(poi.MapId);
            var accessCheck = ValidateMuseumAccess(userMuseumId, map?.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (!string.IsNullOrWhiteSpace(dto.PoiType)) poi.Poitype = dto.PoiType;
            if (dto.LocationX.HasValue) poi.LocationX = dto.LocationX.Value;
            if (dto.LocationY.HasValue) poi.LocationY = dto.LocationY.Value;
            if (dto.Description != null) poi.Description = dto.Description;

            _unitOfWork.MapPois.Update(poi);
            await _unitOfWork.CompleteAsync();

            var poiDto = _mapper.Map<MapPoiDto>(poi);
            return ResponseModel.Success("Map POI updated successfully", poiDto);
        }

        public async Task<ResponseModel> DeleteMapPoiAsync(int id, int? userMuseumId)
        {
            var poi = await _unitOfWork.MapPois.GetByIdAsync(id);
            if (poi == null) return ResponseModel.NotFound("Map POI not found.");

            var map = await _unitOfWork.MuseumMaps.GetByIdAsync(poi.MapId);
            var accessCheck = ValidateMuseumAccess(userMuseumId, map?.MuseumId);
            if (accessCheck != null) return accessCheck;

            _unitOfWork.MapPois.Delete(poi);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Map POI deleted successfully.");
        }

        // --- Tour Route Management ---

        private const string TourRouteIncludes = "TourRouteExhibits.Exhibit.ExhibitTranslations,TourRouteExhibits.Exhibit.Room,TourRouteExhibits.Exhibit.Map,TourRouteTranslations,AgeGroup,Exhibition.ExhibitionTranslations";

        public async Task<ResponseModel> GetTourRoutesAsync(int museumId)
        {
            var routes = await _unitOfWork.TourRoutes.FindAsync(
                r => r.MuseumId == museumId,
                TourRouteIncludes);
            var dtos = _mapper.Map<IEnumerable<TourRouteDto>>(routes);
            return ResponseModel.Success("Tour routes retrieved successfully", dtos);
        }

        public async Task<ResponseModel> GetTourRouteByIdAsync(int id)
        {
            var route = await _unitOfWork.TourRoutes.GetFirstOrDefaultAsync(
                r => r.Id == id,
                TourRouteIncludes);
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");
            var dto = _mapper.Map<TourRouteDto>(route);
            return ResponseModel.Success("Tour route retrieved successfully", dto);
        }

        public async Task<ResponseModel> GetTourRoutesByExhibitionAsync(int exhibitionId)
        {
            var routes = await _unitOfWork.TourRoutes.FindAsync(
                r => r.ExhibitionId == exhibitionId,
                TourRouteIncludes);
            var dtos = _mapper.Map<IEnumerable<TourRouteDto>>(routes);
            return ResponseModel.Success("Tour routes for exhibition retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateTourRouteAsync(CreateTourRouteDto routeDto, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, routeDto.MuseumId);
            if (accessCheck != null) return accessCheck;

            var route = _mapper.Map<TourRoute>(routeDto);
            route.Status = "Active";
            route.CreatedAt = DateTime.UtcNow;
            route.UpdatedAt = DateTime.UtcNow;

            // Add stops
            if (routeDto.Stops != null && routeDto.Stops.Any())
            {
                foreach (var stopDto in routeDto.Stops)
                {
                    route.TourRouteExhibits.Add(new TourRouteExhibit
                    {
                        ExhibitId = stopDto.ExhibitId,
                        StopOrder = stopDto.StopOrder,
                        EstimatedMinutes = stopDto.EstimatedMinutes
                    });
                }
            }

            // Add translations
            if (routeDto.Translations != null && routeDto.Translations.Any())
            {
                foreach (var transDto in routeDto.Translations)
                {
                    route.TourRouteTranslations.Add(new TourRouteTranslation
                    {
                        LanguageCode = transDto.LanguageCode,
                        RouteName = transDto.RouteName,
                        Description = transDto.Description
                    });
                }
            }

            // If no translations provided, create a default one from the Name field
            if (!route.TourRouteTranslations.Any() && !string.IsNullOrEmpty(routeDto.Name))
            {
                route.TourRouteTranslations.Add(new TourRouteTranslation
                {
                    LanguageCode = "vi",
                    RouteName = routeDto.Name
                });
            }

            await _unitOfWork.TourRoutes.AddAsync(route);
            await _unitOfWork.CompleteAsync();

            // Re-fetch with includes for response
            return await GetTourRouteByIdAsync(route.Id);
        }

        public async Task<ResponseModel> UpdateTourRouteAsync(int id, UpdateTourRouteDto routeDto, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetFirstOrDefaultAsync(
                r => r.Id == id,
                "TourRouteTranslations");
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (routeDto.EstimatedDurationMinutes.HasValue)
                route.EstimatedMinutes = routeDto.EstimatedDurationMinutes;
            if (routeDto.AgeGroupId.HasValue)
                route.AgeGroupId = routeDto.AgeGroupId;
            if (routeDto.ExhibitionId.HasValue)
                route.ExhibitionId = routeDto.ExhibitionId;
            if (routeDto.IsDefault.HasValue)
                route.IsDefault = routeDto.IsDefault.Value;
            if (routeDto.ThumbnailUrl != null)
                route.ThumbnailUrl = routeDto.ThumbnailUrl;
            if (routeDto.Status != null)
                route.Status = routeDto.Status;

            // Update the default translation name and description if provided
            if (!string.IsNullOrEmpty(routeDto.Name) || routeDto.Description != null)
            {
                var viTrans = route.TourRouteTranslations.FirstOrDefault(t => t.LanguageCode == "vi");
                if (viTrans != null)
                {
                    if (!string.IsNullOrEmpty(routeDto.Name)) viTrans.RouteName = routeDto.Name;
                    if (routeDto.Description != null) viTrans.Description = routeDto.Description;
                }
                else
                {
                    var firstTrans = route.TourRouteTranslations.FirstOrDefault();
                    if (firstTrans != null)
                    {
                        if (!string.IsNullOrEmpty(routeDto.Name)) firstTrans.RouteName = routeDto.Name;
                        if (routeDto.Description != null) firstTrans.Description = routeDto.Description;
                    }
                }
            }

            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TourRoutes.Update(route);
            await _unitOfWork.CompleteAsync();

            return await GetTourRouteByIdAsync(id);
        }

        public async Task<ResponseModel> DeleteTourRouteAsync(int id, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetFirstOrDefaultAsync(
                r => r.Id == id,
                "TourRouteExhibits,TourRouteTranslations");
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            // Delete related stops and translations first
            foreach (var stop in route.TourRouteExhibits.ToList())
                _unitOfWork.TourRouteExhibits.Delete(stop);
            foreach (var trans in route.TourRouteTranslations.ToList())
                _unitOfWork.TourRouteTranslations.Delete(trans);

            _unitOfWork.TourRoutes.Delete(route);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Tour route deleted successfully");
        }

        // --- Tour Route Stops ---

        public async Task<ResponseModel> AddStopToRouteAsync(int routeId, CreateTourRouteStopDto stopDto, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetByIdAsync(routeId);
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            // Check exhibit exists
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(stopDto.ExhibitId);
            if (exhibit == null)
                return ResponseModel.NotFound("Exhibit not found");

            // Check if already in route
            var existing = await _unitOfWork.TourRouteExhibits.GetFirstOrDefaultAsync(
                s => s.TourRouteId == routeId && s.ExhibitId == stopDto.ExhibitId);
            if (existing != null)
                return ResponseModel.Error("Exhibit already exists in this route");

            var stop = new TourRouteExhibit
            {
                TourRouteId = routeId,
                ExhibitId = stopDto.ExhibitId,
                StopOrder = stopDto.StopOrder,
                EstimatedMinutes = stopDto.EstimatedMinutes
            };

            await _unitOfWork.TourRouteExhibits.AddAsync(stop);
            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TourRoutes.Update(route);
            await _unitOfWork.CompleteAsync();

            return await GetTourRouteByIdAsync(routeId);
        }

        public async Task<ResponseModel> RemoveStopFromRouteAsync(int routeId, int exhibitId, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetByIdAsync(routeId);
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            var stop = await _unitOfWork.TourRouteExhibits.GetFirstOrDefaultAsync(
                s => s.TourRouteId == routeId && s.ExhibitId == exhibitId);
            if (stop == null)
                return ResponseModel.NotFound("Stop not found in this route");

            _unitOfWork.TourRouteExhibits.Delete(stop);

            // Re-order remaining stops
            var remainingStops = await _unitOfWork.TourRouteExhibits.FindAsync(
                s => s.TourRouteId == routeId && s.ExhibitId != exhibitId);
            int order = 1;
            foreach (var s in remainingStops.OrderBy(s => s.StopOrder))
            {
                s.StopOrder = order++;
                _unitOfWork.TourRouteExhibits.Update(s);
            }

            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TourRoutes.Update(route);
            await _unitOfWork.CompleteAsync();

            return await GetTourRouteByIdAsync(routeId);
        }

        public async Task<ResponseModel> ReorderRouteStopsAsync(int routeId, List<int> exhibitIdsInOrder, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetByIdAsync(routeId);
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            var stops = await _unitOfWork.TourRouteExhibits.FindAsync(
                s => s.TourRouteId == routeId);
            var stopList = stops.ToList();

            for (int i = 0; i < exhibitIdsInOrder.Count; i++)
            {
                var stop = stopList.FirstOrDefault(s => s.ExhibitId == exhibitIdsInOrder[i]);
                if (stop != null)
                {
                    stop.StopOrder = i + 1;
                    _unitOfWork.TourRouteExhibits.Update(stop);
                }
            }

            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TourRoutes.Update(route);
            await _unitOfWork.CompleteAsync();

            return await GetTourRouteByIdAsync(routeId);
        }

        // --- Tour Route Translations ---

        public async Task<ResponseModel> AddOrUpdateRouteTranslationAsync(int routeId, TourRouteTranslationDto dto, int? userMuseumId)
        {
            var route = await _unitOfWork.TourRoutes.GetByIdAsync(routeId);
            if (route == null)
                return ResponseModel.NotFound("Tour route not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, route.MuseumId);
            if (accessCheck != null) return accessCheck;

            var existing = await _unitOfWork.TourRouteTranslations.GetFirstOrDefaultAsync(
                t => t.TourRouteId == routeId && t.LanguageCode == dto.LanguageCode);

            if (existing != null)
            {
                existing.RouteName = dto.RouteName;
                existing.Description = dto.Description;
                _unitOfWork.TourRouteTranslations.Update(existing);
            }
            else
            {
                var translation = new TourRouteTranslation
                {
                    TourRouteId = routeId,
                    LanguageCode = dto.LanguageCode,
                    RouteName = dto.RouteName,
                    Description = dto.Description
                };
                await _unitOfWork.TourRouteTranslations.AddAsync(translation);
            }

            route.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.TourRoutes.Update(route);
            await _unitOfWork.CompleteAsync();

            return await GetTourRouteByIdAsync(routeId);
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

            // Update exhibit's thumbnail URL
            exhibit.ThumbnailUrl = fileUrl;
            exhibit.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Exhibits.Update(exhibit);

            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Image uploaded successfully", fileUrl);
        }

        public async Task<ResponseModel> UploadExhibitionImageAsync(int exhibitionId, IFormFile file, int? userMuseumId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetByIdAsync(exhibitionId);
            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            var fileUrl = await _mediaService.UploadFileAsync(file, "exhibitions");

            exhibition.ThumbnailUrl = fileUrl;
            exhibition.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Exhibitions.Update(exhibition);

            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Exhibition image uploaded successfully", fileUrl);
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

        public async Task<ResponseModel> GetLanguagesAsync()
        {
            return ResponseModel.Success("Get languages successfully", new[]
            {
                new { code = "vi", name = "Tiếng Việt" },
                new { code = "en", name = "English" }
            });
        }

        public async Task<ResponseModel> GetRouteTranslationsAsync(int routeId)
        {
            var route = await _unitOfWork.TourRoutes.GetFirstOrDefaultAsync(
                r => r.Id == routeId,
                includeProperties: "TourRouteTranslations");

            if (route == null) return ResponseModel.NotFound("Tour route not found");

            var dtos = route.TourRouteTranslations.Select(t => new TourRouteTranslationDto
            {
                LanguageCode = t.LanguageCode,
                RouteName = t.RouteName,
                Description = t.Description
            });

            return ResponseModel.Success("Get tour route translations successfully", dtos);
        }

        public async Task<ResponseModel> GetExhibitionTranslationsAsync(int exhibitionId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == exhibitionId,
                includeProperties: "ExhibitionTranslations");

            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var dtos = exhibition.ExhibitionTranslations.Select(t => new ExhibitionTranslationDto
            {
                ExhibitionId = t.ExhibitionId,
                LanguageCode = t.LanguageCode,
                Name = t.Name,
                Description = t.Description
            });

            return ResponseModel.Success("Get exhibition translations successfully", dtos);
        }

        public async Task<ResponseModel> AddOrUpdateExhibitionTranslationAsync(int exhibitionId, ExhibitionTranslationDto dto, int? userMuseumId)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetFirstOrDefaultAsync(
                e => e.Id == exhibitionId,
                includeProperties: "ExhibitionTranslations");

            if (exhibition == null) return ResponseModel.NotFound("Exhibition not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibition.MuseumId);
            if (accessCheck != null) return accessCheck;

            var existing = exhibition.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == dto.LanguageCode);
            if (existing != null)
            {
                existing.Name = dto.Name;
                existing.Description = dto.Description;
            }
            else
            {
                exhibition.ExhibitionTranslations.Add(new ExhibitionTranslation
                {
                    ExhibitionId = exhibitionId,
                    LanguageCode = dto.LanguageCode,
                    Name = dto.Name,
                    Description = dto.Description
                });
            }

            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Exhibition translation updated successfully");
        }

        public async Task<ResponseModel> GetCategoryTranslationsAsync(int categoryId)
        {
            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(
                c => c.Id == categoryId,
                includeProperties: "CategoryTranslations");

            if (category == null) return ResponseModel.NotFound("Category not found");

            var dtos = category.CategoryTranslations.Select(t => new CategoryTranslationDto
            {
                CategoryId = t.CategoryId,
                LanguageCode = t.LanguageCode,
                CategoryName = t.CategoryName,
                Description = t.Description
            });

            return ResponseModel.Success("Get category translations successfully", dtos);
        }

        public async Task<ResponseModel> AddOrUpdateCategoryTranslationAsync(int categoryId, CategoryTranslationDto dto, int? userMuseumId)
        {
            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(
                c => c.Id == categoryId,
                includeProperties: "CategoryTranslations");

            if (category == null) return ResponseModel.NotFound("Category not found");

            var existing = category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode == dto.LanguageCode);
            if (existing != null)
            {
                existing.CategoryName = dto.CategoryName;
                existing.Description = dto.Description;
            }
            else
            {
                category.CategoryTranslations.Add(new CategoryTranslation
                {
                    CategoryId = categoryId,
                    LanguageCode = dto.LanguageCode,
                    CategoryName = dto.CategoryName,
                    Description = dto.Description
                });
            }

            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Category translation updated successfully");
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

        public async Task<ResponseModel> GetThemesAsync(int? museumId, string? lang = null)
        {
            IEnumerable<Theme> themes;
            if (museumId.HasValue)
            {
                themes = await _unitOfWork.Themes.FindAsync(
                    t => t.MuseumId == null || t.MuseumId == museumId.Value,
                    "ThemeTranslations");
            }
            else
            {
                themes = await _unitOfWork.Themes.FindAsync(t => t.MuseumId == null, "ThemeTranslations");
            }
            var dtos = _mapper.Map<IEnumerable<ThemeDto>>(themes).ToList();
            ApplyThemeLanguage(dtos, lang);
            return ResponseModel.Success("Get themes successful", dtos);
        }

        public async Task<ResponseModel> GetThemeByIdAsync(int id, string? lang = null)
        {
            var theme = await _unitOfWork.Themes.GetFirstOrDefaultAsync(t => t.Id == id, "ThemeTranslations");
            if (theme == null) return ResponseModel.NotFound("Theme not found");
            var dto = _mapper.Map<ThemeDto>(theme);
            ApplyThemeLanguage(new[] { dto }, lang);
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
            await UpsertThemeTranslationsAsync(theme.Id, theme.ThemeName, theme.Description, themeDto.Translations);
            await _unitOfWork.CompleteAsync();
            var saved = await _unitOfWork.Themes.GetFirstOrDefaultAsync(t => t.Id == theme.Id, "ThemeTranslations");
            var dto = _mapper.Map<ThemeDto>(saved ?? theme);
            return ResponseModel.Success("Theme created successfully", dto);
        }

        public async Task<ResponseModel> UpdateThemeAsync(int id, CreateThemeDto themeDto, int? userMuseumId)
        {
            var theme = await _unitOfWork.Themes.GetByIdAsync(id);
            if (theme == null) return ResponseModel.NotFound("Theme not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, theme.MuseumId);
            if (accessCheck != null) return accessCheck;

            theme.MuseumId = themeDto.MuseumId;
            theme.ThemeName = themeDto.ThemeName;
            theme.Description = themeDto.Description;
            _unitOfWork.Themes.Update(theme);
            await UpsertThemeTranslationsAsync(id, theme.ThemeName, theme.Description, themeDto.Translations);
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

        public async Task<ResponseModel> PublishContentVersionAsync(int versionId, int? userMuseumId, int userId)
        {
            var version = await _unitOfWork.ContentVersions.GetByIdAsync(versionId);
            if (version == null)
            {
                return ResponseModel.NotFound("Content version not found");
            }

            var accessCheck = ValidateMuseumAccess(userMuseumId, version.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (string.Equals(version.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                return ResponseModel.BadRequest("This version is already published.");
            }

            version.Status = "Published";
            version.PublishedAt = DateTime.UtcNow;
            version.PublishedBy = userId;

            _unitOfWork.ContentVersions.Update(version);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Version published successfully");
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

            // Update AroverlayUrl or ArmarkerUrl on Exhibit
            if (assetType == "Model3D")
            {
                exhibit.AroverlayUrl = fileUrl;
            }
            else if (assetType == "MarkerImage")
            {
                exhibit.ArmarkerUrl = fileUrl;
            }
            exhibit.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Exhibits.Update(exhibit);

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

        /// <summary>
        /// Migrate old OverlayImage AR assets: delete them and clear AroverlayUrl on exhibits.
        /// This is a one-time cleanup for the transition from 2D overlay to 3D model.
        /// </summary>
        public async Task<ResponseModel> MigrateOldOverlayAssetsAsync(int museumId, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, museumId);
            if (accessCheck != null) return accessCheck;

            var oldAssets = await _unitOfWork.ExhibitArassets.FindAsync(
                a => a.AssetType == "OverlayImage" && a.Exhibit.MuseumId == museumId,
                includeProperties: "Exhibit"
            );

            var assetList = oldAssets.ToList();
            int deletedCount = 0;

            foreach (var asset in assetList)
            {
                _mediaService.DeleteFile(asset.AssetUrl);

                // Clear AroverlayUrl on the exhibit if it matches the old overlay
                if (asset.Exhibit != null && asset.Exhibit.AroverlayUrl == asset.AssetUrl)
                {
                    asset.Exhibit.AroverlayUrl = null;
                    asset.Exhibit.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Exhibits.Update(asset.Exhibit);
                }

                _unitOfWork.ExhibitArassets.Delete(asset);
                deletedCount++;
            }

            if (deletedCount > 0)
            {
                await _unitOfWork.CompleteAsync();
            }

            return ResponseModel.Success($"Migrated {deletedCount} old OverlayImage AR assets", deletedCount);
        }

        public async Task<ResponseModel> SignArAssetUploadAsync(int exhibitId, SignUploadRequestDto dto, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            const long maxBytes = 209715200; // 200 MiB — Khớp Kestrel 200 MB
            if (dto.FileSize > maxBytes)
            {
                return ResponseModel.BadRequest($"File too large. Maximum is {maxBytes} bytes ({maxBytes / 1024 / 1024} MB). Got {dto.FileSize}.");
            }

            var publicId = $"exhibit-{exhibitId}-model-{Guid.NewGuid().ToString().Substring(0, 8)}";
            var folder = "museum_ar/ar";

            var signResult = _mediaService.GenerateSignedUpload(folder, publicId, maxBytes);
            return ResponseModel.Success("Signed upload parameters generated", signResult);
        }

        public async Task<ResponseModel> ConfirmArAssetUploadAsync(int exhibitId, ConfirmUploadDto dto, int? userMuseumId)
        {
            var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(exhibitId);
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var accessCheck = ValidateMuseumAccess(userMuseumId, exhibit.MuseumId);
            if (accessCheck != null) return accessCheck;

            if (string.IsNullOrWhiteSpace(dto.SecureUrl))
            {
                return ResponseModel.BadRequest("SecureUrl is required.");
            }

            var asset = new ExhibitArasset
            {
                ExhibitId = exhibitId,
                AssetType = dto.AssetType ?? "Model3D",
                AssetUrl = dto.SecureUrl,
                FileSizeBytes = dto.Bytes,
                Description = $"Uploaded via signed upload: {dto.PublicId}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ExhibitArassets.AddAsync(asset);

            // Update AroverlayUrl on Exhibit for Model3D
            if (asset.AssetType == "Model3D")
            {
                exhibit.AroverlayUrl = dto.SecureUrl;
            }
            exhibit.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Exhibits.Update(exhibit);

            await _unitOfWork.CompleteAsync();

            var assetDto = _mapper.Map<ExhibitArassetDto>(asset);
            return ResponseModel.Success("AR Asset confirmed and saved successfully", assetDto);
        }

        // --- Offline Package Management ---


        public async Task<ResponseModel> GenerateOfflinePackageAsync(int museumId, int versionId, int? userMuseumId)
        {
            var accessCheck = ValidateMuseumAccess(userMuseumId, museumId);
            if (accessCheck != null) return accessCheck;

            // Validate that the ContentVersion exists and belongs to the same museum
            var version = await _unitOfWork.ContentVersions.GetByIdAsync(versionId);
            if (version == null)
            {
                return ResponseModel.NotFound($"Content version with ID {versionId} not found.");
            }
            if (version.MuseumId != museumId)
            {
                return ResponseModel.BadRequest("Content version does not belong to the current museum.");
            }

            if (!string.Equals(version.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                return ResponseModel.BadRequest("Chỉ các phiên bản đã xuất bản (Published) mới được phép sử dụng để tạo gói dữ liệu offline.");
            }

            // Validate that this is the latest content version for the museum
            var allVersions = await _unitOfWork.ContentVersions.FindAsync(v => v.MuseumId == museumId);
            if (allVersions != null && allVersions.Any())
            {
                var latestVersion = allVersions.OrderByDescending(v => v.Id).FirstOrDefault();
                if (latestVersion != null && latestVersion.Id != versionId)
                {
                    return ResponseModel.BadRequest($"Chỉ được phép tạo offline package từ phiên bản nội dung mới nhất (v{latestVersion.VersionNumber}).");
                }
            }

            // Check if there is already an offline package for this version
            var existingPackage = await _unitOfWork.OfflinePackages.GetFirstOrDefaultAsync(p => p.VersionId == versionId);
            if (existingPackage != null)
            {
                return ResponseModel.BadRequest("Gói dữ liệu offline cho phiên bản này đã tồn tại.");
            }

            // Fetch museum & content entities for the package
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            var allExhibits = (await _unitOfWork.Exhibits.GetExhibitsWithTranslationsAndMetadataAsync(museumId)).ToList();
            var exhibits = allExhibits.Where(e => string.Equals(e.Status, "Published", StringComparison.OrdinalIgnoreCase)).ToList();
            var maps = (await _unitOfWork.MuseumMaps.FindAsync(m => m.MuseumId == museumId)).ToList();
            var tourRoutes = (await _unitOfWork.TourRoutes.FindAsync(r => r.MuseumId == museumId, "TourRouteExhibits,TourRouteTranslations")).ToList();
            var categories = (await _unitOfWork.Categories.FindAsync(c => c.MuseumId == museumId, "CategoryTranslations")).ToList();
            var rooms = (await _unitOfWork.Rooms.FindAsync(r => r.MuseumId == museumId)).ToList();
            var waypoints = (await _unitOfWork.Waypoints.FindAsync(w => w.MuseumId == museumId)).ToList();
            var edges = (await _unitOfWork.WaypointEdges.FindAsync(e => e.MuseumId == museumId)).ToList();
            var exhibitions = (await _unitOfWork.Exhibitions.FindAsync(e => e.MuseumId == museumId, "ExhibitionTranslations")).ToList();
            var mapIds = maps.Select(m => m.Id).ToList();
            var pois = (await _unitOfWork.MapPois.FindAsync(p => mapIds.Contains(p.MapId))).ToList();

            var exhibitIds = exhibits.Select(e => e.Id).ToList();
            var arAssets = (await _unitOfWork.ExhibitArassets.FindAsync(a => exhibitIds.Contains(a.ExhibitId))).ToList();
            var images = (await _unitOfWork.ExhibitImages.FindAsync(i => exhibitIds.Contains(i.ExhibitId))).ToList();
            var translations = exhibits.SelectMany(e => e.ExhibitTranslations ?? new List<Repository.Entities.ExhibitTranslation>())
                                       .Where(t => !string.IsNullOrWhiteSpace(t.AudioUrl))
                                       .ToList();

            int exhibitCount = exhibits.Count;
            int arAssetCount = arAssets.Count;
            int imageCount = images.Count;
            int audioCount = translations.Count;

            // Create initial database record with "Building" status
            var package = new OfflinePackage
            {
                MuseumId = museumId,
                VersionId = versionId,
                Status = "Building",
                CreatedAt = DateTime.UtcNow,
                PackageSizeBytes = 0,
                PackageUrl = "",
                ExhibitCount = exhibitCount,
                ArassetCount = arAssetCount,
                ImageCount = imageCount,
                AudioCount = audioCount
            };

            await _unitOfWork.OfflinePackages.AddAsync(package);
            await _unitOfWork.CompleteAsync();

            try
            {
                // Ensure output directory exists: wwwroot/uploads/packages
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var packagesDir = Path.Combine(wwwroot, "uploads", "packages");
                if (!Directory.Exists(packagesDir))
                {
                    Directory.CreateDirectory(packagesDir);
                }

                var fileName = $"museum_{museumId}_v{versionId}.zip";
                var zipFilePath = Path.Combine(packagesDir, fileName);

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, leaveOpen: true))
                        {
                            // 1. Add manifest.json
                            var manifestDto = new
                            {
                                MuseumId = museumId,
                                MuseumName = museum?.Name,
                                VersionId = versionId,
                                VersionNumber = version.VersionNumber,
                                GeneratedAt = DateTime.UtcNow,
                                ExhibitCount = exhibitCount,
                                ArAssetCount = arAssetCount,
                                ImageCount = imageCount,
                                AudioCount = audioCount,
                                Exhibits = _mapper.Map<IEnumerable<ExhibitDto>>(exhibits),
                                Maps = _mapper.Map<IEnumerable<MuseumMapDto>>(maps),
                                TourRoutes = _mapper.Map<IEnumerable<TourRouteDto>>(tourRoutes),
                                Categories = _mapper.Map<IEnumerable<CategoryDto>>(categories),
                                Rooms = _mapper.Map<IEnumerable<RoomDto>>(rooms),
                                Waypoints = waypoints.Select(w => new { w.Id, w.MuseumId, w.MapId, w.FloorNumber, w.X, w.Y, w.Type, w.RoomId, w.Code, w.Label, w.CreatedAt, w.UpdatedAt }),
                                WaypointEdges = edges.Select(e => new { e.Id, e.MuseumId, e.FromWaypointId, e.ToWaypointId, e.Distance, e.EdgeType, e.IsBidirectional, e.CreatedAt, e.UpdatedAt }),
                                Exhibitions = _mapper.Map<IEnumerable<ExhibitionDto>>(exhibitions),
                                MapPois = _mapper.Map<IEnumerable<MapPoiDto>>(pois)
                            };

                            var jsonString = JsonSerializer.Serialize(manifestDto, new JsonSerializerOptions { WriteIndented = true });
                            var manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.Optimal);
                            using (var entryStream = manifestEntry.Open())
                            using (var writer = new StreamWriter(entryStream))
                            {
                                await writer.WriteAsync(jsonString);
                            }

                            // Helper function to download and pack a media file into zip
                            async Task AddUrlFileToZipAsync(string? url, string folderName, string defaultName)
                            {
                                if (string.IsNullOrWhiteSpace(url)) return;
                                try
                                {
                                    string trimmedUrl = url.Trim();

                                    // If URL is an absolute HTTP/HTTPS URL
                                    if (trimmedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                        trimmedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var uri = new Uri(trimmedUrl);
                                        var pathPart = uri.AbsolutePath.TrimStart('/');
                                        var localFileFromUri = Path.Combine(wwwroot, pathPart.Replace('/', Path.DirectorySeparatorChar));

                                        // 1. Try reading directly from local wwwroot if file exists locally
                                        if (File.Exists(localFileFromUri))
                                        {
                                            var fileExt = Path.GetExtension(localFileFromUri);
                                            if (string.IsNullOrEmpty(fileExt)) fileExt = ".jpg";
                                            var entryName = $"{folderName}/{defaultName}{fileExt}";
                                            archive.CreateEntryFromFile(localFileFromUri, entryName, CompressionLevel.Optimal);
                                            return;
                                        }

                                        // 2. Otherwise download via HTTP
                                        var escapedUrl = new Uri(trimmedUrl).AbsoluteUri;
                                        var bytes = await httpClient.GetByteArrayAsync(escapedUrl);
                                        var ext = Path.GetExtension(uri.AbsolutePath);
                                        if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                                        var httpEntryName = $"{folderName}/{defaultName}{ext}";
                                        var entry = archive.CreateEntry(httpEntryName, CompressionLevel.Optimal);
                                        using (var entryStream = entry.Open())
                                        {
                                            await entryStream.WriteAsync(bytes, 0, bytes.Length);
                                        }
                                    }
                                    else
                                    {
                                        // Handle local relative paths (e.g. "/uploads/...", "uploads/...", "uploads\...")
                                        var cleanPath = trimmedUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                                        var localPath = Path.Combine(wwwroot, cleanPath);

                                        if (File.Exists(localPath))
                                        {
                                            var fileExt = Path.GetExtension(localPath);
                                            if (string.IsNullOrEmpty(fileExt)) fileExt = ".jpg";
                                            var entryName = $"{folderName}/{defaultName}{fileExt}";
                                            archive.CreateEntryFromFile(localPath, entryName, CompressionLevel.Optimal);
                                        }
                                    }
                                }
                                catch
                                {
                                    // Skip inaccessible external media without crashing package generation
                                }
                            }

                            // 2. Add Map Images
                            foreach (var map in maps)
                            {
                                await AddUrlFileToZipAsync(map.MapImageUrl, "maps", $"map_{map.Id}");
                            }

                            // 3. Add Exhibit Primary Thumbnails & AR Overlays/Markers
                            foreach (var exhibit in exhibits)
                            {
                                await AddUrlFileToZipAsync(exhibit.ThumbnailUrl, "images", $"exhibit_{exhibit.Id}_thumb");
                                await AddUrlFileToZipAsync(exhibit.AroverlayUrl, "ar", $"exhibit_{exhibit.Id}_overlay");
                                await AddUrlFileToZipAsync(exhibit.ArmarkerUrl, "ar", $"exhibit_{exhibit.Id}_marker");
                            }

                            // 4. Add Exhibit Gallery Images
                            foreach (var img in images)
                            {
                                await AddUrlFileToZipAsync(img.ImageUrl, "images", $"exhibit_{img.ExhibitId}_img_{img.Id}");
                            }

                            // 5. Add Audio Guides
                            foreach (var trans in translations)
                            {
                                await AddUrlFileToZipAsync(trans.AudioUrl, "audio", $"exhibit_{trans.ExhibitId}_{trans.LanguageCode}");
                            }

                            // 6. Add AR Assets
                            foreach (var ar in arAssets)
                            {
                                await AddUrlFileToZipAsync(ar.AssetUrl, "ar", $"exhibit_{ar.ExhibitId}_ar_{ar.Id}");
                            }
                        }

                        // Calculate final ZIP file size & SHA256 Checksum
                        zipFileStream.Position = 0;
                        using var sha256 = SHA256.Create();
                        var hashBytes = sha256.ComputeHash(zipFileStream);
                        var checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                        long fileSize = zipFileStream.Length;

                        package.Status = "Available";
                        package.PackageUrl = $"/uploads/packages/{fileName}";
                        package.PackageSizeBytes = fileSize;
                        package.Checksum = checksum;
                        package.BuiltAt = DateTime.UtcNow;
                    }
                }

                _unitOfWork.OfflinePackages.Update(package);
                await _unitOfWork.CompleteAsync();

                return ResponseModel.Success("Offline package ZIP generated successfully", package.Id);
            }
            catch (Exception ex)
            {
                package.Status = "Failed";
                _unitOfWork.OfflinePackages.Update(package);
                await _unitOfWork.CompleteAsync();
                return ResponseModel.Error($"Failed to generate offline ZIP package: {ex.Message}");
            }
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
            var tagGroups = await _unitOfWork.TagGroups.FindAsync(_ => true, "TagGroupTranslations");
            var dtos = _mapper.Map<IEnumerable<TagGroupDto>>(tagGroups);
            return ResponseModel.Success("Get tag groups successful", dtos);
        }

        public async Task<ResponseModel> CreateTagGroupAsync(CreateTagGroupDto tagGroupDto)
        {
            var tagGroup = _mapper.Map<TagGroup>(tagGroupDto);
            tagGroup.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.TagGroups.AddAsync(tagGroup);
            await _unitOfWork.CompleteAsync();
            await UpsertTagGroupTranslationsAsync(tagGroup.Id, tagGroup.GroupName, tagGroupDto.Translations);
            await _unitOfWork.CompleteAsync();
            var saved = await _unitOfWork.TagGroups.GetFirstOrDefaultAsync(g => g.Id == tagGroup.Id, "TagGroupTranslations");
            var dto = _mapper.Map<TagGroupDto>(saved ?? tagGroup);
            return ResponseModel.Success("Tag group created successfully", dto);
        }

        public async Task<ResponseModel> UpdateTagGroupAsync(int id, CreateTagGroupDto tagGroupDto)
        {
            var tagGroup = await _unitOfWork.TagGroups.GetByIdAsync(id);
            if (tagGroup == null) return ResponseModel.NotFound("Tag group not found");

            tagGroup.GroupName = tagGroupDto.GroupName;
            tagGroup.SortOrder = tagGroupDto.SortOrder;
            _unitOfWork.TagGroups.Update(tagGroup);
            await UpsertTagGroupTranslationsAsync(id, tagGroup.GroupName, tagGroupDto.Translations);
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

        public async Task<ResponseModel> GetTagsByGroupAsync(int tagGroupId, string? lang = null)
        {
            var tags = await _unitOfWork.Tags.FindAsync(t => t.TagGroupId == tagGroupId, "TagTranslations");
            var dtos = _mapper.Map<IEnumerable<TagDto>>(tags).ToList();
            ApplyTagLanguage(dtos, lang);
            return ResponseModel.Success("Get tags successful", dtos);
        }

        public async Task<ResponseModel> GetAllTagsAsync(string? lang = null)
        {
            var tags = await _unitOfWork.Tags.FindAsync(_ => true, "TagTranslations");
            var dtos = _mapper.Map<IEnumerable<TagDto>>(tags).ToList();
            ApplyTagLanguage(dtos, lang);
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
            await UpsertTagTranslationsAsync(tag.Id, tag.TagName, tagDto.Translations);
            await _unitOfWork.CompleteAsync();
            var saved = await _unitOfWork.Tags.GetFirstOrDefaultAsync(t => t.Id == tag.Id, "TagTranslations");
            var dto = _mapper.Map<TagDto>(saved ?? tag);
            return ResponseModel.Success("Tag created successfully", dto);
        }

        public async Task<ResponseModel> UpdateTagAsync(int id, CreateTagDto tagDto)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) return ResponseModel.NotFound("Tag not found");

            tag.TagGroupId = tagDto.TagGroupId;
            tag.TagName = tagDto.TagName;
            tag.SortOrder = tagDto.SortOrder;
            _unitOfWork.Tags.Update(tag);
            await UpsertTagTranslationsAsync(id, tag.TagName, tagDto.Translations);
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

        public async Task<ResponseModel> GetExhibitTagsAsync(int exhibitId, string? lang = null)
        {
            var exhibit = await _unitOfWork.Exhibits.GetFirstOrDefaultAsync(
                e => e.Id == exhibitId,
                includeProperties: "Tags.TagTranslations"
            );
            if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

            var dtos = _mapper.Map<IEnumerable<TagDto>>(exhibit.Tags).ToList();
            ApplyTagLanguage(dtos, lang);
            return ResponseModel.Success("Get exhibit tags successful", dtos);
        }

        private async Task UpsertThemeTranslationsAsync(
            int themeId,
            string themeName,
            string? description,
            ICollection<ThemeTranslationDto>? translations)
        {
            var incoming = (translations ?? Enumerable.Empty<ThemeTranslationDto>())
                .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode) && !string.IsNullOrWhiteSpace(t.ThemeName))
                .GroupBy(t => t.LanguageCode.Trim().ToLower())
                .Select(g => g.Last())
                .ToList();

            if (!incoming.Any(t => t.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)))
            {
                incoming.Insert(0, new ThemeTranslationDto
                {
                    ThemeId = themeId,
                    LanguageCode = "vi",
                    ThemeName = themeName,
                    Description = description
                });
            }

            foreach (var transDto in incoming)
            {
                var code = transDto.LanguageCode.Trim().ToLower();
                var existing = await _unitOfWork.ThemeTranslations.GetFirstOrDefaultAsync(
                    t => t.ThemeId == themeId && t.LanguageCode == code);
                if (existing != null)
                {
                    existing.ThemeName = transDto.ThemeName.Trim();
                    existing.Description = transDto.Description;
                    _unitOfWork.ThemeTranslations.Update(existing);
                }
                else
                {
                    await _unitOfWork.ThemeTranslations.AddAsync(new ThemeTranslation
                    {
                        ThemeId = themeId,
                        LanguageCode = code,
                        ThemeName = transDto.ThemeName.Trim(),
                        Description = transDto.Description
                    });
                }
            }
        }

        private async Task UpsertTagTranslationsAsync(
            int tagId,
            string tagName,
            ICollection<TagTranslationDto>? translations)
        {
            var incoming = (translations ?? Enumerable.Empty<TagTranslationDto>())
                .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode) && !string.IsNullOrWhiteSpace(t.TagName))
                .GroupBy(t => t.LanguageCode.Trim().ToLower())
                .Select(g => g.Last())
                .ToList();

            if (!incoming.Any(t => t.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)))
            {
                incoming.Insert(0, new TagTranslationDto
                {
                    TagId = tagId,
                    LanguageCode = "vi",
                    TagName = tagName
                });
            }

            foreach (var transDto in incoming)
            {
                var code = transDto.LanguageCode.Trim().ToLower();
                var existing = await _unitOfWork.TagTranslations.GetFirstOrDefaultAsync(
                    t => t.TagId == tagId && t.LanguageCode == code);
                if (existing != null)
                {
                    existing.TagName = transDto.TagName.Trim();
                    _unitOfWork.TagTranslations.Update(existing);
                }
                else
                {
                    await _unitOfWork.TagTranslations.AddAsync(new TagTranslation
                    {
                        TagId = tagId,
                        LanguageCode = code,
                        TagName = transDto.TagName.Trim()
                    });
                }
            }
        }

        private async Task UpsertTagGroupTranslationsAsync(
            int tagGroupId,
            string groupName,
            ICollection<TagGroupTranslationDto>? translations)
        {
            var incoming = (translations ?? Enumerable.Empty<TagGroupTranslationDto>())
                .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode) && !string.IsNullOrWhiteSpace(t.GroupName))
                .GroupBy(t => t.LanguageCode.Trim().ToLower())
                .Select(g => g.Last())
                .ToList();

            if (!incoming.Any(t => t.LanguageCode.Equals("vi", StringComparison.OrdinalIgnoreCase)))
            {
                incoming.Insert(0, new TagGroupTranslationDto
                {
                    TagGroupId = tagGroupId,
                    LanguageCode = "vi",
                    GroupName = groupName
                });
            }

            foreach (var transDto in incoming)
            {
                var code = transDto.LanguageCode.Trim().ToLower();
                var existing = await _unitOfWork.TagGroupTranslations.GetFirstOrDefaultAsync(
                    t => t.TagGroupId == tagGroupId && t.LanguageCode == code);
                if (existing != null)
                {
                    existing.GroupName = transDto.GroupName.Trim();
                    _unitOfWork.TagGroupTranslations.Update(existing);
                }
                else
                {
                    await _unitOfWork.TagGroupTranslations.AddAsync(new TagGroupTranslation
                    {
                        TagGroupId = tagGroupId,
                        LanguageCode = code,
                        GroupName = transDto.GroupName.Trim()
                    });
                }
            }
        }
    }
}
