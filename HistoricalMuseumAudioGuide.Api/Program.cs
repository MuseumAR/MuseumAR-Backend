using DotNetEnv;
using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Mappings;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Admin;
using HistoricalMuseumAudioGuide.Service.Services.Analytics;
using HistoricalMuseumAudioGuide.Service.Services.Auth;
using HistoricalMuseumAudioGuide.Service.Services.Content;
using HistoricalMuseumAudioGuide.Service.Services.Media;
using HistoricalMuseumAudioGuide.Service.Services.Ticketing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
builder.Services.AddScoped<IMuseumManagerService, MuseumManagerService>();

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

app.UseAuthentication();
app.UseAuthorization();

// Redirect root to Scalar API Reference UI
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
