using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Petoria.Core.Contracts;
using Petoria.Infrastructure.Data;
using Petoria.Infrastructure.Data.Entities;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel and Form options for larger file uploads (30MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024;
});

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new
            {
                message = errors.Values.FirstOrDefault()?.FirstOrDefault() ?? "Грешка при валидация",
                errors = errors
            });
        };
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<Petoria.Core.Contracts.IAuthService, Petoria.Core.Services.AuthService>();
builder.Services.AddScoped<Petoria.Core.Contracts.IPhotoService, Petoria.Core.Services.CloudinaryService>();
builder.Services.AddScoped<Petoria.Core.Contracts.IEmailService, Petoria.Core.Services.EmailService>();
builder.Services.AddScoped<Petoria.Core.Contracts.IPricingService, Petoria.Core.Services.PricingService>();

// SignalR
builder.Services.AddSignalR();

// Configure strong-typed settings objects
builder.Services.Configure<Petoria.Core.Models.Email.SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// Response Compression (Gzip + Brotli)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "text/plain",
        "text/html",
        "text/css",
        "application/javascript"
    });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Stricter rate limit for authentication endpoints (brute force protection)
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.AutoReplenishment = true;
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});



// CORS Configuration
var allowedOrigins = builder.Configuration["AllowedOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:5174" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        corsBuilder => corsBuilder
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization", "x-requested-with", "x-signalr-user-agent")
            .AllowCredentials()); // Allow cookies
});

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    
    // Read token from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices
                .GetRequiredService<UserManager<ApplicationUser>>();
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail("Invalid token");
                return;
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || await userManager.IsLockedOutAsync(user))
            {
                context.Fail("User is blocked");
                context.HttpContext.Response.StatusCode = 401;
            }
        }
    };
});

var app = builder.Build();

// Apply pending migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Initialize roles and admin user
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.InitializeRolesAndAdminAsync();
}

// Seed demo data (only runs in Development when the database is empty)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await Petoria.Infrastructure.Data.DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Response Compression
app.UseResponseCompression();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(self)");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' https://accounts.google.com https://apis.google.com https://js.stripe.com; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://unpkg.com; " +
        "img-src 'self' data: blob: https://res.cloudinary.com https://*.tile.openstreetmap.org; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "frame-src https://accounts.google.com https://js.stripe.com; " +
        "connect-src 'self' https://accounts.google.com https://api.bigdatacloud.net https://*.stripe.com");
    await next();
});

app.UseCors("AllowReactApp");

// Rate Limiting
app.UseRateLimiter();

// Enable serving static files from wwwroot
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Petoria.Hubs.LiveUsersHub>("/hubs/liveusers");

app.Run();
