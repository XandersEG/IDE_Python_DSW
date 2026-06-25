namespace IDEPython.DTOs.Models
{
    public class Course
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Password { get; set; }

        public String? EmailProfessor { get; set; }

        public List<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
 }
