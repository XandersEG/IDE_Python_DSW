using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IDEPython.Logica
{
    public class CoursesResponse
    {
        public bool exito { get; set; }

        public CourseInfo[] cursos { get; set; }
    }

    public class CourseInfo
    {
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string CorreoUsuarioProfesor { get; set; }
    }

}
