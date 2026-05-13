using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Curso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }

        public List<Enunciado> Enunciados { get; set; } = new List<Enunciado>();
    }
}
