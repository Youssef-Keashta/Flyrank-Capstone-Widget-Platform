using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WidgetPlatform.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task NotifyNewSubmissionAsync(Guid submissionId, Guid widgetId)
        {
            if (_configuration.GetValue<bool>("EmailNotification:SimulateFailure"))
                throw new InvalidOperationException("Simulated email provider outage.");

            _logger.LogInformation("Email sent: new submission {SubmissionId} for widget {WidgetId}", submissionId, widgetId);
            return Task.CompletedTask;
        }
    }
}
