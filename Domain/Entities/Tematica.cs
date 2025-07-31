namespace retoSquadmakers.Domain.Entities;

public class Tematica
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    
    public ICollection<ChisteTematica> ChisteTematicas { get; set; } = new List<ChisteTematica>();
}