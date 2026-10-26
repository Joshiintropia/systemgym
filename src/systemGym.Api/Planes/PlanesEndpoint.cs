using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SystemGym.Api.Data;

namespace SystemGym.Api.Planes;

// Como se espera recibir
public record AltaPlanRequest(string Nombre, decimal Precio, int DuracionDias);

public record AsignarPlanRequest(Guid PlanId);

public record ActualizarPlanDtoRequest(string? Nombre, decimal? Precio, int? DuracionDias);


public static class PlanesEndpoint
{
    public static void MapPlanes(this WebApplication app)
    {
        var grupo = app.MapGroup("/planes");

        grupo.MapGet("/", async (GymDbContext db) =>
            await db.Plan.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync()
        );

        grupo.MapGet("/{id:guid}", async (Guid id, GymDbContext db) =>
            await db.Plan.FindAsync(id) is { } plan ? Results.Ok(plan) : Results.NotFound()
        );

        grupo.MapPost("/", async (AltaPlanRequest req, GymDbContext db, ILogger<Plan> log) =>
        {
            /*if (string.IsNullOrWhiteSpace(req.Nombre) || req.Precio.CompareTo(0) != 0 || req.DuracionDias.CompareTo(0) != 0)
            {
                return Results.BadRequest("Nombre, Precio o Duracion del plan son obligatorios");
            }*/
            
            if (string.IsNullOrWhiteSpace(req.Nombre)) return Results.BadRequest("Nombre es obligatorio");
            if (req.Precio.CompareTo(0) > 1) return Results.BadRequest("Precio es obligatorio");
            if (req.DuracionDias <= 0) return Results.BadRequest("Duracion dias es obligatorio");

            var plan = new Plan
            {
                Nombre = req.Nombre.Trim(),
                Precio = req.Precio,
                DuracionDias = req.DuracionDias,
            };
            
            db.Plan.Add(plan);
            await db.SaveChangesAsync();
            
            log.LogInformation("Alta de plan: {PlanId}",  plan.Id);
            return Results.Created($"/planes/{plan.Id}", plan);

        });

        grupo.MapPatch("/planes/{id:guid}", async (Guid id, ActualizarPlanDtoRequest req, GymDbContext db, ILogger<Plan> log) =>
        {
            var plan = await db.Plan.FindAsync(id);

            if (plan is null)
            {
                return Results.NotFound();
            }
            
            if (!string.IsNullOrWhiteSpace(req.Nombre)) plan.Nombre = req.Nombre.Trim();

            if (req.Precio is not null && req.Precio.Value.CompareTo(plan.Precio) != 0) plan.Precio = req.Precio.Value;
            
            if (req.DuracionDias is not null && !req.DuracionDias.Equals(plan.DuracionDias)) plan.DuracionDias = req.DuracionDias.Value;
            
            await db.SaveChangesAsync();
            log.LogInformation("Actualizacion de plan: {planId}", plan.Id);
            
            return Results.NoContent();
            
        });
        
        // Asignar el plan al socio
        grupo.MapPost("/socios/{socioId:guid}/membresia",
            async (Guid socioId, AsignarPlanRequest req, GymDbContext db, ILogger<Membresia> log) =>
            {
                log.LogInformation($"Request recibido: {req}");
                if (!await db.Socios.AnyAsync(s => s.Id == socioId))
                {
                    Results.NotFound("Socio No encontrado");
                }

                var plan = await db.Plan.FindAsync(req.PlanId);
                if (plan is null || !plan.Activo)
                {
                    Results.NotFound("Plan no valido");
                }

                var inicio= DateOnly.FromDateTime(DateTime.Now);

                var membresia = new Membresia
                {
                    SocioId = socioId,
                    PlanId = plan!.Id,
                    FechaInicio = inicio,
                    FechaFin = inicio.AddDays(plan.DuracionDias - 1),
                };

                db.Menbresias.Add(membresia);
                await db.SaveChangesAsync();

                log.LogInformation("Membresia {MembresiaId} para socio {SocioId} con plan {PlanId}", membresia.Id,
                    socioId, plan.Id);
                return Results.Created($"/socios/{socioId}/membresias/{membresia.Id}",
                    new
                    {
                        membresia.Id, membresia.SocioId, Plan = plan.Nombre, membresia.FechaInicio, membresia.FechaFin
                    });
            });

        grupo.MapGet("/socio/{socioId:guid}/membresia", async (Guid socioId, GymDbContext db) =>
        {
            await db.Menbresias.Where(
                m => m.PlanId == socioId).OrderBy(m => m.FechaFin)
                .Select(m => new
                {
                    m.Id,
                    Plan = m.Plan.Nombre,
                    m.FechaInicio,
                    m.FechaFin
                }).ToArrayAsync();
        });

    }    
}