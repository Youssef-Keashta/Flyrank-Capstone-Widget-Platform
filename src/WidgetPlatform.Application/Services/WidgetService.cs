using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Data;
using WidgetPlatform.Domain;

namespace WidgetPlatform.Application.Services;

public class WidgetService : IWidgetService
{
    private readonly WidgetPlatformDbContext _db;
    private readonly IConfiguration _configuration;

    public WidgetService(WidgetPlatformDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<WidgetResponse> CreateAsync(string ownerId, CreateWidgetRequest request)
    {
        var widget = new Widget
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Type = request.Type,
            Title = request.Title,
            Description = request.Description,
            FieldsJson = request.FieldsJson,
            ButtonText = request.ButtonText,
            DisplayOptionsJson = request.DisplayOptionsJson,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        _db.Widgets.Add(widget);
        await _db.SaveChangesAsync();

        return ToResponse(widget);
    }

    public async Task<List<WidgetResponse>> GetAllAsync(string ownerId)
    {
        var widgets = await _db.Widgets.Where(w => w.OwnerId == ownerId).ToListAsync();
        return widgets.Select(ToResponse).ToList();
    }

    public async Task<WidgetResponse?> GetByIdAsync(string ownerId, Guid widgetId)
    {
        var widget = await _db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);
        return widget is null ? null : ToResponse(widget);
    }

    public async Task<WidgetResponse?> UpdateAsync(string ownerId, Guid widgetId, UpdateWidgetRequest request)
    {
        var widget = await _db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);
        if (widget is null) return null;

        widget.Title = request.Title;
        widget.Description = request.Description;
        widget.FieldsJson = request.FieldsJson;
        widget.ButtonText = request.ButtonText;
        widget.DisplayOptionsJson = request.DisplayOptionsJson;
        widget.Version += 1;

        await _db.SaveChangesAsync();
        return ToResponse(widget);
    }

    public async Task<bool> DeleteAsync(string ownerId, Guid widgetId)
    {
        var widget = await _db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);
        if (widget is null) return false;

        _db.Widgets.Remove(widget);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<WidgetConfigResponse?> GetPublicConfigAsync(Guid widgetId)
    {
        var widget = await _db.Widgets.FirstOrDefaultAsync(w => w.Id == widgetId);
        if (widget is null) return null;

        return new WidgetConfigResponse(
            widget.Id, widget.Type.ToString(), widget.Title, widget.Description,
            widget.FieldsJson, widget.ButtonText, widget.DisplayOptionsJson, widget.Version);
    }

    private WidgetResponse ToResponse(Widget w)
    {
        var baseUrl = _configuration["App:PublicBaseUrl"];
        var snippet = $"<script src=\"{baseUrl}/widget.v1.js?id={w.Id}\"></script>";

        return new(w.Id, w.Type, w.Title, w.Description, w.FieldsJson, w.ButtonText,
            w.DisplayOptionsJson, w.Version, w.CreatedAt, snippet);
    }
}