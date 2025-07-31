namespace retoSquadmakers.Application.DTOs;

public class ChisteFilterRequest
{
    public int? MinPalabras { get; set; }
    public string? Contiene { get; set; }
    public int? AutorId { get; set; }
    public int? TematicaId { get; set; }
}