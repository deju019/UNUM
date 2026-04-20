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
        private readonly int _usuarioId;
        private readonly TransactionService _transactionService = new();
        private readonly ObjectiveService _objectiveService = new();

        // Modificamos el constructor para exigir el ID del usuario
        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();

            // Guardamos el ID en nuestra variable privada
            _usuarioId = idUsuarioLogueado;

            // Cargamos el historial de transacciones del usuario al abrir la ventana
            CargarHistorial();

            // Cargamos los objetivos del usuario
            CargarObjetivos();
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
                CalcularSaldoActual(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar el historial de transacciones:\n" + ex.Message, "Error de Lectura", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalcularSaldoActual(DataTable transacciones)
        {
            decimal saldo = 0m;

            foreach (DataRow row in transacciones.Rows)
            {
                if (row["Importe"] == DBNull.Value || row["Tipo"] == DBNull.Value)
                {
                    continue;
                }

                var tipo = row["Tipo"].ToString();
                var importe = Convert.ToDecimal(row["Importe"]);

                if (string.Equals(tipo, "Ingreso", StringComparison.OrdinalIgnoreCase))
                {
                    saldo += importe;
                }
                else if (string.Equals(tipo, "Gasto", StringComparison.OrdinalIgnoreCase))
                {
                    saldo -= importe;
                }
            }

            txtSaldoTotal.Text = $"{saldo:0.00} €";
            borderSaldo.Background = saldo < 0
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
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


        // ==========================================
        // SISTEMA DE NAVEGACIÓN (MENÚ LATERAL)
        // ==========================================
        private void btnMenuTransacciones_Click(object sender, RoutedEventArgs e)
        {
            // Mostramos Transacciones, Ocultamos Simulador
            panelTransacciones.Visibility = Visibility.Visible;
            panelSimulador.Visibility = Visibility.Collapsed;

            // Feedback visual en el menú (Pintamos el botón activo)
            btnMenuTransacciones.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94)); // #34495E
            btnMenuSimulador.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void btnMenuSimulador_Click(object sender, RoutedEventArgs e)
        {
            // Mostramos Simulador, Ocultamos Transacciones
            panelSimulador.Visibility = Visibility.Visible;
            panelTransacciones.Visibility = Visibility.Collapsed;

            // Feedback visual en el menú
            btnMenuSimulador.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94)); // #34495E
            btnMenuTransacciones.Background = System.Windows.Media.Brushes.Transparent;
        }


        // ==========================================
        // SISTEMA MULTI-OBJETIVO (CRUD y MODALES)
        // ==========================================
        private void CargarObjetivos()
        {
            try
            {
                var dt = _objectiveService.GetObjectives(_usuarioId);
                gridObjetivos.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando objetivos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNuevoObjetivo_Click(object sender, RoutedEventArgs e)
        {
            // Instanciamos la ventana modal pasándole quién es el usuario
            ObjetivoModalWindow modal = new ObjetivoModalWindow(_usuarioId);

            // Le decimos a C# que el 'dueño' del modal es la ventana principal, para que se bloquee el fondo
            modal.Owner = this;

            // ShowDialog detiene la ejecución hasta que se cierra la ventanita
            if (modal.ShowDialog() == true)
            {
                // Si devuelve true, es que se guardó bien. Refrescamos la tabla.
                CargarObjetivos();
            }
        }

        private void btnBorrarObjetivo_Click(object sender, RoutedEventArgs e)
        {
            if (gridObjetivos.SelectedItem is not DataRowView fila)
            {
                MessageBox.Show("Selecciona un objetivo para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idObjeti = Convert.ToInt32(fila["Id"]);
            if (MessageBox.Show("¿Borrar este objetivo?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                bool deleted = _objectiveService.DeleteObjective(idObjeti, _usuarioId);
                if (deleted)
                {
                    CargarObjetivos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar objetivo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
