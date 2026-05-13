using System.Collections.Generic;
using System.Windows;
using IDEPython.Modelo;

namespace IDEPython
{
    public partial class VistaCursos : Window
    {
        public VistaCursos()
        {
            InitializeComponent();
            CargarCursosEstudiante();
        }

        private void CargarCursosEstudiante()
        {
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