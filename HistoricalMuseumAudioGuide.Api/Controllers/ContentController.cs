    using Microsoft.AspNetCore.Mvc;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Content;
using System.Threading.Tasks;
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

        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
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

        [HttpGet("museums/{museumId}/exhibits")]
        public async Task<IActionResult> GetAllExhibits(int museumId)
        {
            var response = await _contentService.GetAllExhibitsAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("exhibits/{id}")]
        public async Task<IActionResult> GetExhibit(int id)
        {
            var response = await _contentService.GetExhibitByIdAsync(id);
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

        [HttpGet("exhibits/{id}/translations")]
        public async Task<IActionResult> GetTranslations(int id)
        {
            var response = await _contentService.GetExhibitTranslationsAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("museums/{museumId}/versions")]
        public async Task<IActionResult> CreateContentVersion(int museumId, [FromQuery] string versionNumber, [FromQuery] string description)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateNewContentVersionAsync(museumId, versionNumber, description, userMuseumId);
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

        [HttpGet("museums/{museumId}/packages")]
        public async Task<IActionResult> GetOfflinePackages(int museumId)
        {
            var response = await _contentService.GetOfflinePackagesByMuseumIdAsync(museumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("museums/{museumId}/packages/generate")]
        public async Task<IActionResult> GenerateOfflinePackage(int museumId, [FromBody] CreateOfflinePackageDto dto)
        {
            if (museumId != dto.MuseumId) return BadRequest("Museum ID mismatch");
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.GenerateOfflinePackageAsync(museumId, dto.VersionId, userMuseumId);
            return ResponseParser.Result(response);
        }

        // --- Exhibition Management (Read - Public, Write - Authorized) ---

        [HttpGet("museums/{museumId}/exhibitions")]
        public async Task<IActionResult> GetExhibitions(int museumId)
        {
            var response = await _contentService.GetExhibitionsByMuseumIdAsync(museumId);
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

        // --- Maps Management (Read - Public, Write - Authorized) ---

        [HttpGet("museums/{museumId}/maps")]
        public async Task<IActionResult> GetMuseumMaps(int museumId)
        {
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

        // --- Tour Routes Management (Read - Public, Write - Authorized) ---

        [HttpGet("museums/{museumId}/routes")]
        public async Task<IActionResult> GetTourRoutes(int museumId)
        {
            var response = await _contentService.GetTourRoutesAsync(museumId);
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

        // --- Category Management (Read - Public, Write - Authorized) ---

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] int? museumId)
        {
            var response = await _contentService.GetCategoriesAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var response = await _contentService.GetCategoryByIdAsync(id);
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
        public async Task<IActionResult> GetThemes([FromQuery] int? museumId)
        {
            var response = await _contentService.GetThemesAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpGet("themes/{id}")]
        public async Task<IActionResult> GetTheme(int id)
        {
            var response = await _contentService.GetThemeByIdAsync(id);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPost("themes")]
        public async Task<IActionResult> CreateTheme([FromBody] CreateThemeDto themeDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.CreateThemeAsync(themeDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,ContentManager,SystemAdmin")]
        [HttpPut("themes/{id}")]
        public async Task<IActionResult> UpdateTheme(int id, [FromBody] CreateThemeDto themeDto)
        {
            var userMuseumId = GetCurrentUserMuseumId();
            var response = await _contentService.UpdateThemeAsync(id, themeDto, userMuseumId);
            return ResponseParser.Result(response);
        }

        [Authorize(Roles = "MuseumManager,MuseumContentManager,SystemAdmin")]
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
    }
}
