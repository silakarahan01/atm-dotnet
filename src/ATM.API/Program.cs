using System.Text;
using ATM.API.Middleware;
using ATM.Application;
using ATM.Infrastructure;
using ATM.Infrastructure.Data;
using ATM.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Uygulama (CQRS) + Altyapı (EF Core/PostgreSQL, repo'lar, güvenlik) katmanları
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT kimlik doğrulama
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
        };
    });
builder.Services.AddAuthorization();

// Web API + OpenAPI (Scalar) + standart hata yönetimi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Açılışta otomatik migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DatabaseSeeder.Seed(db);
}

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "ATM API";
    options.AddHttpAuthentication("Bearer", http =>
    {
        http.Token = "buraya-login-sonrasi-gelen-token";
    });
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Entegrasyon testlerinin WebApplicationFactory ile erişebilmesi için
public partial class Program;
