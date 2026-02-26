using System.Windows;

namespace UNUM
{
    public partial class MainWindow : Window
    {
        // Variable global privada para recordar quién es el usuario mientras la ventana esté abierta
        private int _usuarioId;

        // Modificamos el constructor para exigir el ID del usuario
        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();
            
            // Guardamos el ID en nuestra variable privada
            _usuarioId = idUsuarioLogueado;

            // Actualizamos la interfaz para demostrar que tenemos el ID
            txtBienvenida.Text = $"Panel de Control (ID de Usuario: {_usuarioId})";
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Cerramos sesión borrando la ventana actual y volviendo al inicio
            InicioWindow inicio = new InicioWindow();
            inicio.Show();
            this.Close();
        }
    }
}