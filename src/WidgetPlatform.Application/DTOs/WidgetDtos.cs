using System;
using System.Collections.Generic;
using System.Text;
using WidgetPlatform.Domain;

namespace WidgetPlatform.Application.DTOs
{
    public record CreateWidgetRequest(
        WidgetType Type,
        string Title,
        string? Description,
        string FieldsJson,
        string ButtonText,
        string? DisplayOptionsJson
    );

    public record UpdateWidgetRequest(
        string Title,
        string? Description,
        string FieldsJson,
        string ButtonText,
        string? DisplayOptionsJson
    );

    public record WidgetResponse(
        Guid Id,
        WidgetType Type,
        string Title,
        string? Description,
        string FieldsJson,
        string ButtonText,
        string? DisplayOptionsJson,
        int Version,
        DateTime CreatedAt
    );
}
