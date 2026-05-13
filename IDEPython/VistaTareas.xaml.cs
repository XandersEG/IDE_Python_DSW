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

        public VistaTareas(Modelo.Curso curso) : this()
        {
            if (curso != null)
            {
                // Mostrar nombre del curso en la vista
                var tbCurso = this.FindName("tbCursoNombre") as System.Windows.Controls.TextBlock;
                if (tbCurso != null)
                    tbCurso.Text = $"Curso: {curso.Nombre}";


                // TODO: Cargar enunciados con PHP
                List<Modelo.Enunciado> enunciados = new List<Modelo.Enunciado>();

                enunciados.Add(new Modelo.Enunciado
                {
                    Id = 101,
                    Titulo = "Tareíta 1",
                    Descripcion = "Programas Recursivo: \nCree un programa que implemente una función recursiva para calcular el factorial de un número."
                });

                enunciados.Add(new Modelo.Enunciado
                {
                    Titulo = "Tarea 2",
                    Descripcion = "Serie Fibonacci: \nCree un programa que calcule la serie de Fibonacci hasta un número dado.",
                    Id = 33
                });

                icTareas.ItemsSource = enunciados;
            }
        }
    }
}
