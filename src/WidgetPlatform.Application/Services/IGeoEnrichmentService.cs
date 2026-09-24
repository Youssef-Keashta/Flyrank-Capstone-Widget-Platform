using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Application.Models;

namespace WidgetPlatform.Application.Services
{
    public interface IGeoEnrichmentService
    {
        Task<GeoResult?> EnrichAsync(string ipAddress);
    }
}
