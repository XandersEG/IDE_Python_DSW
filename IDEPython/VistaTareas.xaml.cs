using IDEPython.Logica;
using IDEPython.Modelo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IDEPython
{
    /// <summary>
    /// Lógica de interacción para VistaTareas.xaml
    /// </summary>
    public partial class VistaTareas : Window
    {
        User user;
        private ApiService api;
        Course course;
        public VistaTareas()
        {
            InitializeComponent();
        }

        private async void loadAssignments()
        {
            string endpoint = "/listarTareasEstudiante";
            endpoint += $"?nombreCurso={Uri.EscapeDataString(course.Name)}";
            endpoint+= $"&correoProfesor={Uri.EscapeDataString(course.EmailProfessor)}";

            string answer = await api.GetAsync(endpoint);

            if (answer == null)
            {
                MessageBox.Show("No se obtuvo respuesta de parte del servidor, intente de nuevo más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                AssignmentsResponse? assignmentsResponse =
                    JsonSerializer.Deserialize<AssignmentsResponse>(answer, options);

                if (assignmentsResponse == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor");
                    return;
                }

                List<Assignment> assignments = new();

                if (assignmentsResponse.exito)
                {
                    foreach (AssignmentInfo assignmentInfo in assignmentsResponse.tareas)
                    {
                        Assignment assignment = new Assignment
                        {
                            Id = int.Parse(assignmentInfo.idEnunciado),
                            Title = assignmentInfo.Titulo,
                            Description = assignmentInfo.Descripcion + "\n\nFecha límite: " + assignmentInfo.FechaLimite.ToString()
                        };
                        assignments.Add(assignment);
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Excepción");
            }
        }
        public VistaTareas(Course course, User user, ApiService api) : this()
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
            VistaCursos window = new VistaCursos(user, api);
            window.Show();
            this.Close();
        }
    }

}

    
    
