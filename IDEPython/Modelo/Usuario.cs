using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Usuario
    {
        public int Id { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string Correo { get; set; }
        public string Contraseña { get; set; }
    }
}
