using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Data;
using WidgetPlatform.Domain;
using Microsoft.EntityFrameworkCore;

namespace WidgetPlatform.Application.Services
{
    public class WidgetService : IWidgetService
    {
        private readonly WidgetPlatformDbContext _db;

        public WidgetService(WidgetPlatformDbContext db) => _db = db;

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
            return await _db.Widgets
                .Where(w => w.OwnerId == ownerId)
                .Select(w => ToResponse(w))
                .ToListAsync();
        }

        public async Task<WidgetResponse?> GetByIdAsync(string ownerId, Guid widgetId)
        {
            var widget = await _db.Widgets
                .FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);

            return widget is null ? null : ToResponse(widget);
        }

        public async Task<WidgetResponse?> UpdateAsync(string ownerId, Guid widgetId, UpdateWidgetRequest request)
        {
            var widget = await _db.Widgets
                .FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);

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
            var widget = await _db.Widgets
                .FirstOrDefaultAsync(w => w.Id == widgetId && w.OwnerId == ownerId);

            if (widget is null) return false;

            _db.Widgets.Remove(widget);
            await _db.SaveChangesAsync();
            return true;
        }

        private static WidgetResponse ToResponse(Widget w) =>
            new(w.Id, w.Type, w.Title, w.Description, w.FieldsJson, w.ButtonText, w.DisplayOptionsJson, w.Version, w.CreatedAt);
    }
}
