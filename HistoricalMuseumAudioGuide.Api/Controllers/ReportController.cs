using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public ReportController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("museums/{museumId}/dashboard")]
        public async Task<IActionResult> GetDashboardStats(int museumId)
        {
            var response = await _analyticsService.GetDashboardStatsAsync(museumId);
            return ResponseParser.Result(response);
        }
    }
}
