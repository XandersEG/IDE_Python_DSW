using IDEPython.DTOs.Http;
using IDEPython.Services;
using System.Text.Json;
using System.Windows;

namespace IDEPython
{
    /// <summary>
    /// Lógica de interacción para RegisterView.xaml
    /// </summary>
    public partial class RegisterView : Window
    {
        ApiService api;
        public RegisterView(ApiService api)
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

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(txtNombre1.Text))
            {
                MessageBox.Show("El primer nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Information);
                txtNombre1.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApellido1.Text))
            {
                MessageBox.Show("El primer apellido es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Information);
                txtApellido1.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MessageBox.Show("El correo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCorreo.Focus();
                return;
            }

            if (!txtCorreo.Text.Contains("@") || !txtCorreo.Text.Contains("."))
            {
                MessageBox.Show("El correo no tiene un formato válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                txtCorreo.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("La contraseña es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Password.Length < 8)
            {
                MessageBox.Show("La contraseña debe tener al menos 8 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Focus();
                return;
            }

            RegisterRequest request = new RegisterRequest
            {
                primerNombre = txtNombre1.Text,
                segundoNombre = txtNombre2.Text,
                primerApellido = txtApellido1.Text,
                segundoApellido = txtApellido2.Text,
                correo = txtCorreo.Text,
                contrasena = txtPassword.Password,
                confirmarContrasena = txtConfirmPassword.Password
            };

            try
            {
                string respuesta = await api.PostAsync(
                    "/register",
                    request
                );

                RegisterResponse? register =
                    JsonSerializer.Deserialize<RegisterResponse>(respuesta);

                if (register == null)
                {
                    MessageBox.Show("Respuesta inválida");
                    return;
                }

                if (register.exito)
                {
                    var result = MessageBox.Show("Usted se ha registrado exitosamente. ¿Desea iniciar sesión?", "Registro exitoso", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes) return;
                    string email = register.correo ?? string.Empty;
                    if (!string.IsNullOrEmpty(email))
                    {
                        VistaLogin window = new VistaLogin(email);
                        window.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error: No se obtuvo el correo registrado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(register.mensaje);
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Error de red al intentar registrar el usuario.\nPor favor verifique su conexión a internet e inténtelo de nuevo ", "Error de red", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al intentar registrar el usuario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



        }
    }
}
