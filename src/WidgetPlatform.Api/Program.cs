using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WidgetPlatform.Application.Services;
using WidgetPlatform.Data;
using WidgetPlatform.Domain;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace WidgetPlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<WidgetPlatformDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<WidgetPlatformDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IWidgetService, WidgetService>();
            builder.Services.AddScoped<ISubmissionService, SubmissionService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
                };
            });

            const string PublicWidgetCorsPolicy = "PublicWidgetPolicy";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(PublicWidgetCorsPolicy, policy =>
                {
                    policy.WithOrigins("http://localhost:5500")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("SubmissionPolicy", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromSeconds(10);
                    opt.QueueLimit = 0;
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please try again shortly." }, token);
                };
            });

            builder.Services.AddScoped<IGeoEnrichmentService, GeoEnrichmentService>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 16 * 1024;
            });

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Microsoft.AspNetCore.Http.BadHttpRequestException ex)
                {
                    context.Response.StatusCode = ex.StatusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { error = "Request payload too large or malformed." });
                }
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.MapGet("/widget.v1.js", (HttpContext ctx) =>
            {
                ctx.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                return Results.Text(WidgetPlatform.Api.StaticContent.WidgetScript.Content, "application/javascript");
            });

            app.Run();
        }
    }
}
