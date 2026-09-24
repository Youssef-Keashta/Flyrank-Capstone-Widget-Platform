using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Data;

namespace WidgetPlatform.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly WidgetPlatformDbContext _db;

        public DashboardService(WidgetPlatformDbContext db) => _db = db;

        private async Task<bool> OwnsWidgetAsync(string ownerId, Guid widgetId) =>
            await _db.Widgets.AnyAsync(w => w.Id == widgetId && w.OwnerId == ownerId);

        public async Task<List<SubmissionListItem>?> GetSubmissionsAsync(string ownerId, Guid widgetId)
        {
            if (!await OwnsWidgetAsync(ownerId, widgetId)) return null;

            return await _db.Submissions
                .Where(s => s.WidgetId == widgetId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SubmissionListItem(s.Id, s.DataJson, s.Country, s.City, s.CreatedAt))
                .ToListAsync();
        }

        public async Task<DashboardStatsResponse?> GetStatsAsync(string ownerId, Guid widgetId)
        {
            if (!await OwnsWidgetAsync(ownerId, widgetId)) return null;

            var submissions = await _db.Submissions.Where(s => s.WidgetId == widgetId).ToListAsync();

            var byDay = submissions
                .GroupBy(s => s.CreatedAt.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Count());

            var byCountry = submissions
                .GroupBy(s => s.Country ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            return new DashboardStatsResponse(submissions.Count, byDay, byCountry);
        }
    }
}
