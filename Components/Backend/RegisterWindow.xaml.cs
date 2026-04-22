using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using UNUM.Services;

namespace UNUM
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService = new();
        private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9._-]{3,20}$");
        private static readonly Regex PasswordRegex = new("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$");

        public RegisterWindow()
        {
            InitializeComponent();
            UpdateRealtimeValidation();
        }

        private void txtUsuarioRegistro_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRealtimeValidation();
        }

        private void txtPasswordRegistro_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateRealtimeValidation();
        }

        private void txtPasswordConfirmacion_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateRealtimeValidation();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            HideMessage();
            UpdateRealtimeValidation();

            string usuario = txtUsuarioRegistro.Text.Trim();
            string password = txtPasswordRegistro.Password;
            string confirmacion = txtPasswordConfirmacion.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Rellena todos los campos.", true);
                return;
            }

            if (!UsernameRegex.IsMatch(usuario))
            {
                ShowMessage("El usuario debe tener entre 3 y 20 caracteres y solo usar letras, números, punto, guion o guion bajo.", true);
                return;
            }

            if (!PasswordRegex.IsMatch(password))
            {
                ShowMessage("La contraseña no cumple la política mínima de seguridad.", true);
                return;
            }

            if (password != confirmacion)
            {
                ShowMessage("La confirmación de contraseña no coincide.", true);
                return;
            }

            try
            {
                int usuarioId = _authService.RegisterUser(usuario, password);

                ShowMessage("Usuario registrado correctamente. Iniciando sesion...", false);

                MainWindow ventanaPrincipal = new MainWindow(usuarioId);
                ventanaPrincipal.Show();
                Close();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    ShowMessage("Ese nombre de usuario ya existe. Elige otro.", true);
                }
                else
                {
                    ShowMessage("Error de base de datos: " + ex.Message, true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error inesperado: " + ex.Message, true);
            }
        }

        private void UpdateRealtimeValidation()
        {
            string usuario = txtUsuarioRegistro.Text.Trim();
            string password = txtPasswordRegistro.Password;
            string confirmacion = txtPasswordConfirmacion.Password;

            bool userValid = UsernameRegex.IsMatch(usuario);
            bool passValid = PasswordRegex.IsMatch(password);
            bool confirmValid = !string.IsNullOrWhiteSpace(confirmacion) && confirmacion == password;

            UpdateFieldState(
                txtUsuarioRegistro,
                txtUsuarioRegistroHint,
                usuario,
                userValid,
                "3-20 caracteres. Permitidos: letras, números, . _ -"
            );

            UpdateFieldState(
                txtPasswordRegistro,
                txtPasswordRegistroHint,
                password,
                passValid,
                "Debe incluir 8+ caracteres, mayúscula, minúscula y número."
            );

            UpdateFieldState(
                txtPasswordConfirmacion,
                txtPasswordConfirmacionHint,
                confirmacion,
                confirmValid,
                "La confirmación no coincide."
            );

            bool canSubmit = userValid && passValid && confirmValid;
            btnRegistrar.IsEnabled = canSubmit;
            btnRegistrar.Opacity = canSubmit ? 1.0 : 0.6;
        }

        private static void UpdateFieldState(Control field, TextBlock hint, string value, bool isValid, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                field.ClearValue(Border.BorderBrushProperty);
                hint.Visibility = Visibility.Collapsed;
                hint.Text = string.Empty;
                return;
            }

            if (isValid)
            {
                field.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5FB879"));
                hint.Visibility = Visibility.Collapsed;
                hint.Text = string.Empty;
                return;
            }

            field.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD9534F"));
            hint.Visibility = Visibility.Visible;
            hint.Text = errorMessage;
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Acción del botón Volver
            InicioWindow inicio = new InicioWindow();
            inicio.Show();
            this.Close();
        }

        private void ShowMessage(string message, bool isError)
        {
            brdMensajeRegistro.Visibility = Visibility.Visible;
            txtMensajeRegistro.Text = message;

            if (isError)
            {
                brdMensajeRegistro.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDECEC"));
                brdMensajeRegistro.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5C2C7"));
                txtMensajeRegistro.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8A1F2A"));
                return;
            }

            brdMensajeRegistro.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAF8EE"));
            brdMensajeRegistro.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBFE6CA"));
            txtMensajeRegistro.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E7E34"));
        }

        private void HideMessage()
        {
            brdMensajeRegistro.Visibility = Visibility.Collapsed;
            txtMensajeRegistro.Text = string.Empty;
        }
    }
}
