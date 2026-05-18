using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Logica
{
    public class LoginResponse
    {
        public bool exito { get; set; }

        public string token { get; set; }

        public Usuario usuario { get; set; }
    }

    public class Usuario
    {
        public string correo { get; set; }

        public string nombre { get; set; }

        public string rol { get; set; }
    }
}
