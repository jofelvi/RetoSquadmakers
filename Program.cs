using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using retoSquadmakers.Infrastructure.Persistence;
using retoSquadmakers.Application.DTOs;
using retoSquadmakers.Domain.Entities;
using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;
using retoSquadmakers.Application.Interfaces;
using retoSquadmakers.Application.UseCases;
using retoSquadmakers.Application.Services;
using retoSquadmakers.Infrastructure.Security;
using retoSquadmakers.Presentation.Controllers;
using retoSquadmakers.Infrastructure.ExternalServices;
using retoSquadmakers.Infrastructure.BackgroundServices;
using retoSquadmakers.Application.EventHandlers;
using retoSquadmakers.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register Hexagonal Architecture services
// Infrastructure - Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IChisteRepository, ChisteRepository>();
builder.Services.AddScoped<ITematicaRepository, TematicaRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
builder.Services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();

// Infrastructure - External Services
builder.Services.AddScoped<IJwtTokenGenerator, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// HttpClient services for external APIs
builder.Services.AddHttpClient<IChuckNorrisApiService, ChuckNorrisApiService>();
builder.Services.AddHttpClient<IDadJokeApiService, DadJokeApiService>();

// EJERCICIO 3 - Sistema de Notificaciones con Inyección de Dependencias
// Configurar EmailNotificador como implementación por defecto de INotificador
builder.Services.AddScoped<EmailNotificador>();
builder.Services.AddScoped<SmsNotificador>();
builder.Services.AddScoped<INotificador>(provider => provider.GetRequiredService<EmailNotificador>());

// ServicioDeAlertas que depende de INotificador
builder.Services.AddScoped<ServicioDeAlertas>();

// Notification Providers (sistema complejo - opcional)
builder.Services.AddScoped<INotificationProvider, EmailNotificationProvider>();
builder.Services.AddScoped<INotificationProvider, SmsNotificationProvider>();
builder.Services.AddScoped<INotificationProvider, PushNotificationProvider>();

// Notification Services (sistema complejo - opcional)
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

// Event Handlers
builder.Services.AddScoped<ChisteCreadoEventHandler>();

// Background Services
builder.Services.AddHostedService<NotificationProcessorService>();

// Application - Domain Services
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IChisteService, ChisteService>();

// Application - Use Cases
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<CreateOrUpdateUserFromOAuthUseCase>();
builder.Services.AddScoped<GetUserInfoUseCase>();
builder.Services.AddScoped<CreateChisteUseCase>();
builder.Services.AddScoped<GetChistesUseCase>();

// Add controllers support
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "RetoSquadmakers API", 
        Version = "v1",
        Description = "API de Chistes con Autenticación OAuth 2.0 y JWT"
    });
    
    // Configuración para JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints using controllers
app.MapHomeEndpoints();
app.MapAuthEndpoints();
// app.MapChisteEndpoints(); // Disabled - using new ChistesController instead
app.MapControllers(); // Add support for controller-based endpoints

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
