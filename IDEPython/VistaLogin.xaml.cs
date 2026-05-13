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
            //Logica Validacion
            VistaCursos cursos = new VistaCursos();
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
