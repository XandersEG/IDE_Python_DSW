using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Windows;
using IDEPython.Modelo;

namespace IDEPython
{
    public partial class VistaCursos : Window
    {
        User user;
        public String userName { get; set; }

        public VistaCursos(User user)
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
            List<Course> cursos = new List<Course>
            {
                new Course { Id = 1, Code = "IC001", Name = "Introducción a la Programación" },
                new Course { Id = 2, Code = "IC002", Name = "Taller de Programación" },
                new Course { Id = 3, Code = "IC101", Name = "POO" }
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
                var course = btn.DataContext as Course;
                if (course != null)
                {
                    VistaTareas ventanaTareas = new VistaTareas(course, this.user);
                    ventanaTareas.Show();
                    this.Close();
                }
            }
        }

        private void IrAlIDE_Click(object sender, RoutedEventArgs e)
        {
            IDE ventanaIDE = new IDE(this.user);
            ventanaIDE.Show();
            this.Close();
        }

        private void CargarProyectos()
        {
            List<Assignment> proyectos = new List<Assignment>();

            proyectos.Add(new Assignment
            {
                Title = "Crear Proyecto",
                Description = "+",
                Id = -1 
            });

            proyectos.Add(new Assignment
            {
                Id = 101,
                Title = "Tareíta 1",
                Description = "Programas Recursivo"
            });


            icProyectos.ItemsSource = proyectos;
        }
        
    }
}