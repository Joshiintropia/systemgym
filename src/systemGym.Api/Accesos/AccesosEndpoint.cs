using Microsoft.EntityFrameworkCore;
using SystemGym.Api.Data;

namespace SystemGym.Api.Accesos;

public record ValidarAccesoRequest(string SocioId);

public record AccesoResponse(bool Pasa, string Msg);

public static class AccesosEndpoint
{
    public static void MapAcceso(this WebApplication app)
    {
        app.MapPost("/acceso", async (ValidarAccesoRequest req, GymDbContext db, ILogger <AccesoResponse> log) =>
        {
            if (!Guid.TryParse(req.SocioId, out Guid socioId))
            {
                return Results.Ok(new AccesoResponse(false, "Codigo no reconocido"));
            }
            
            var nombre = await db.Socios
                .Where(s => s.Id == socioId)
                .Select(s => s.Nombre)
                .FirstOrDefaultAsync();

            if (nombre is null)
            {
                return Results.Ok(new AccesoResponse(false, "Socio no encontrado"));
            }
            
            var hoy = DateOnly.FromDateTime(DateTime.Now);
            var vigente = await db.Menbresias
                .Where(m => m.SocioId == socioId && m.FechaInicio <= hoy && m.FechaFin >= hoy)
                .OrderByDescending(m => m.FechaFin)
                .Select(m => new { m.FechaFin, Plan = m.Plan.Nombre })
                .FirstOrDefaultAsync();

            if (vigente is null)
            {
                log.LogInformation("Sin acceso al socio {socioId}, membresia vigente?: {vigancia}", socioId, false);
                return Results.Ok(new AccesoResponse(false, "Sin membresia vigente"));
            }
            
            log.LogInformation("Acceso permitido para el socio: {socioId}", socioId);
            return Results.Ok(new AccesoResponse(true, $"Bienvenid@ {nombre}. tu plan {vigente.Plan}, vence el: {vigente.FechaFin:dd/MM/yyyy}"));
        });
    }
}