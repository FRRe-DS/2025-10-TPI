using ComprasAPI.Controllers;
using ComprasAPI.Data;
using ComprasAPI.Models;
using ComprasAPI.Services;  // ← AÑADIR este using
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

// ✅ 1. CONFIGURAR STOCK SERVICE (AGREGAR ESTO)
builder.Services.AddHttpClient<IStockService, StockService>((provider, client) =>
{
    // Usar configuración de appsettings.json o valor por defecto
    var config = provider.GetRequiredService<IConfiguration>();
    var stockApiUrl = config["ExternalApis:Stock:BaseUrl"] ?? "http://localhost:5001";

    client.BaseAddress = new Uri(stockApiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ✅ 2. REGISTRAR EL SERVICIO (AGREGAR ESTO)
builder.Services.AddScoped<IStockService, StockService>();

//AÑADIR JWT CON KEYCLOACK

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //configure keycloak settings
        var configuration = builder.Configuration;

        var authority = configuration["Keycloak:Authority"];
        var audience = configuration["Keycloak:Audience"];
        //var metadataAddress = configuration["Keycloak:MetadataAddress"];
        var requireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

        options.Authority = authority;
        options.Audience = audience;
        //options.MetadataAddress = metadataAddress;
        options.RequireHttpsMetadata = options.RequireHttpsMetadata = bool.Parse(builder.Configuration["Keycloak:RequireHttpsMetadata"]);

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
            policy.WithOrigins("https://localhost:4200", "http://localhost:4200") // URL de tu Angular
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// ----------------------
// [Swagger] Servicios
// ----------------------
builder.Services.AddEndpointsApiExplorer();   // [Swagger] Explora endpoints para generar la doc
builder.Services.AddSwaggerGen();             // [Swagger] Generador de OpenAPI/Swagger

var app = builder.Build();

app.UseCors("AllowAngular");



// ----------------------
// [Swagger] Middleware
//     Podés dejarlo siempre activo o solo en Development (descomentar el if si preferís).
// ----------------------

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();                             // [Swagger] Publica /swagger/v1/swagger.json
app.UseSwaggerUI();                           // [Swagger] UI en /swagger
// }

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
