using Microsoft.EntityFrameworkCore;

namespace SystemGym.Api.Data;

public class GymDbContext(DbContextOptions<GymDbContext> options) : DbContext(options)
{
    public DbSet<Socio> Socios => Set<Socio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Socio>(e =>
        {
            e.Property(s => s.Nombre).HasMaxLength(255).IsRequired();
            e.Property(s => s.Apellido).HasMaxLength(255).IsRequired();
            e.Property(s => s.Telefono).HasMaxLength(20);
            e.Property(s => s.Email).HasMaxLength(255);
        });
    }
}