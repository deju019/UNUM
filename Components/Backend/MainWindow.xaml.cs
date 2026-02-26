using System.Windows;
using System;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Data;

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

        private void btnGuardarTransaccion_Click(object sender, RoutedEventArgs e)
        {
            // 1. Extracción de datos de la interfaz
            string tipo = ((ComboBoxItem)cmbTipo.SelectedItem).Content.ToString();
            string categoria = ((ComboBoxItem)cmbCategoria.SelectedItem).Content.ToString();
            string descripcion = txtDescripcion.Text;

            // 2. Control de Calidad (QA): Validar que el importe sea un número válido
            // Nota: Dependiendo del idioma de tu PC, los decimales se separan con '.' o ','
            if (!decimal.TryParse(txtImporte.Text, out decimal importe) || importe <= 0)
            {
                MessageBox.Show("Por favor, introduce un importe numérico válido mayor que cero (ej. 50,50 o 50.50).", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 3. Consulta SQL parametrizada. Pasamos el _usuarioId de la sesión activa
                    string query = "INSERT INTO Transacciones (UsuarioId, Tipo, Categoria, Importe, FechaTransaccion, Descripcion) " +
                                   "VALUES (@usuarioId, @tipo, @categoria, @importe, @fecha, @descripcion)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", _usuarioId);
                        cmd.Parameters.AddWithValue("@tipo", tipo);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@importe", importe);
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now.Date); // Guardamos la fecha de hoy
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Transacción registrada con éxito.", "Operación Completada", MessageBoxButton.OK, MessageBoxImage.Information);

                            // 4. Limpiamos el formulario para el siguiente uso (Buena práctica de UX)
                            txtImporte.Clear();
                            txtDescripcion.Clear();

                            CargarHistorial();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar la transacción:\n" + ex.Message, "Fallo Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CargarHistorial()
        {
            string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Añadimos 'Id' a la consulta SQL
                    string query = "SELECT Id, Tipo, Categoria, Importe, FechaTransaccion AS 'Fecha', Descripcion " +
                                   "FROM Transacciones WHERE UsuarioId = @usuarioId ORDER BY FechaTransaccion DESC, Id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", _usuarioId);

                        // El DataAdapter es un puente que ejecuta la consulta y rellena un objeto DataTable automáticamente
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Data Binding: Le decimos al DataGrid visual que su origen de datos es nuestra tabla en memoria
                            gridTransacciones.ItemsSource = dt.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al descargar el historial de transacciones:\n" + ex.Message, "Error de Lectura", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();

                        // 4. Sentencia DELETE segura
                        string query = "DELETE FROM Transacciones WHERE Id = @idTransaccion AND UsuarioId = @usuarioId";

                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@idTransaccion", idTransaccion);
                            cmd.Parameters.AddWithValue("@usuarioId", _usuarioId);

                            int filasAfectadas = cmd.ExecuteNonQuery();

                            if (filasAfectadas > 0)
                            {
                                // 5. Refrescamos la tabla para que el elemento desaparezca visualmente
                                CargarHistorial();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al intentar eliminar la transacción:\n" + ex.Message, "Fallo Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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