using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WidgetPlatform.Application.Services;

namespace flyrank_capstone_widget_platform.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

        private string OwnerId => User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value;

        [HttpGet("widgets/{widgetId}/submissions")]
        public async Task<IActionResult> GetSubmissions(Guid widgetId)
        {
            var result = await _dashboardService.GetSubmissionsAsync(OwnerId, widgetId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("widgets/{widgetId}/stats")]
        public async Task<IActionResult> GetStats(Guid widgetId)
        {
            var result = await _dashboardService.GetStatsAsync(OwnerId, widgetId);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
