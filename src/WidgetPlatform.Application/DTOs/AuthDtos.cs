using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetPlatform.Application.DTOs
{
    public record RegisterRequest(string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record AuthResult(bool Succeeded, string? Error, string? Token = null);
}
