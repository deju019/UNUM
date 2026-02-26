using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace UNUM
{
    public partial class LoginWindow : Window
    {
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
                MessageBox.Show("Por favor, introduce usuario y contraseña.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=127.0.0.1; Port=3306; Database=unum; Uid=root; Pwd=admin123;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Consulta para verificar las credenciales
                    string query = "SELECT Id FROM Usuarios WHERE NombreUsuario = @user AND Contrasena = @pass";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", usuario);
                        cmd.Parameters.AddWithValue("@pass", password);

                        object resultado = cmd.ExecuteScalar();

                        if (resultado != null)
                        {
                            int usuarioId = Convert.ToInt32(resultado);
                            MessageBox.Show("¡Login exitoso! Bienvenido " + usuario, "Acceso Concedido", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Abrimos la ventana de registro/principal
                            MainWindow ventanaPrincipal = new MainWindow();
                            ventanaPrincipal.Show();

                            // Cerramos esta ventana de Login
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error de base de datos:\n" + ex.Message, "Fallo Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}