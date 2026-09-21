using Microsoft.AspNetCore.Mvc;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Application.Services;

namespace flyrank_capstone_widget_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionsController(ISubmissionService submissionService) => _submissionService = submissionService;

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubmissionRequest request)
        {
            var result = await _submissionService.SubmitAsync(request);

            if (!result.Succeeded)
                return result.Error == "Widget not found"
                    ? NotFound(new { error = result.Error })
                    : BadRequest(new { error = result.Error });

            return CreatedAtAction(null, result.Result);
        }
    }
}
