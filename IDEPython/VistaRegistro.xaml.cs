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
        public VistaRegistro()
        {
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

        }
    }
}
