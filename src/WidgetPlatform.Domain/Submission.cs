using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Domain
{
    public class Submission
    {
        public Guid Id { get; set; }

        public Guid WidgetId { get; set; }
        public string OwnerId { get; set; } = string.Empty;

        public string DataJson { get; set; } = "{}";

        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? GeoProvider { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
