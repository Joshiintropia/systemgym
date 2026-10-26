namespace SystemGym.Api.Planes;

public class Plan
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Nombre { get; set; } = "";
    public decimal Precio  { get; set; }
    public int DuracionDias  { get; set; }
    public bool Activo { get; init; } = true;
}