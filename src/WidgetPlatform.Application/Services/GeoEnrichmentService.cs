using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using WidgetPlatform.Application.Models;

namespace WidgetPlatform.Application.Services
{
    public class GeoEnrichmentService : IGeoEnrichmentService
    {
        private readonly IConfiguration _configuration;

        public GeoEnrichmentService(IConfiguration configuration) => _configuration = configuration;

        public async Task<GeoResult?> EnrichAsync(string ipAddress)
        {
            if (!_configuration.GetValue<bool>("GeoEnrichment:ProviderADown"))
            {
                await Task.Delay(20);
                return new GeoResult("United States", "New York", "ProviderA");
            }

            if (!_configuration.GetValue<bool>("GeoEnrichment:ProviderBDown"))
            {
                await Task.Delay(20);
                return new GeoResult("United States", "San Francisco", "ProviderB");
            }

            return null;
        }
    }
}
