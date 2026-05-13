using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IDEPython.Modelo
{
    public class Estudiante : Usuario
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();
     }
}
