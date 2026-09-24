using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Application.DTOs;

namespace WidgetPlatform.Application.Services
{
    public interface ISubmissionService
    {
        Task<SubmissionResult> SubmitAsync(CreateSubmissionRequest request, string? ipAddress);
    }
}
