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
    /// Lógica de interacción para VistaRegistro.xaml
    /// </summary>
    public partial class VistaRegistro : Window
    {
        ApiService api;
        public VistaRegistro(ApiService api)
        {
            this.api = api;
            InitializeComponent();
        }

        private void BtnIrALogin_Click(object sender, RoutedEventArgs e)
        {
            VistaLogin ventanaLogin = new VistaLogin();
            ventanaLogin.Show();
            this.Close();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtNombre1.Text))
            {
                MessageBox.Show("El primer nombre es obligatorio.", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApellido1.Text))
            {
                MessageBox.Show("El primer apellido es obligatorio.", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("El correo es obligatorio.", "Validación");
                return;
            }

            if (!txtCorreo.Text.Contains("@") || !txtCorreo.Text.Contains("."))
        {
                MessageBox.Show("El correo no tiene un formato válido.", "Validación");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("La contraseña es obligatoria.", "Validación");
                return;
            }

            if (txtPassword.Password.Length < 8)
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres.", "Validación");
                return;
            }
        }
    }
}
