using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using WidgetPlatform.Application.DTOs;
using WidgetPlatform.Domain;

namespace WidgetPlatform.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return new AuthResult(false, string.Join("; ", result.Errors.Select(e => e.Description)));

            return new AuthResult(true, null);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            var validPassword = user is not null && await _userManager.CheckPasswordAsync(user, request.Password);

            if (!validPassword)
                return new AuthResult(false, "Invalid credentials");

            // TODO: generate a real signed JWT here.
            return new AuthResult(true, null, Token: "PLACEHOLDER-NO-REAL-TOKEN-YET");
        }
    }
}
