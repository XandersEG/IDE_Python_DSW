using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Windows;
using IDEPython.Modelo;

namespace IDEPython
{
    public partial class VistaCursos : Window
    {
        Usuario user;
        public String userName { get; set; }

        public VistaCursos(Usuario user)
        {
            this.user = user;
            userName = user.FirstName;

            InitializeComponent();
            CargarCursosEstudiante();
            
            this.DataContext = this;
            
        }

        private void CargarCursosEstudiante()
        {
            //Still missing logic to load courses acording to the student email
            List<Curso> cursos = new List<Curso>
            {
                new Curso { Id = 1, Codigo = "IC001", Nombre = "Introducción a la Programación" },
                new Curso { Id = 2, Codigo = "IC002", Nombre = "Taller de Programación" },
                new Curso { Id = 3, Codigo = "IC101", Nombre = "POO" }
            };
            icCursos.ItemsSource = cursos;
        }

        
    }
}