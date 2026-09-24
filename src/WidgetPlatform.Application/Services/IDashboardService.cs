using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Application.DTOs;

namespace WidgetPlatform.Application.Services
{
    public interface IDashboardService
    {
        Task<List<SubmissionListItem>?> GetSubmissionsAsync(string ownerId, Guid widgetId);
        Task<DashboardStatsResponse?> GetStatsAsync(string ownerId, Guid widgetId);
    }
}
