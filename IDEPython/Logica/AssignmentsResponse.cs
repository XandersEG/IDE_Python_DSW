using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

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
        public DateTime FechaLimite { get; set; }
    }

}
