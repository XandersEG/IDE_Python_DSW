using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IDEPython.Modelo
{
    public class Estudiante : Usuario
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();

        public Estudiante(String fn, String sn, String ln1, String ln2, String mail)
        {
            this.FirstName = fn;
            SecondName = sn;
            LastName1 = ln1;
            LastName2 = ln2;
            Email = mail;
        }
    }
}
