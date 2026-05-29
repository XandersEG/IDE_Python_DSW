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
            txtEmail.Focus();

        }
        
        public VistaLogin(String correo)
        {
            InitializeComponent();
            txtEmail.Text = correo;
            txtPassword.Focus();

        }

        private ApiService api = new ApiService();

        private async void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtEmail.Text) && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                txtEmail.Text = "x@.";
                txtPassword.Password = ".";
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Debe de ingresar un correo para iniciar sesión.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Debe de ingresar una contraseña para iniciar sesión.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus(); 
                return;
            }

            var datos = new
            {
                correo = txtEmail.Text,
                contrasena = txtPassword.Password
            };

            string respuesta = await api.PostAsync(
                "/login",
                datos
            );

            try
            {
                LoginResponse? login =
                    JsonSerializer.Deserialize<LoginResponse>(respuesta);

                if (login == null)
                {
                    MessageBox.Show("Respuesta inválida de parte del servidor");
                    return;
                }

                if (login.exito)
                {
                    api.Token = login.token;

                    Student user = new Student(
                        login.datos.nombre, 
                        "",
                        login.datos.apellido,
                        "",
                        login.datos.correo
                    );

                    VistaCursos window = new VistaCursos(user, api);
                    window.Show();
            this.Close();
        }
                else
                {
                    MessageBox.Show("Las credenciales que ha ingresado no son correctas. Verifique que el correo y contraseña ingresados sean correctos","Credenciales inválidas", MessageBoxButton.OK, MessageBoxImage.Error);
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
