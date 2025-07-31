namespace retoSquadmakers.Domain.Entities;

public class ChisteTematica
{
    public int ChisteId { get; set; }
    public int TematicaId { get; set; }
    
    public Chiste Chiste { get; set; } = null!;
    public Tematica Tematica { get; set; } = null!;
}