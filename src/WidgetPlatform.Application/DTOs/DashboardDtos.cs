using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Application.DTOs
{
    public record SubmissionListItem(Guid Id, string DataJson, string? Country, string? City, DateTime CreatedAt);

    public record DashboardStatsResponse(
        int TotalSubmissions,
        Dictionary<string, int> SubmissionsByDay,
        Dictionary<string, int> SubmissionsByCountry
    );
}
