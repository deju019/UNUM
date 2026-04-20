using System.Windows;
using System;
using System.Windows.Controls;
using System.Data;
using UNUM.Services;

namespace UNUM
{
    public partial class MainWindow : Window
    {
        // Variable global privada para recordar quién es el usuario mientras la ventana esté abierta
        private int _usuarioId;
        private readonly TransactionService _transactionService = new();

        // Modificamos el constructor para exigir el ID del usuario
        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();

            // Guardamos el ID en nuestra variable privada
            _usuarioId = idUsuarioLogueado;

            // Actualizamos la interfaz para demostrar que tenemos el ID
            txtBienvenida.Text = $"Panel de Control (ID de Usuario: {_usuarioId})";

            // Cargamos el historial de transacciones del usuario al abrir la ventana
            CargarHistorial();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Cerramos sesión borrando la ventana actual y volviendo al inicio
            InicioWindow inicio = new InicioWindow();
            inicio.Show();
            this.Close();
        }

        private void btnObjetivos_Click(object sender, RoutedEventArgs e)
        {
            var objetivosWindow = new ObjetivosWindow(_usuarioId)
            {
                Owner = this
            };

            objetivosWindow.ShowDialog();
        }

        private void btnGuardarTransaccion_Click(object sender, RoutedEventArgs e)
        {
            // 1. Extracción de datos de la interfaz
            var tipoItem = cmbTipo.SelectedItem as ComboBoxItem;
            var categoriaItem = cmbCategoria.SelectedItem as ComboBoxItem;
            string tipo = tipoItem?.Content?.ToString() ?? string.Empty;
            string categoria = categoriaItem?.Content?.ToString() ?? string.Empty;
            string descripcion = txtDescripcion.Text;

            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(categoria))
            {
                MessageBox.Show("Selecciona tipo y categoria para continuar.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Control de Calidad (QA): Validar que el importe sea un número válido
            // Nota: Dependiendo del idioma de tu PC, los decimales se separan con '.' o ','
            if (!decimal.TryParse(txtImporte.Text, out decimal importe) || importe <= 0)
            {
                MessageBox.Show("Por favor, introduce un importe numérico válido mayor que cero (ej. 50,50 o 50.50).", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _transactionService.AddTransaction(_usuarioId, tipo, categoria, importe, DateTime.Now.Date, descripcion);

                MessageBox.Show("Transaccion registrada con exito.", "Operacion Completada", MessageBoxButton.OK, MessageBoxImage.Information);

                txtImporte.Clear();
                txtDescripcion.Clear();
                CargarHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la transaccion:\n" + ex.Message, "Fallo Critico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarHistorial()
        {
            try
            {
                DataTable dt = _transactionService.GetUserTransactions(_usuarioId);
                gridTransacciones.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar el historial de transacciones:\n" + ex.Message, "Error de Lectura", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBorrarTransaccion_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verificar si hay alguna fila seleccionada en el DataGrid
            if (gridTransacciones.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona una transacción de la tabla para eliminarla.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Extraer el Id de la fila seleccionada (que ahora es un DataRowView)
            DataRowView filaSeleccionada = (DataRowView)gridTransacciones.SelectedItem;
            int idTransaccion = Convert.ToInt32(filaSeleccionada["Id"]);

            // 3. QA: Confirmación de seguridad
            MessageBoxResult confirmacion = MessageBox.Show("¿Estás seguro de que deseas eliminar permanentemente esta transacción?", "Confirmar Borrado", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                try
                {
                    bool deleted = _transactionService.DeleteTransaction(idTransaccion, _usuarioId);

                    if (deleted)
                    {
                        CargarHistorial();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al intentar eliminar la transaccion:\n" + ex.Message, "Fallo Critico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            // Llamamos a la función que ya construimos para descargar los datos de MySQL
            CargarHistorial();
        }
    }
}
