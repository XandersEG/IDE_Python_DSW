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
        
        private ApiService api = new ApiService();


        private void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            var datos = new
            {
                correo = txtEmail.Text,
                contrasena = txtPassword.Password
            };
            // Still missing credentials validation logic
            // TODO: Get user info from BackEnd

            Student user = new Student("Xanders", "Makenssy", "Espinoza", "Guzman", "x.espinoza.1@estudiantec.cr");

                    //Student user = new Student("Xanders", "Makenssy", "Espinoza", "Guzman", datos.correo);
                    VistaCursos window = new VistaCursos(user, api);
                    window.Show();
            this.Close();
        }

        private void irARegistro_Click(object sender, MouseButtonEventArgs e)
        {
           VistaRegistro ventanaRegistro = new VistaRegistro(api);
            ventanaRegistro.Show();
            this.Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VistaRegistro ventanaRegistro = new VistaRegistro(api);
            ventanaRegistro.Show();
            this.Close();
        }
    }
}
