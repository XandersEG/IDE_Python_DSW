using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Project
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Submission> Submissions { get; set; } = new List<Submission>();
     }
}
