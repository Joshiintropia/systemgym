namespace SystemGym.Api.Planes;

public class Membresia
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    
    public Guid SocioId { get; set; }
    public Socio Socio { get; set; } = null;

    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null;
    
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
}