using MySql.Data.MySqlClient;
using System.Windows;
using UNUM.Services;

namespace UNUM
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService = new();

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
                MessageBox.Show("Por favor, rellena todos los campos.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _authService.RegisterUser(usuario, password);

                MessageBox.Show("Usuario registrado correctamente en la base de datos.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);

                InicioWindow inicio = new InicioWindow();
                inicio.Show();
                Close();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Ese nombre de usuario ya existe. Elige otro.", "Error de Integridad", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error de base de datos:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
