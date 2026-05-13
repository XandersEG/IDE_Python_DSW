using IDEPython.Modelo;
using System;
using System.Collections.Generic;
using System.Text;
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
        public VistaTareas()
        {
            InitializeComponent();
        }

        public VistaTareas(Modelo.Course course) : this()
        {
            if (course != null)
            {
                // Mostrar nombre del curso en la vista
                var tbCurso = this.FindName("tbCursoNombre") as System.Windows.Controls.TextBlock;
                if (tbCurso != null)
                    tbCurso.Text = $"Curso: {course.Name}";


                // TODO: Cargar enunciados con PHP
                List<Assignment> enunciados = new List<Assignment>();

                enunciados.Add(new Assignment
                {
                    Id = 101,
                    Title = "Tarea 1",
                    Description = "Programas Recursivo: \nCree un programa que implemente una función recursiva para calcular el factorial de un número N."
                });

                enunciados.Add(new Assignment
                {
                    Title = "Tareíta 2",
                    Description = "Serie Fibonacci: \nCree un programa que calcule N elementos de la serie de Fibonacci.",
                    Id = 33
                });

                icTareas.ItemsSource = enunciados;
            }
        }
    }
}
