using System.Windows;
using UNUM.Services;

namespace UNUM
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuarioLogin.Text;
            string password = txtPasswordLogin.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, introduce usuario y contrasena.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int? usuarioId = _authService.Authenticate(usuario, password);

                if (usuarioId.HasValue)
                {
                    MessageBox.Show("Login exitoso. Bienvenido " + usuario, "Acceso Concedido", MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow ventanaPrincipal = new MainWindow(usuarioId.Value);
                    ventanaPrincipal.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contrasena incorrectos.", "Error de Autenticacion", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de base de datos:\n" + ex.Message, "Fallo Critico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
