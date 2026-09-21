using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Application.Models;
using WidgetPlatform.Data;
using WidgetPlatform.Domain;

namespace WidgetPlatform.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly WidgetPlatformDbContext _db;

        public SubmissionService(WidgetPlatformDbContext db) => _db = db;

        public async Task<SubmissionResult> SubmitAsync(CreateSubmissionRequest request)
        {
            var widget = await _db.Widgets.FindAsync(request.WidgetId);
            if (widget is null)
                return new SubmissionResult(false, "Widget not found");

            var fields = JsonSerializer.Deserialize<List<WidgetFieldDefinition>>(widget.FieldsJson, JsonOptions)
             ?? new List<WidgetFieldDefinition>();

            var errors = new List<string>();
            foreach (var field in fields)
            {
                var hasValue = request.Data.TryGetValue(field.Name, out var value) && !string.IsNullOrWhiteSpace(value);

                if (field.Required && !hasValue)
                    errors.Add($"'{field.Name}' is required.");

                if (hasValue && field.Type == "email" && !value!.Contains('@'))
                    errors.Add($"'{field.Name}' must be a valid email.");
            }

            if (errors.Count > 0)
                return new SubmissionResult(false, string.Join(" ", errors));

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                WidgetId = widget.Id,
                OwnerId = widget.OwnerId, // denormalized, per your design doc
                DataJson = JsonSerializer.Serialize(request.Data),
                CreatedAt = DateTime.UtcNow
            };

            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            return new SubmissionResult(true, null, new SubmissionResponse(submission.Id, submission.WidgetId, submission.CreatedAt));
        }
    }
}
