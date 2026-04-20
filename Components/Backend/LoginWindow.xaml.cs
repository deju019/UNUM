using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using UNUM.Services;

namespace UNUM
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new();
        private static readonly Regex UsernameRegex = new("^[a-zA-Z0-9._-]{3,20}$");

        public LoginWindow()
        {
            InitializeComponent();
            UpdateRealtimeValidation();
        }

        private void txtUsuarioLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRealtimeValidation();
        }

        private void txtPasswordLogin_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateRealtimeValidation();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            HideMessage();
            UpdateRealtimeValidation();

            string usuario = txtUsuarioLogin.Text.Trim();
            string password = txtPasswordLogin.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Introduce usuario y contraseña.", true);
                return;
            }

            if (!UsernameRegex.IsMatch(usuario))
            {
                ShowMessage("El usuario debe tener entre 3 y 20 caracteres y solo usar letras, números, punto, guion o guion bajo.", true);
                return;
            }

            if (password.Length < 6)
            {
                ShowMessage("La contraseña debe tener al menos 6 caracteres.", true);
                return;
            }

            try
            {
                int? usuarioId = _authService.Authenticate(usuario, password);

                if (usuarioId.HasValue)
                {
                    ShowMessage($"Acceso concedido. Bienvenido, {usuario}.", false);

                    try
                    {
                        MainWindow ventanaPrincipal = new MainWindow(usuarioId.Value);
                        ventanaPrincipal.Show();
                        Close();
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("No se pudo abrir el panel principal: " + ex.Message, true);
                    }
                }
                else
                {
                    ShowMessage("Usuario o contraseña incorrectos.", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error de base de datos: " + ex.Message, true);
            }
        }

        private void UpdateRealtimeValidation()
        {
            string usuario = txtUsuarioLogin.Text.Trim();
            string password = txtPasswordLogin.Password;

            bool userValid = UsernameRegex.IsMatch(usuario);
            bool passValid = password.Length >= 6;

            UpdateFieldState(
                txtUsuarioLogin,
                txtUsuarioLoginHint,
                usuario,
                userValid,
                "3-20 caracteres. Permitidos: letras, números, . _ -"
            );

            UpdateFieldState(
                txtPasswordLogin,
                txtPasswordLoginHint,
                password,
                passValid,
                "Mínimo 6 caracteres."
            );

            bool canSubmit = userValid && passValid;
            btnLogin.IsEnabled = canSubmit;
            btnLogin.Opacity = canSubmit ? 1.0 : 0.6;
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

        private void ShowMessage(string message, bool isError)
        {
            brdMensajeLogin.Visibility = Visibility.Visible;
            txtMensajeLogin.Text = message;

            if (isError)
            {
                brdMensajeLogin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDECEC"));
                brdMensajeLogin.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5C2C7"));
                txtMensajeLogin.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8A1F2A"));
                return;
            }

            brdMensajeLogin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAF8EE"));
            brdMensajeLogin.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBFE6CA"));
            txtMensajeLogin.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E7E34"));
        }

        private void HideMessage()
        {
            brdMensajeLogin.Visibility = Visibility.Collapsed;
            txtMensajeLogin.Text = string.Empty;
        }

        private void btnVolverLogin_Click(object sender, RoutedEventArgs e)
        {
            InicioWindow inicio = new InicioWindow();
            inicio.Show();
            Close();
        }
    }
}
