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
    /// Lógica de interacción para VistaLogin.xaml
    /// </summary>
    public partial class VistaLogin : Window
    {
        public VistaLogin()
        {
            InitializeComponent();
        }
        


        private void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Still missing credentials validation logic
            // TODO: Get user info from BackEnd
            Estudiante user = new Estudiante("Xanders", "Makenssy", "Espinoza", "Guzman", "x.espinoza.1@estudiantec.cr");

            VistaCursos cursos = new VistaCursos(user);
            cursos.Show();
            this.Close();
        }

        private void irARegistro_Click(object sender, MouseButtonEventArgs e)
        {
           VistaRegistro ventanaRegistro = new VistaRegistro();
            ventanaRegistro.Show();
            this.Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VistaRegistro ventanaRegistro = new VistaRegistro();
            ventanaRegistro.Show();
            this.Close();
        }
    }
}
