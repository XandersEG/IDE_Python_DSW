namespace IDEPython.DTOs.Http
{
    public class RegisterRequest
    {
        public string? primerNombre { get; set; }
        public string? segundoNombre { get; set; }
        public string? primerApellido { get; set; }
        public string? segundoApellido { get; set; }
        public string? correo { get; set; }
        public string? contrasena { get; set; }
        public string? confirmarContrasena { get; set; }
    }
}
