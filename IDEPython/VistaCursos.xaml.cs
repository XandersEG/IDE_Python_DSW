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

        private void EliminarProyecto_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn != null)
            {
                var project = btn.DataContext as Project;
                if (project != null && !string.IsNullOrEmpty(project.Path) && System.IO.Directory.Exists(project.Path))
                {
                    var result = MessageBox.Show($"¿Está seguro de que desea eliminar el proyecto '{project.Name}' y todo su contenido?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            System.IO.Directory.Delete(project.Path, true);
                            CargarProyectos();
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("No se pudo eliminar el proyecto: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void IrAlIDE_Click(object sender, RoutedEventArgs e)
        {
            IDE ventanaIDE = new IDE(this.user);
            ventanaIDE.Show();
            this.Close();
        }

        private void AbrirProyecto_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn != null)
            {
                var project = btn.DataContext as Project;
                if (project != null && !string.IsNullOrEmpty(project.Path))
                {
                    IDE ventanaIDE = new IDE(this.user, project.Path);
                    ventanaIDE.Show();
                    this.Close();
                }
                else
                {
                    // Create new Project
                    var projectsRoot = System.IO.Path.Combine(AppContext.BaseDirectory, "Projects");
                    var baseName = "NewProject_";
                    int i = 1;
                    string newPath;
                    do
                    {
                        newPath = System.IO.Path.Combine(projectsRoot, baseName + i);
                        i++;
                    } while (System.IO.Directory.Exists(newPath));
                    System.IO.Directory.CreateDirectory(newPath);
                    // Create project template
                    var templatePath = System.IO.Path.Combine(newPath, "main.py");
                    System.IO.File.WriteAllText(templatePath, "# New project template\nprint(\"Hello New Project\")\n");
                    IDE ventanaIDE = new IDE(this.user, newPath);
                    ventanaIDE.Show();
                    this.Close();
                }
            }
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