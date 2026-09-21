using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Application.Models
{
    public record WidgetFieldDefinition(string Name, string Label, string Type, bool Required);
}
