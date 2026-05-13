using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Entrega
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int NumeroVersion { get; set; }
        public int EnunciadoId { get; set; }
        public int ProyectoId { get; set; }
    }
}
