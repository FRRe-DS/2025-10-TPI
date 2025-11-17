using ComprasAPI.Controllers;
using ComprasAPI.Data;
using ComprasAPI.Models;
using ComprasAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración JWT en appsettings.json
builder.Configuration["Jwt:Key"] = "MI_CLAVE_SECRETA_DE_EXACTAMENTE_32_!!AAAA";
builder.Configuration["Jwt:Issuer"] = "ComprasAPI";
builder.Configuration["Jwt:Audience"] = "ComprasUsuarios";
builder.Services.AddHttpClient();

// ✅ 1. CONFIGURAR STOCK SERVICE
builder.Services.AddHttpClient<IStockService, StockService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var stockApiUrl = config["ExternalApis:Stock:BaseUrl"] ?? "http://localhost:5001";

    client.BaseAddress = new Uri(stockApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ✅ 2. CONFIGURAR LOGÍSTICA SERVICE
builder.Services.AddHttpClient<ILogisticaService, LogisticaService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var logisticaApiUrl = config["ExternalApis:Logistica:BaseUrl"] ?? "http://localhost:5002";

    client.BaseAddress = new Uri(logisticaApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ✅ 3. REGISTRAR SERVICIOS
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ILogisticaService, LogisticaService>();

// AÑADIR JWT CON KEYCLOACK
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configuration = builder.Configuration;

        var authority = configuration["Keycloak:Authority"];
        var audience = configuration["Keycloak:Audience"];
        var requireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Keycloak:RequireHttpsMetadata"]);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = audience,
            ValidIssuer = authority,
            ClockSkew = TimeSpan.FromHours(3)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = e =>
            {
                return Task.CompletedTask;
            },
            OnTokenValidated = e =>
            {
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = e =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = e =>
            {
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 36))));

// Agregar CORS al inicio, después de builder.Services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("https://localhost:4200", "http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ----------------------
// [Swagger] Servicios
// ----------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAngular");

// ----------------------
// [Swagger] Middleware
// ----------------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();