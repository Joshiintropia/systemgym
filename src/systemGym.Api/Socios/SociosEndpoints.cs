using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using SystemGym.Api.Data;

namespace SystemGym.Api.Socios;

public record AltaSocioRequest(string Nombre, string Apellido, string? Telefono, string? Email);

record ActualizarSocioDtoRequest(string? Nombre, string? Apellido, string? Email, string? Telefono);

public static class SociosEndpoints
{
    public static void MapSocios(this WebApplication app)
    {
        var grupo = app.MapGroup("/socios");

        grupo.MapGet("/", async (GymDbContext db) =>
            await db.Socios.OrderBy(s => s.Apellido).ThenBy(s => s.Nombre).ToListAsync()
            );

        grupo.MapGet("/{id:guid}", async (Guid id, GymDbContext db) =>
            await db.Socios.FindAsync(id) is { } socio ? Results.Ok(socio) : Results.NotFound()
        );

        grupo.MapGet("/{socioId:guid}/qr", async (Guid socioId, GymDbContext db) =>
        {
            if (!await db.Socios.AnyAsync(s => s.Id == socioId))
            {
                Results.NotFound();
            }
            
            byte[] qr = PngByteQRCodeHelper.GetQRCode(socioId.ToString(), QRCodeGenerator.ECCLevel.Q, 10);
            return Results.File(qr, "image/png");
        });

        grupo.MapPost("/", async (AltaSocioRequest req, GymDbContext db, ILogger<Socio> log) =>
            {
                if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Apellido))
                {
                    return Results.BadRequest("Nombre y Apellido son obligatorios.");
                }

                var socio = new Socio
                {
                    Nombre = req.Nombre.Trim(),
                    Apellido = req.Apellido.Trim(),
                    Email = req.Email?.Trim(),
                    Telefono = req.Telefono?.Trim()
                };
                
                db.Socios.Add(socio);
                await db.SaveChangesAsync();
                
                log.LogInformation("Alta de socio: {idSocio}", socio.Id);
                return Results.Created($"/socios/{socio.Id}", socio);
            }
        );

        grupo.MapPatch("/{id:guid}", async (Guid id, ActualizarSocioDtoRequest req, GymDbContext db, ILogger<Socio> log) =>
            {
                var socio  = await db.Socios.FindAsync(id);

                if (socio is null)
                {
                    return Results.NotFound();
                }
                
                if (!string.IsNullOrWhiteSpace(req.Nombre)) socio.Nombre = req.Nombre.Trim();
                
                if (!string.IsNullOrWhiteSpace(req.Apellido)) socio.Apellido = req.Apellido.Trim();
                
                if (!string.IsNullOrWhiteSpace(req.Email)) socio.Email = req.Email.Trim();
                
                if (!string.IsNullOrWhiteSpace(req.Telefono)) socio.Telefono = req.Telefono.Trim();
                
                await db.SaveChangesAsync();
                log.LogInformation("Actualizacion de socio: {idSocio}",  socio.Id);
                
                return Results.NoContent();
            }
            
        );



    }
}