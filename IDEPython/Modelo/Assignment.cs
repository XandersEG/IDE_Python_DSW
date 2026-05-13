using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }

        public int IdCourse { get; set; }
        public List<Submission> Submissions { get; set; } = new List<Submission>();
     }
}
