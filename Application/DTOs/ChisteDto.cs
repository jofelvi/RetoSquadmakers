namespace retoSquadmakers.Application.DTOs;

public class CreateChisteDto
{
    public string Texto { get; set; } = string.Empty;
    public List<int> TematicaIds { get; set; } = new();
}

public class UpdateChisteDto
{
    public string Texto { get; set; } = string.Empty;
}