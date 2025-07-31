namespace retoSquadmakers.Domain.Entities;

public class Chiste
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int AutorId { get; set; }
    public string Origen { get; set; } = "Local";
    
    public Usuario Autor { get; set; } = null!;
    public ICollection<ChisteTematica> ChisteTematicas { get; set; } = new List<ChisteTematica>();
}