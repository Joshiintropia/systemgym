namespace SystemGym.Api;

public class Socio
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public String Nombre { get; set; } = "";
    public String Apellido { get; set; } = "";
    public String? Email { get; set; }
    public String? Telefono { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

}