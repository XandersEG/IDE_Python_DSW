using IDEPython.Logica;
using IDEPython.Modelo;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace IDEPython
{

    public class CardUnirse { }
    public partial class VistaCursos : Window
    {
        User user;
        public String userName { get; set; }
        private ApiService api;

        public VistaCursos(User user, ApiService api)
        {
            this.user = user;
            this.api = api;
            userName = user.FirstName+" "+user.LastName1;

            InitializeComponent();
            CargarCursosEstudiante();

            CargarProyectos();

            
            this.DataContext = this;
            
        }

        private async void CargarCursosEstudiante()
        {

            List<Object> courses = new List<Object>
                {
                    new CardUnirse()
                };

            string answer = await api.GetAsync(
                "/listarCursosEstudiante"
            );

            if (answer == null)
            {
                MessageBox.Show("No se obtuvo respuesta de parte del servidor, intente de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            try
            {
                CoursesResponse? coursesResponse =
                    JsonSerializer.Deserialize<CoursesResponse>(answer);

                if (coursesResponse == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor");
                    return;
                }

                if (coursesResponse.exito)
                {
                    foreach (CourseInfo courseInfo in coursesResponse.cursos)
                    {
                        Course course = new Course
                        {
                            Code = courseInfo.Codigo,
                            Name = courseInfo.Nombre,
                            EmailProfessor = courseInfo.CorreoUsuarioProfesor
                        };
                        courses.Add(course);
                    }
            
                }
                else
                {
                    MessageBox.Show("Hubo un error al cargar los cursos");
                    return;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                icCursos.ItemsSource = courses;
            }
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
                    VistaTareas ventanaTareas = new VistaTareas(course, this.user, api);
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
                    // Ensure we operate only inside the per-user projects root and never delete the root folder itself
                    var projectsRoot = Utils.GetUserProjectsRoot(this.user);
                    var projectFullPath = System.IO.Path.GetFullPath(project.Path).TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    var rootFullPath = System.IO.Path.GetFullPath(projectsRoot).TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    if (string.Equals(projectFullPath, rootFullPath, System.StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("No se puede eliminar la carpeta raíz de proyectos del usuario.", "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

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
            IDE ventanaIDE = new IDE(this.user, api);
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
                    IDE ventanaIDE = new IDE(this.user, project.Path, api);
                    ventanaIDE.Show();
                    this.Close();
                }
                else
                {
                    // Create new Project inside the per-user projects root
                    var projectsRoot = Utils.GetUserProjectsRoot(this.user);
                    System.IO.Directory.CreateDirectory(projectsRoot);
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
                    IDE ventanaIDE = new IDE(this.user, newPath, api);
                    ventanaIDE.Show();
                    this.Close();
                }
            }
        }

        private async void btnUnirseCurso_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton == null) return;

            var gridContenedor = boton.Parent as Grid;
            if (gridContenedor != null)
            {
                var txtPassword = gridContenedor.FindName("txtPasswordCurso") as TextBox;

                if (txtPassword != null)
                {
                    string codigoCurso = txtPassword.Text.Trim();

                    if (string.IsNullOrEmpty(codigoCurso))
                    {
                        MessageBox.Show("Por favor, ingrese el código del curso.", "Campo vacío", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtPassword.Focus();
                        return;
                    }

                    var data = new
                    {
                        contrasena = codigoCurso
                    };

                    string response = await api.PostAsync(
                        "/unirseACurso",
                        data
                    );

                    try
                    {
                        JoinResponse? answer =
                            JsonSerializer.Deserialize<JoinResponse>(response);

                        if (answer == null)
                        {
                            MessageBox.Show("Respuesta inválida de parte del servidor");
                            return;
                        }

                        if (answer.exito)
                        {
                            MessageBox.Show("¡Te has unido al curso exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarCursosEstudiante();

                        }
                        else
                        {
                            MessageBox.Show("No se pudo unir al curso: " + answer.mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtPassword.Focus();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ocurrió un error al intentar unirse al curso: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); return;
                    }
                }
            }
        }
        private void CargarProyectos()
        {
            var projectsRoot = Utils.GetUserProjectsRoot(this.user);
            System.IO.Directory.CreateDirectory(projectsRoot);

            // If there are no projects, create a sample one
            var dirs = System.IO.Directory.GetDirectories(projectsRoot);
            if (dirs.Length == 0)
            {
                var samplePath = System.IO.Path.Combine(projectsRoot, "SampleProject");
                System.IO.Directory.CreateDirectory(samplePath);
                System.IO.File.WriteAllText(System.IO.Path.Combine(samplePath, "main.py"), "# New project template\nprint(\"Hello from SampleProject\")");
            }

            List<Project> proyectos = new List<Project>();

            // Card to create new project
            proyectos.Add(new Project { Name = "+ NUEVO", Description = "", Path = string.Empty });
            foreach (var dir in System.IO.Directory.GetDirectories(projectsRoot))
            {
                proyectos.Add(new Project
                {
                    Name = System.IO.Path.GetFileName(dir),
                    Description = "",
                    Path = dir
                });
            }

            icProyectos.ItemsSource = proyectos;
        }
        
    }
}