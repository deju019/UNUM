using MySql.Data.MySqlClient;
using System;
using System.Windows;

namespace UNUM
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuarioRegistro.Text;
            string password = txtPasswordRegistro.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, rellena todos los campos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=127.0.0.1; Port=3306; Database=unum; Uid=root; Pwd=admin123;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Mantenemos tu consulta original con "Contraseña" (asegúrate de que en la BD se llame igual)
                    string query = "INSERT INTO Usuarios (NombreUsuario, Contrasena) VALUES (@user, @pass)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", usuario);
                        cmd.Parameters.AddWithValue("@pass", password);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Usuario registrado correctamente en la Base de Datos.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Redirigimos al inicio para que pueda hacer Login con su nueva cuenta
                            InicioWindow inicio = new InicioWindow();
                            inicio.Show();
                            this.Close();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) // Error MySQL para "Duplicate entry"
                    {
                        MessageBox.Show("Ese nombre de usuario ya existe. Elige otro.", "Error de Integridad", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Error de base de datos:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            // Acción del botón Volver
            InicioWindow inicio = new InicioWindow();
            inicio.Show();
            this.Close();
        }
    }
}