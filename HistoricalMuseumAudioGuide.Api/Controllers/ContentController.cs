using Microsoft.AspNetCore.Mvc;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Admin.Content;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

        // --- Exhibit Management ---

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

        [HttpPost("exhibits")]
        public async Task<IActionResult> CreateExhibit([FromBody] CreateExhibitDto exhibitDto)
        {
            var response = await _contentService.CreateExhibitAsync(exhibitDto);
            return ResponseParser.Result(response);
        }

        [HttpPut("exhibits/{id}")]
        public async Task<IActionResult> UpdateExhibit(int id, [FromBody] CreateExhibitDto exhibitDto)
        {
            var response = await _contentService.UpdateExhibitAsync(id, exhibitDto);
            return ResponseParser.Result(response);
        }

        [HttpDelete("exhibits/{id}")]
        public async Task<IActionResult> DeleteExhibit(int id)
        {
            var response = await _contentService.DeleteExhibitAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpPost("exhibits/{id}/publish")]
        public async Task<IActionResult> PublishExhibit(int id)
        {
            var response = await _contentService.PublishExhibitAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpPost("exhibits/{id}/unpublish")]
        public async Task<IActionResult> UnpublishExhibit(int id)
        {
            var response = await _contentService.UnpublishExhibitAsync(id);
            return ResponseParser.Result(response);
        }

        // --- Media Management ---

        [HttpPost("exhibits/{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, [FromForm] IFormFile file, [FromForm] string caption)
        {
            var response = await _contentService.UploadExhibitImageAsync(id, file, caption);
            return ResponseParser.Result(response);
        }

        [HttpPost("exhibits/{id}/upload-audio")]
        public async Task<IActionResult> UploadAudio(int id, [FromForm] string languageCode, [FromForm] IFormFile file)
        {
            var response = await _contentService.UploadExhibitAudioAsync(id, languageCode, file);
            return ResponseParser.Result(response);
        }

        // --- Translation & Versions ---

        [HttpGet("exhibits/{id}/translations")]
        public async Task<IActionResult> GetTranslations(int id)
        {
            var response = await _contentService.GetExhibitTranslationsAsync(id);
            return ResponseParser.Result(response);
        }

        [HttpPost("museums/{museumId}/versions")]
        public async Task<IActionResult> CreateContentVersion(int museumId, [FromQuery] string versionNumber, [FromQuery] string description)
        {
            var response = await _contentService.CreateNewContentVersionAsync(museumId, versionNumber, description);
            return ResponseParser.Result(response);
        }

        // --- AR Asset Management ---

        [HttpGet("exhibits/{exhibitId}/ar-assets")]
        public async Task<IActionResult> GetArAssets(int exhibitId)
        {
            var response = await _contentService.GetArAssetsByExhibitIdAsync(exhibitId);
            return ResponseParser.Result(response);
        }

        [HttpPost("exhibits/{exhibitId}/ar-assets/upload")]
        public async Task<IActionResult> AddArAsset(int exhibitId, [FromForm] string assetType, [FromForm] IFormFile file, [FromForm] string? description)
        {
            var response = await _contentService.AddArAssetAsync(exhibitId, assetType, file, description);
            return ResponseParser.Result(response);
        }

        [HttpDelete("ar-assets/{id}")]
        public async Task<IActionResult> DeleteArAsset(int id)
        {
            var response = await _contentService.DeleteArAssetAsync(id);
            return ResponseParser.Result(response);
        }

        // --- Offline Package Management ---

        [HttpGet("museums/{museumId}/packages")]
        public async Task<IActionResult> GetOfflinePackages(int museumId)
        {
            var response = await _contentService.GetOfflinePackagesByMuseumIdAsync(museumId);
            return ResponseParser.Result(response);
        }

        [HttpPost("museums/{museumId}/packages/generate")]
        public async Task<IActionResult> GenerateOfflinePackage(int museumId, [FromBody] CreateOfflinePackageDto dto)
        {
            if (museumId != dto.MuseumId) return BadRequest("Museum ID mismatch");
            var response = await _contentService.GenerateOfflinePackageAsync(museumId, dto.VersionId);
            return ResponseParser.Result(response);
        }
    }
}
