using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Application.DTOs;

namespace WidgetPlatform.Application.Services
{
    public interface IWidgetService
    {
        Task<WidgetResponse> CreateAsync(string ownerId, CreateWidgetRequest request);
        Task<List<WidgetResponse>> GetAllAsync(string ownerId);
        Task<WidgetResponse?> GetByIdAsync(string ownerId, Guid widgetId);
        Task<WidgetResponse?> UpdateAsync(string ownerId, Guid widgetId, UpdateWidgetRequest request);
        Task<bool> DeleteAsync(string ownerId, Guid widgetId);
    }
}
