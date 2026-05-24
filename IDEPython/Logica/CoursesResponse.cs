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
        public string name { get; set; }
        public string code { get; set; }
        public string password { get; set; }
        public string email { get; set; }
    }

}
