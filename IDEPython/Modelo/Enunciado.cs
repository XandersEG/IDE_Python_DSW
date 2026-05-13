using System;
using System.Collections.Generic;
using System.Text;

namespace IDEPython.Modelo
{
    public class Enunciado
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLimite { get; set; }

        public int CursoId { get; set; }
        public List<Entrega> Entregas { get; set; } = new List<Entrega>();
     }
}
