using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Submission
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int VersionNumber { get; set; }
        public int AssignmentId { get; set; }
        public int ProjectId { get; set; }
    }
 }
