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

<<<<<<< HEAD
                    MainWindow ventanaPrincipal = new MainWindow(usuarioId.Value);
                    ventanaPrincipal.Show();
                    Close();
=======
                    // Consulta para verificar las credenciales
                    string query = "SELECT Id FROM Usuarios WHERE NombreUsuario = @user AND Contraseña = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", usuario);
                        cmd.Parameters.AddWithValue("@pass", password);

                        object resultado = cmd.ExecuteScalar();

                        if (resultado != null)
                        {
                            int usuarioId = Convert.ToInt32(resultado);
                            MessageBox.Show("�Login exitoso! Bienvenido " + usuario, "Acceso Concedido", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Abrimos la ventana de registro/principal
                            MainWindow ventanaPrincipal = new MainWindow(usuarioId);
                            ventanaPrincipal.Show();

                            // Cerramos esta ventana de Login
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Usuario o contrase�a incorrectos.", "Error de Autenticaci�n", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
>>>>>>> origin/UR03-SimluadorDeAhorro
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
