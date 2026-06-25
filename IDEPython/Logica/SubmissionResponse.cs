using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Logica
{
    internal class SubmissionResponse
    {
        public bool exito { get; set; }

        public int idEntrega { get; set; }
        public string? mensaje { get; set; }
    }
}
