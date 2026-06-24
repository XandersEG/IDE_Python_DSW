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
    /// Lógica de interacción para AddMembersDialog.xaml
    /// </summary>
    public partial class AddMembersDialog : Window
    {
        public string EmailIngresado { get; private set; }
        public AddMembersDialog()
        {
            InitializeComponent();
            txtEmail.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Por favor, ingresa un correo electrónico.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Por favor, ingresa un correo electrónico válido.", "Formato incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EmailIngresado = email;


            //Validacion del correo en la base y añadir usuario al grupo de trabajo
            this.DialogResult = true;
            this.Close();
        }
        private void btnDontSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
