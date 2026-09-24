using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Application.Services
{
    public interface INotificationService
    {
        Task NotifyNewSubmissionAsync(Guid submissionId, Guid widgetId);
    }
}
