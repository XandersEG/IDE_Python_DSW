namespace IDEPython.DTOs.Http
{
    public class CoursesResponse
    {
        public bool exito { get; set; }

        public CourseInfo[]? cursos { get; set; }
    }

    public class CourseInfo
    {
        public string? Nombre { get; set; }
        public string? Codigo { get; set; }
        public string? CorreoUsuarioProfesor { get; set; }
    }

}
