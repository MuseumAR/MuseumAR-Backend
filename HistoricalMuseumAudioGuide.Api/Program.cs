using DotNetEnv;
using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Mappings;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Admin;
using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using HistoricalMuseumAudioGuide.Service.Services.Audit;
using HistoricalMuseumAudioGuide.Service.Services.Auth;
using HistoricalMuseumAudioGuide.Service.Services.Content;
using HistoricalMuseumAudioGuide.Service.Services.Media;
using HistoricalMuseumAudioGuide.Service.Services.Navigation;
using HistoricalMuseumAudioGuide.Service.Services.Payment;
using HistoricalMuseumAudioGuide.Service.Services.SystemConfig;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services.Visitor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayOS;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
    });
builder.Services.AddOpenApi();

// Configure CORS
var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
var allowedOrigins = !string.IsNullOrEmpty(allowedOriginsEnv)
    ? allowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : new[] { "http://localhost:3000", "http://localhost:3001", "http://localhost:5173", "http://localhost:8081", "http://localhost:8082" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
builder.Services.AddDbContext<MuseumAudioGuideContext>(options =>
    options.UseSqlServer(connectionString));

// Repository & UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ITicketingService, TicketingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMuseumManagerService, MuseumManagerService>();
builder.Services.AddScoped<IVisitorService, VisitorService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();
builder.Services.AddScoped<IMuseumResolver, MuseumResolver>();
builder.Services.AddScoped<INavigationService, NavigationService>();

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Secret"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT Secret is missing")))
        };
    });

// Đăng ký PayOSClient Singleton
builder.Services.AddSingleton<PayOSClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    // Đọc ưu tiên từ Biến môi trường (.env), nếu không có mới tìm trong Configuration
    var clientId = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID")
                   ?? config["PAYOS_CLIENT_ID"]
                   ?? throw new ArgumentNullException("PAYOS_CLIENT_ID missing");

    var apiKey = Environment.GetEnvironmentVariable("PAYOS_API_KEY")
                 ?? config["PAYOS_API_KEY"]
                 ?? throw new ArgumentNullException("PAYOS_API_KEY missing");

    var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY")
                      ?? config["PAYOS_CHECKSUM_KEY"]
                      ?? throw new ArgumentNullException("PAYOS_CHECKSUM_KEY missing");

    return new PayOSClient(clientId, apiKey, checksumKey);
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Redirect root to Scalar API Reference UI
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
