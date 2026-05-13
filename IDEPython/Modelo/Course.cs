using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public String EmailProfessor { get; set; }

        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
 }
