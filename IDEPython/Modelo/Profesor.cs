using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IDEPython.Modelo
{
    public class Profesor : Usuario
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();

        public Profesor(String fn, String sn, String fl, String sl, String mail)
        {
            this.FirstName = fn;
            SecondName = sn;
            LastName1 = fl;
            LastName2 = sl;
            Email = mail;
        }
    }
}
