using System.Windows;

namespace UNUM
{
    public partial class InicioWindow : Window
    {
        public InicioWindow()
        {
            InitializeComponent();
        }

        private void btnIrLogin_Click(object sender, RoutedEventArgs e)
        {
            // Abre la ventana de Login
            LoginWindow login = new LoginWindow();
            login.Show();
            
            // Cierra la ventana de Inicio
            this.Close();
        }

        private void btnIrRegistro_Click(object sender, RoutedEventArgs e)
        {
            // Nota arquitectónica: MainWindow es la ventana que programamos para el Registro
            RegisterWindow registro = new RegisterWindow();
            registro.Show();
            
            // Cierra la ventana de Inicio
            this.Close();
        }
    }
}