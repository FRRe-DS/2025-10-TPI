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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 36))));


var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// ===== Usuario de prueba =====
LoginController.AddTestUser(new RegisterRequest
{
    FirstName = "Juan",           
    LastName = "Pérez",            
    Email = "juan@mail.com",
    Password = "123456",
    RepeatPassword = "123456"       
});

app.Run();
