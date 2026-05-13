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

            CargarProyectos();

            
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


        private void btnVolverAlLogin_Click(object sender, RoutedEventArgs e)
        {
            VistaLogin ventanaLogin = new VistaLogin();
            ventanaLogin.Show();
            this.Close();
        }

        private void VerTareasCurso_Click(object sender, RoutedEventArgs e) {
            var btn = sender as System.Windows.Controls.Button;
            if (btn != null)
            {
                var curso = btn.DataContext as Course;
                if (curso != null)
                {
                    VistaTareas ventanaTareas = new VistaTareas(curso);
            ventanaTareas.Show();
            this.Close();
        }
            }
        }

        private void IrAlIDE_Click(object sender, RoutedEventArgs e)
        {
            IDE ventanaIDE = new IDE();
            ventanaIDE.Show();
            this.Close();
        }

        private void CargarProyectos()
        {
            List<Enunciado> proyectos = new List<Enunciado>();

            proyectos.Add(new Enunciado
            {
                Titulo = "Crear Proyecto",
                Descripcion = "+",
                Id = -1 
            });

            proyectos.Add(new Enunciado
            {
                Id = 101, 
                Titulo = "Tareíta 1",
                Descripcion = "Programas Recursivo"
            });


            icProyectos.ItemsSource = proyectos;
        }
        
    }
}