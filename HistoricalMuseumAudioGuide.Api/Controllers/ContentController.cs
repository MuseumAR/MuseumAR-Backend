    using Microsoft.AspNetCore.Mvc;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Content;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly IMuseumResolver _museumResolver;

        public ContentController(IContentService contentService, IMuseumResolver museumResolver)
        {
            _contentService = contentService;
            _museumResolver = museumResolver;
        }

        /// <summary>
        /// Extracts the MuseumId from the current user's JWT claims.
        /// Returns null for SystemAdmin (unrestricted access) or unauthenticated users.
        /// </summary>
        private int? GetCurrentUserMuseumId()
        {
            var museumIdClaim = User.FindFirst("MuseumId");
            if (museumIdClaim != null && int.TryParse(museumIdClaim.Value, out int museumId))
            {
                return museumId;
            }
            return null;
        }

        // --- Exhibit Management (Read - Public) ---

        [HttpGet("exhibits")]
        public async Task<IActionResult> GetAllExhibits([FromQuery] string? lang = null)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetAllExhibitsAsync(museumId, lang);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibits/{id}")]
        public async Task<IActionResult> GetExhibit(int id, [FromQuery] string? lang = null)
        {
            var response = await _contentService.GetExhibitByIdAsync(id, lang);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibits/scan-qr")]
        public async Task<IActionResult> ScanExhibitQr([FromQuery] string qrData, [FromQuery] string? lang = "vi", [FromQuery] int? visitorId = null)
        {
            var response = await _contentService.ScanExhibitQrAsync(qrData, lang, visitorId);
            return ResponseParser.Result(response);
        }

        // --- Exhibit Management (Write - ContentManager only, Museum-Scoped) ---

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits")]
        public async Task<IActionResult> CreateExhibit([FromBody] CreateExhibitDto exhibitDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateExhibitAsync(exhibitDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPut("exhibits/{id}")]
        public async Task<IActionResult> UpdateExhibit(int id, [FromBody] CreateExhibitDto exhibitDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateExhibitAsync(id, exhibitDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpDelete("exhibits/{id}")]
        public async Task<IActionResult> DeleteExhibit(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteExhibitAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits/{id}/publish")]
        public async Task<IActionResult> PublishExhibit(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.PublishExhibitAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits/{id}/unpublish")]
        public async Task<IActionResult> UnpublishExhibit(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UnpublishExhibitAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Room Management ---

        [HttpGet("rooms/museum/{museumId}")]
        public async Task<IActionResult> GetRoomsByMuseumId(int museumId, [FromQuery] string? lang = null)
        {
            var result = await _contentService.GetRoomsByMuseumIdAsync(museumId, lang);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("rooms")]
        [Authorize(Roles = "ContentManager,MuseumAdmin,SystemAdmin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto roomDto)
        {
            int? userMuseumId = GetCurrentUserMuseumId();
            var result = await _contentService.CreateRoomAsync(roomDto, userMuseumId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("rooms/{id}")]
        [Authorize(Roles = "ContentManager,MuseumAdmin,SystemAdmin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto roomDto)
        {
            int? userMuseumId = GetCurrentUserMuseumId();
            var result = await _contentService.UpdateRoomAsync(id, roomDto, userMuseumId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("rooms/{id}")]
        [Authorize(Roles = "ContentManager,MuseumAdmin,SystemAdmin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            int? userMuseumId = GetCurrentUserMuseumId();
            var result = await _contentService.DeleteRoomAsync(id, userMuseumId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("rooms/{id}/translations")]
        public async Task<IActionResult> GetRoomTranslations(int id)
        {
            var result = await _contentService.GetRoomTranslationsAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "ContentManager,MuseumAdmin,SystemAdmin")]
        [HttpPut("rooms/{id}/translations")]
        public async Task<IActionResult> AddOrUpdateRoomTranslation(int id, [FromBody] RoomTranslationDto dto)
        {
            int? userMuseumId = GetCurrentUserMuseumId();
            var result = await _contentService.AddOrUpdateRoomTranslationAsync(id, dto, userMuseumId);
            return StatusCode(result.StatusCode, result);
        }

        // --- Media Management (Write - ContentManager only, Museum-Scoped) ---

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits/{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file, [FromForm] string caption)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UploadExhibitImageAsync(id, file, caption, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits/{id}/upload-audio")]
        public async Task<IActionResult> UploadAudio(int id, [FromForm] string languageCode, [FromForm] IFormFile file)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UploadExhibitAudioAsync(id, languageCode, file, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Translation & Versions (Read - Public, Write - Authorized) ---

        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var response = await _contentService.GetLanguagesAsync();
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibits/{id}/translations")]
        public async Task<IActionResult> GetTranslations(int id)
        {
            var response = await _contentService.GetExhibitTranslationsAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
        [HttpPost("exhibits/{id}/translations")]
        public async Task<IActionResult> AddOrUpdateExhibitTranslation(int id, [FromBody] ExhibitTranslationDto dto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddOrUpdateExhibitTranslationAsync(id, dto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("versions")]
        public async Task<IActionResult> CreateContentVersion([FromQuery] string versionNumber, [FromQuery] string description)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateNewContentVersionAsync(museumId, versionNumber, description, userMuseumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("versions")]
        public async Task<IActionResult> GetContentVersions()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetContentVersionsAsync(museumId);
            return ResponseParser.Result(response);
        }

        // --- AR Asset Management (Read - Public, Write - Authorized) ---

        [HttpGet("exhibits/{exhibitId}/ar-assets")]
        public async Task<IActionResult> GetArAssets(int exhibitId)
        {
            var response = await _contentService.GetArAssetsByExhibitIdAsync(exhibitId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpPost("exhibits/{exhibitId}/ar-assets/upload")]
        public async Task<IActionResult> AddArAsset(int exhibitId, [FromForm] string assetType, [FromForm] IFormFile file, [FromForm] string? description)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddArAssetAsync(exhibitId, assetType, file, description, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager")]
        [HttpDelete("ar-assets/{id}")]
        public async Task<IActionResult> DeleteArAsset(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteArAssetAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Offline Package Management (Read - Public, Write - Authorized) ---

        [HttpGet("packages")]
        public async Task<IActionResult> GetOfflinePackages()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetOfflinePackagesByMuseumIdAsync(museumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("packages/generate")]
        public async Task<IActionResult> GenerateOfflinePackage([FromBody] CreateOfflinePackageDto dto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.GenerateOfflinePackageAsync(museumId, dto.VersionId, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Exhibition Management (Read - Public, Write - Authorized) ---

        [HttpGet("exhibitions")]
        public async Task<IActionResult> GetExhibitions([FromQuery] string? lang = null)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetExhibitionsByMuseumIdAsync(museumId, lang);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("exhibitions")]
        public async Task<IActionResult> CreateExhibition(CreateExhibitionDto createExhibitionDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateExhibitionAsync(createExhibitionDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("exhibitions/{id}/upload-image")]
        public async Task<IActionResult> UploadExhibitionImage(int id, IFormFile file)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UploadExhibitionImageAsync(id, file, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("exhibitions/{id}")]
        public async Task<IActionResult> UpdateExhibition(int id, CreateExhibitionDto createExhibitionDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateExhibitionAsync(id, createExhibitionDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpDelete("exhibitions/{id}")]
        public async Task<IActionResult> DeleteExhibition(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteExhibitionAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibitions/{id}/translations")]
        public async Task<IActionResult> GetExhibitionTranslations(int id)
        {
            var response = await _contentService.GetExhibitionTranslationsAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("exhibitions/{id}/translations")]
        public async Task<IActionResult> AddOrUpdateExhibitionTranslation(int id, [FromBody] ExhibitionTranslationDto dto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddOrUpdateExhibitionTranslationAsync(id, dto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibitions/{exhibitionId}/exhibits")]
        public async Task<IActionResult> GetExhibitsByExhibition(int exhibitionId, [FromQuery] string? lang = null)
        {
            var response = await _contentService.GetExhibitsByExhibitionIdAsync(exhibitionId, lang);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("exhibitions/{exhibitionId}/exhibits")]
        public async Task<IActionResult> AssignExhibitsToExhibition(int exhibitionId, [FromBody] List<int> exhibitIds)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AssignExhibitsToExhibitionAsync(exhibitionId, exhibitIds, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpDelete("exhibitions/{exhibitionId}/exhibits/{exhibitId}")]
        public async Task<IActionResult> RemoveExhibitFromExhibition(int exhibitionId, int exhibitId)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.RemoveExhibitFromExhibitionAsync(exhibitionId, exhibitId, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Maps Management (Read - Public, Write - Authorized) ---

        [HttpGet("maps")]
        public async Task<IActionResult> GetMuseumMaps()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetMuseumMapsAsync(museumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("maps")]
        public async Task<IActionResult> CreateMuseumMap([FromForm] CreateMuseumMapDto createMuseumMapDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateMuseumMapAsync(createMuseumMapDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("maps/{id}")]
        public async Task<IActionResult> UpdateMuseumMap(int id, [FromForm] UpdateMuseumMapDto updateMuseumMapDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateMuseumMapAsync(id, updateMuseumMapDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpDelete("maps/{id}")]
        public async Task<IActionResult> DeleteMuseumMap(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteMuseumMapAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Tour Routes Management (Read - Public, Write - Authorized) ---

        [HttpGet("routes")]
        public async Task<IActionResult> GetTourRoutes()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetTourRoutesAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("routes/{id}")]
        public async Task<IActionResult> GetTourRoute(int id)
        {
            var response = await _contentService.GetTourRouteByIdAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpGet("routes/exhibition/{exhibitionId}")]
        public async Task<IActionResult> GetTourRoutesByExhibition(int exhibitionId)
        {
            var response = await _contentService.GetTourRoutesByExhibitionAsync(exhibitionId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("routes")]
        public async Task<IActionResult> CreateTourRoute(CreateTourRouteDto createTourRouteDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateTourRouteAsync(createTourRouteDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("routes/{id}")]
        public async Task<IActionResult> UpdateTourRoute(int id, UpdateTourRouteDto updateTourRouteDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateTourRouteAsync(id, updateTourRouteDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpDelete("routes/{id}")]
        public async Task<IActionResult> DeleteTourRoute(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteTourRouteAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Tour Route Stops ---

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("routes/{routeId}/stops")]
        public async Task<IActionResult> AddStopToRoute(int routeId, CreateTourRouteStopDto stopDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddStopToRouteAsync(routeId, stopDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpDelete("routes/{routeId}/stops/{exhibitId}")]
        public async Task<IActionResult> RemoveStopFromRoute(int routeId, int exhibitId)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.RemoveStopFromRouteAsync(routeId, exhibitId, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("routes/{routeId}/stops/reorder")]
        public async Task<IActionResult> ReorderRouteStops(int routeId, [FromBody] List<int> exhibitIdsInOrder)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.ReorderRouteStopsAsync(routeId, exhibitIdsInOrder, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Tour Route Translations ---

        [HttpGet("routes/{routeId}/translations")]
        public async Task<IActionResult> GetRouteTranslations(int routeId)
        {
            var response = await _contentService.GetRouteTranslationsAsync(routeId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("routes/{routeId}/translations")]
        public async Task<IActionResult> AddOrUpdateRouteTranslation(int routeId, TourRouteTranslationDto translationDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddOrUpdateRouteTranslationAsync(routeId, translationDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Category Management (Read - Public, Write - Authorized) ---

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetCategoriesAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var response = await _contentService.GetCategoryByIdAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpGet("categories/{id}/translations")]
        public async Task<IActionResult> GetCategoryTranslations(int id)
        {
            var response = await _contentService.GetCategoryTranslationsAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
        [HttpPut("categories/{id}/translations")]
        public async Task<IActionResult> AddOrUpdateCategoryTranslation(int id, [FromBody] CategoryTranslationDto dto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AddOrUpdateCategoryTranslationAsync(id, dto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateCategoryAsync(categoryDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto categoryDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateCategoryAsync(id, categoryDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteCategoryAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Reference Metadata (Read - Public, Write - Authorized) ---

        [HttpGet("themes")]
        public async Task<IActionResult> GetThemes([FromQuery] string? lang = null)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _contentService.GetThemesAsync(museumId, lang);
            return ResponseParser.Result(response);
        }

        [HttpGet("themes/{id}")]
        public async Task<IActionResult> GetTheme(int id, [FromQuery] string? lang = null)
        {
            var response = await _contentService.GetThemeByIdAsync(id, lang);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
        [HttpPost("themes")]
        public async Task<IActionResult> CreateTheme([FromBody] CreateThemeDto themeDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateThemeAsync(themeDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
        [HttpPut("themes/{id}")]
        public async Task<IActionResult> UpdateTheme(int id, [FromBody] CreateThemeDto themeDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateThemeAsync(id, themeDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,MuseumManager,SystemAdmin")]
        [HttpDelete("themes/{id}")]
        public async Task<IActionResult> DeleteTheme(int id)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.DeleteThemeAsync(id, userMuseumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("age-groups")]
        public async Task<IActionResult> GetAgeGroups()
        {
            var response = await _contentService.GetAllAgeGroupsAsync();
            return ResponseParser.Result(response);
        }

        // --- Tag Management ---

        [HttpGet("tag-groups")]
        public async Task<IActionResult> GetTagGroups()
        {
            var response = await _contentService.GetTagGroupsAsync();
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost("tag-groups")]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto tagGroupDto)
        {
            var response = await _contentService.CreateTagGroupAsync(tagGroupDto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("tag-groups/{id}")]
        public async Task<IActionResult> UpdateTagGroup(int id, [FromBody] CreateTagGroupDto tagGroupDto)
        {
            var response = await _contentService.UpdateTagGroupAsync(id, tagGroupDto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpDelete("tag-groups/{id}")]
        public async Task<IActionResult> DeleteTagGroup(int id)
        {
            var response = await _contentService.DeleteTagGroupAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags([FromQuery] string? lang = null)
        {
            var response = await _contentService.GetAllTagsAsync(lang);
            return ResponseParser.Result(response);
        }

        [HttpGet("tag-groups/{tagGroupId}/tags")]
        public async Task<IActionResult> GetTagsByGroup(int tagGroupId, [FromQuery] string? lang = null)
        {
            var response = await _contentService.GetTagsByGroupAsync(tagGroupId, lang);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost("tags")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto tagDto)
        {
            var response = await _contentService.CreateTagAsync(tagDto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPut("tags/{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] CreateTagDto tagDto)
        {
            var response = await _contentService.UpdateTagAsync(id, tagDto);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpDelete("tags/{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var response = await _contentService.DeleteTagAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,SystemAdmin")]
        [HttpPost("exhibits/{exhibitId}/tags")]
        public async Task<IActionResult> AssignTagsToExhibit(int exhibitId, [FromBody] List<int> tagIds)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.AssignTagsToExhibitAsync(exhibitId, tagIds, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "ContentManager,SystemAdmin")]
        [HttpDelete("exhibits/{exhibitId}/tags/{tagId}")]
        public async Task<IActionResult> RemoveTagFromExhibit(int exhibitId, int tagId)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.RemoveTagFromExhibitAsync(exhibitId, tagId, userMuseumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibits/{exhibitId}/tags")]
        public async Task<IActionResult> GetExhibitTags(int exhibitId, [FromQuery] string? lang = null)
        {
            var response = await _contentService.GetExhibitTagsAsync(exhibitId, lang);
            return ResponseParser.Result(response);
        }
    }
}
