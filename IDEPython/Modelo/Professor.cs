using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace IDEPython.Modelo
{
    public class Professor : User
    {
        public List<Course> Courses { get; set; } = new List<Course>();

        public Professor(String fn, String sn, String fl, String sl, String mail)
        {
            this.FirstName = fn;
            SecondName = sn;
            LastName1 = fl;
            LastName2 = sl;
            Email = mail;
        }
    }
}
