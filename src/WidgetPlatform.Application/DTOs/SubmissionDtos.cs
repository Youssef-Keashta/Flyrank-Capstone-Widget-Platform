using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Application.DTOs
{
    public record CreateSubmissionRequest(Guid WidgetId, Dictionary<string, string> Data, string? Website = null);

    public record SubmissionResponse(Guid Id, Guid WidgetId, DateTime CreatedAt);

    public record SubmissionResult(bool Succeeded, string? Error, SubmissionResponse? Result = null);
}
