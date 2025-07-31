namespace retoSquadmakers.Application.DTOs;

public class ChisteResponse
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string Origen { get; set; } = string.Empty;
    public AutorInfo Autor { get; set; } = new();
    public List<TematicaInfo> Tematicas { get; set; } = new();

    public class AutorInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TematicaInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}