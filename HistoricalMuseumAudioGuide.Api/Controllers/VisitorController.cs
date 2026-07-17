using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Visitor;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;
using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitorController : ControllerBase
    {
        private readonly IVisitorService _visitorService;
        private readonly IAnalyticsService _analyticsService;
        private readonly IMuseumResolver _museumResolver;

        public VisitorController(IVisitorService visitorService, IAnalyticsService analyticsService, IMuseumResolver museumResolver)
        {
            _visitorService = visitorService;
            _analyticsService = analyticsService;
            _museumResolver = museumResolver;
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromBody] VisitorSyncDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                userId = id;
            }

            var response = await _visitorService.SyncVisitorAsync(request, userId);
            return ResponseParser.Result(response);
        }

        [HttpPost("track-action")]
        public async Task<IActionResult> TrackAction([FromBody] CreateAnalyticsLogDto request)
        {
            int? visitorId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
            {
                var visitorRes = await _visitorService.GetVisitorByUserIdAsync(id);
                if (visitorRes.StatusCode == 200 && visitorRes.Data is Visitor visitor)
                {
                    visitorId = visitor.Id;
                }
            }

            var response = await _analyticsService.RecordActionAsync(visitorId, request);
            return ResponseParser.Result(response);
        }

        [HttpGet("sync-check")]
        public async Task<IActionResult> CheckForUpdates()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var response = await _visitorService.GetLatestOfflinePackageAsync(museumId);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.GetVisitorProfileAsync(visitor.Id);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("bookmarks")]
        public async Task<IActionResult> GetBookmarks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.GetBookmarksAsync(visitor.Id);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("bookmarks")]
        public async Task<IActionResult> AddBookmark([FromBody] CreateBookmarkDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.AddBookmarkAsync(visitor.Id, request);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpDelete("bookmarks/{exhibitId}")]
        public async Task<IActionResult> RemoveBookmark(int exhibitId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.RemoveBookmarkAsync(visitor.Id, exhibitId);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpGet("visited-exhibits")]
        public async Task<IActionResult> GetVisitedExhibits()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.GetVisitedExhibitsAsync(visitor.Id);
            return ResponseParser.Result(response);
        }

        [Authorize]
        [HttpPost("visited-exhibits")]
        public async Task<IActionResult> TrackVisitedExhibit([FromBody] CreateVisitedExhibitDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            
            int userId = int.Parse(userIdClaim.Value);
            var visitorRes = await _visitorService.GetVisitorByUserIdAsync(userId);
            if (visitorRes.StatusCode != 200 || visitorRes.Data is not Visitor visitor)
                return NotFound(visitorRes.Message);
            
            var response = await _visitorService.TrackVisitedExhibitAsync(visitor.Id, request);
            return ResponseParser.Result(response);
        }
    }
}
