using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Application.Services;

namespace WidgetPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WidgetsController : ControllerBase
{
    private readonly IWidgetService _widgetService;

    public WidgetsController(IWidgetService widgetService) => _widgetService = widgetService;

    private string OwnerId =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value; 

    [HttpPost]
    public async Task<IActionResult> Create(CreateWidgetRequest request)
    {
        var widget = await _widgetService.CreateAsync(OwnerId, request);
        return CreatedAtAction(nameof(GetById), new { id = widget.Id }, widget);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var widgets = await _widgetService.GetAllAsync(OwnerId);
        return Ok(widgets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var widget = await _widgetService.GetByIdAsync(OwnerId, id);
        return widget is null ? NotFound() : Ok(widget);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateWidgetRequest request)
    {
        var widget = await _widgetService.UpdateAsync(OwnerId, id, request);
        return widget is null ? NotFound() : Ok(widget);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _widgetService.DeleteAsync(OwnerId, id);
        return deleted ? NoContent() : NotFound();
    }
}