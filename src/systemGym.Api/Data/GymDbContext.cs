using Microsoft.EntityFrameworkCore;
using SystemGym.Api.Planes;

namespace SystemGym.Api.Data;

public class GymDbContext(DbContextOptions<GymDbContext> options) : DbContext(options)
{
    public DbSet<Socio> Socios => Set<Socio>();

    public DbSet<Plan> Plan => Set<Plan>();

    public DbSet<Membresia> Menbresias => Set<Membresia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Socio>(e =>
        {
            e.Property(s => s.Nombre).HasMaxLength(255).IsRequired();
            e.Property(s => s.Apellido).HasMaxLength(255).IsRequired();
            e.Property(s => s.Telefono).HasMaxLength(20);
            e.Property(s => s.Email).HasMaxLength(255);
        });

        modelBuilder.Entity<Plan>(e =>
        {
            e.Property(p => p.Nombre).HasMaxLength(255).IsRequired();
            e.Property(p => p.Precio).HasPrecision(10, 2).IsRequired();
            e.Property(p => p.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Membresia>(e =>
        {
            e.HasIndex(m => new { m.SocioId, m.FechaFin });
        });
    }
}