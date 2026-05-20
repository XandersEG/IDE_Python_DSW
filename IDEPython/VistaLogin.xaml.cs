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
    /// Lógica de interacción para VistaLogin.xaml
    /// </summary>
    public partial class VistaLogin : Window
    {
        public VistaLogin()
        {
            InitializeComponent();

        }
        
        private ApiService api = new ApiService();

        private async void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {

            var datos = new
            {
                correo = txtEmail.Text,
                contrasena = txtPassword.Password
            };

            string respuesta = await api.PostAsync(
                "/login",
                datos
            );

            MessageBox.Show(respuesta);

            try
            {
                LoginResponse? login =
                    JsonSerializer.Deserialize<LoginResponse>(respuesta);

                if (login == null)
                {
                    MessageBox.Show("Respuesta inválida");
                    return;
                }

                else { MessageBox.Show("no nulo"); }

                if (login.exito)
                {
                    api.Token = login.token;

                    Student user = new Student(
                        login.usuario.nombre,
                        "",
                        "",
                        "",
                        login.usuario.correo
                    );

                    //Student user = new Student("Xanders", "Makenssy", "Espinoza", "Guzman", datos.correo);
                    VistaCursos window = new VistaCursos(user, api);
                    window.Show();
            this.Close();
        }
                else
                {
                    MessageBox.Show("Credenciales inválidas");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
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
