using IDEPython.DTOs.Http;
using IDEPython.DTOs.Models;
using IDEPython.Services;
using IDEPython.Utils.Decorator;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace IDEPython
{
    /// <summary>
    /// Lógica de interacción para AssignmentsView.xaml
    /// </summary>
    public partial class AssignmentsView : Window
    {
        User? user;
        private ApiService? api;
        Course? course;
        public AssignmentsView()
        {
            InitializeComponent();
        }

        private async void loadAssignments()
        {
            string endpoint = "/listarTareasEstudiante";
            endpoint += $"?nombreCurso={Uri.EscapeDataString(course?.Name ?? "")}";
            endpoint+= $"&correoProfesor={Uri.EscapeDataString(course?.EmailProfessor ?? "")}";

            try
            {

                string answer = await api!.GetAsync(endpoint);

                if (answer == null)
                {
                    MessageBox.Show("No se obtuvo respuesta de parte del servidor, intente de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                AssignmentsResponse? assignmentsResponse =
                    JsonSerializer.Deserialize<AssignmentsResponse>(answer, options);

                if (assignmentsResponse == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor");
                    return;
                }

                List<Assignment> assignments = new();

                if (assignmentsResponse.exito && assignmentsResponse.tareas != null)
                {
                    foreach (AssignmentInfo assignmentInfo in assignmentsResponse.tareas)
                    {
                        if (!string.IsNullOrEmpty(assignmentInfo.idEnunciado))
                        {
                            Assignment assignment = new Assignment
                            {
                                Id = int.Parse(assignmentInfo.idEnunciado),
                                Title = assignmentInfo.Titulo,
                                Description = (assignmentInfo.Descripcion ?? "") + "\n\nFecha límite: " + assignmentInfo.FechaLimite.ToString()
                            };
                            assignments.Add(assignment);
                        }
                    }

                    icTareas.ItemsSource = assignments;
                }
                else
                {
                    MessageBox.Show("Hubo un error al cargar las tareas");
                    MessageBox.Show(answer);
                    return;
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Error de red al intentar cargar las tareas.\nPor favor verifique su conexión a internet e inténtelo de nuevo ", "Error de red", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Excepción");
            }
        }
        public AssignmentsView(Course course, User user, ApiService api) : this()
        {
            this.user = user;
            this.api = api;
            this.course = course;

            if (course != null)
            {
                // Show course name
                var tbCurso = this.FindName("tbCursoNombre") as System.Windows.Controls.TextBlock;
                if (tbCurso != null)
                    tbCurso.Text = $"Curso: {course.Name}";

                
                loadAssignments();
                // TODO: Verify if the user has already submitted them to
                // show the status (pending, submitted, overdue, expired, etc.)
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (user != null && api != null)
            {
                MainView window = new MainView(user, api);
                window.Show();
                this.Close();
            }
        }


        private void BtnResolve_Click(object sender, RoutedEventArgs e)
        {
            if (user == null || api == null) return;

            // Obtener el Assignment desde el DataContext del botón
            var btn = sender as Button;
            var assignment = btn?.DataContext as Assignment;

            if (assignment == null)
            {
                MessageBox.Show("No se pudo obtener la tarea seleccionada.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var projectsRoot = Utils.Utils.GetUserProjectsRoot(this.user);
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

            // Crear template del proyecto
            var templatePath = System.IO.Path.Combine(newPath, "main.py");
            IScript templateScriptBase = new IDEPython.Utils.Decorator.Script(
                "# New project template\nprint(\"Hello New Project\")\n", "main.py");
            ScriptSigned templateScriptSigned = new IDEPython.Utils.Decorator.ScriptSigned(templateScriptBase);
            System.IO.File.WriteAllText(templatePath, templateScriptSigned.GetContent(),
                                        new System.Text.UTF8Encoding(false));
            templateScriptSigned.RegistrarEnCsv("main.py");

            // Pasar el assignment y el course al IDE
            IDE ventanaIDE = new IDE(this.user, newPath, api, assignment, course);
            ventanaIDE.Show();
            this.Close();
        }
    }

}

    
    
