namespace retoSquadmakers.Application.DTOs;

public class CreateChisteRequest
{
    public string Texto { get; set; } = string.Empty;
    public string Origen { get; set; } = "Local";
    public List<int> TematicaIds { get; set; } = new List<int>();
}