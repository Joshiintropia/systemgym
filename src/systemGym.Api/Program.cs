using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using SystemGym.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Creamos la migracion de la tabla socios.
builder.Services.AddDbContext<GymDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")).UseSnakeCaseNamingConvention()
    );

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Registramos el servicio, debe ir antes de buider.Build()
builder.Services.AddHealthChecks(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint de healthy con json personalizado
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            environment = app.Environment.EnvironmentName,
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            memoryUsageMB = GC.GetTotalMemory(false) / 1024,
            cpuCount = Environment.ProcessorCount,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
});

app.Run();

