using ComprasAPI.Controllers;
using ComprasAPI.Data;
using ComprasAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración JWT en appsettings.json
builder.Configuration["Jwt:Key"] = "MI_CLAVE_SECRETA_DE_EXACTAMENTE_32_!!AAAA";
builder.Configuration["Jwt:Issuer"] = "ComprasAPI";
builder.Configuration["Jwt:Audience"] = "ComprasUsuarios";

// Agregar servicios de autenticación JWT
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//   options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//ValidateLifetime = true,
//ValidateIssuerSigningKey = true,
//ValidIssuer = builder.Configuration["Jwt:Issuer"],
//ValidAudience = builder.Configuration["Jwt:Audience"],
//IssuerSigningKey = new SymmetricSecurityKey(
//Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//});

//AÑADIR JWT CON KEYCLOACK

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //configure keycloak settings
        var configuration = builder.Configuration;

        var authority = configuration["Keycloak:Authority"];
        var audience = configuration["Keycloak:Audience"];
        var metadataAddress = configuration["Keycloak:MetadataAddress"];
        var requireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

        options.Authority = authority;
        //options.Audience = audience;
        options.MetadataAddress = metadataAddress;
        options.RequireHttpsMetadata = requireHttpsMetadata;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidAudience = audience,
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
