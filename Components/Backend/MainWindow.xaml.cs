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

        private void btnGuardarTransaccion_Click(object sender, RoutedEventArgs e)
        {
            // 1. Extracción de datos de la interfaz (Corregido para evitar nulos)
            string tipo = ((ComboBoxItem)cmbTipo.SelectedItem)?.Content?.ToString() ?? "Gasto";
            string categoria = ((ComboBoxItem)cmbCategoria.SelectedItem)?.Content?.ToString() ?? "Otros";
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

                            // AÑADIR ESTA LÍNEA PARA QUE EL SALDO SE ACTUALICE SIEMPRE QUE CAMBIE LA TABLA
                            CalcularSaldoActual();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al descargar el historial de transacciones:\n" + ex.Message, "Error de Lectura", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CalcularSaldoActual()
        {
            string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Sumamos ingresos y restamos gastos
                    string query = @"SELECT SUM(CASE WHEN Tipo = 'Ingreso' THEN Importe WHEN Tipo = 'Gasto' THEN -Importe ELSE 0 END) AS SaldoTotal 
                                     FROM Transacciones WHERE UsuarioId = @usuarioId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", _usuarioId);
                        object resultado = cmd.ExecuteScalar();

                        if (resultado != DBNull.Value && resultado != null)
                        {
                            decimal saldo = Convert.ToDecimal(resultado);
                            txtSaldoTotal.Text = $"{saldo:0.00} €";

                            // UX: Si estamos en números rojos, el recuadro se vuelve rojo. Si no, verde oscuro.
                            if (saldo < 0)
                                borderSaldo.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60)); // Rojo
                            else
                                borderSaldo.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)); // Verde
                        }
                        else
                        {
                            // Si no hay transacciones, el saldo es 0
                            txtSaldoTotal.Text = "0.00 €";
                            borderSaldo.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Fallo silencioso en consola para no molestar al usuario con pop-ups
                    Console.WriteLine("Error al calcular el saldo: " + ex.Message);
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
            string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // SQL: Traducimos el número de prioridad a texto y calculamos el porcentaje al vuelo
                    string query = @"SELECT Id, Nombre, CosteTotal, AhorroActual, 
                                     CASE WHEN Prioridad = 1 THEN '1-Alta' WHEN Prioridad = 2 THEN '2-Media' ELSE '3-Baja' END AS PrioridadTexto,
                                     (AhorroActual / CosteTotal) * 100 AS Porcentaje
                                     FROM Objetivos WHERE UsuarioId = @uId ORDER BY Prioridad ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uId", _usuarioId);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            adapter.Fill(dt);
                            gridObjetivos.ItemsSource = dt.DefaultView;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error cargando objetivos: " + ex.Message); }
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
            if (gridObjetivos.SelectedItem == null) return;

            System.Data.DataRowView fila = (System.Data.DataRowView)gridObjetivos.SelectedItem;
            int idObjeti = Convert.ToInt32(fila["Id"]);

            if (MessageBox.Show("¿Borrar este objetivo?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string connStr = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("DELETE FROM Objetivos WHERE Id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idObjeti);
                        cmd.ExecuteNonQuery();
                        CargarObjetivos();
                    }
                }
            }
        }
    }
}