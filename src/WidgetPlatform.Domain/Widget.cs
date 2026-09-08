using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Domain
{
    public class Widget
    {
        public Guid Id { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public WidgetType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string FieldsJson { get; set; } = "[]";
        public string ButtonText { get; set; } = "Submit";
        public string? DisplayOptionsJson { get; set; }

        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
