using MySql.Data.MySqlClient;
using System;
using System.Reflection;
using System.Windows;

namespace UNUM
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, rellene todos los campos", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=127.0.0.1; Port = 3306; Database=unum; Uid=root; Pwd=admin123;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "INSERT INTO Usuarios (NombreUsuario, Contrasena) VALUES (@user, @pass)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", usuario);
                        cmd.Parameters.AddWithValue("@pass", password);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Usuario registrado correctamente en la Base de Datos.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                            txtUsuario.Clear();
                            txtPassword.Clear();
                        }
                    }
                }

                catch (MySqlException ex)
                {
                    if (ex.Number == 1062) // Código de error MySQL para "Duplicate entry"
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
    }
}