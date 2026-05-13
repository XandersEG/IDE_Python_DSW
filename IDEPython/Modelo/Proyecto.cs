using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Proyecto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RutaArchivo { get; set; }
        public List<Entrega> Entregas { get; set; } = new List<Entrega>();
    }
}
