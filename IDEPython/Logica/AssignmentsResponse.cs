using System.Text.Json.Serialization;

namespace IDEPython.Logica
{
    public class AssignmentsResponse
    {
        public bool exito { get; set; }

        public AssignmentInfo[]? tareas { get; set; }
    }

    public class AssignmentInfo
    {
        public string? idEnunciado { get; set; }
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        [JsonPropertyName("FechaLimite")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime FechaLimite { get; set; }
    }

}
