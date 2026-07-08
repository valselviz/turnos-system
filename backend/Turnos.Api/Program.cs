using Microsoft.EntityFrameworkCore;
using Turnos.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger (documentación interactiva en /swagger)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core + PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("TurnosDb")
    ?? throw new InvalidOperationException("Falta la connection string 'TurnosDb' en appsettings.json.");

builder.Services.AddDbContext<TurnosDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS: habilitamos el origen del frontend (Vite en localhost:5173 por defecto)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

app.Run();
