using System.Windows;
using System;
using System.Windows.Controls;
using System.Data;
using UNUM.Services;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Globalization;
using Microsoft.Win32;

namespace UNUM
{
    public partial class MainWindow : Window
    {
        // Variable global privada para recordar quién es el usuario mientras la ventana esté abierta
        private readonly int _usuarioId;
        private readonly TransactionService _transactionService = new();
        private readonly ObjectiveService _objectiveService = new();
        private DataTable _transacciones = new();
        private bool _restaurandoEstadoFiltros;
        private static readonly string[] CategoriasGasto = { "Ocio", "Supermercado", "Facturas", "Otros" };
        private static readonly string[] CategoriasIngreso = { "Nómina", "Otros" };

        // Modificamos el constructor para exigir el ID del usuario
        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();

            // Guardamos el ID en nuestra variable privada
            _usuarioId = idUsuarioLogueado;

            ActualizarCategoriasPorTipo();
            ActualizarCategoriasFiltroPorTipo();
            RestaurarEstadoFiltros();

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

            if (!EsCategoriaValidaParaTipo(tipo, categoria))
            {
                MessageBox.Show("La categoria seleccionada no corresponde con el tipo de transaccion.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                ActualizarCategoriasPorTipo();
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
                _transacciones = _transactionService.GetUserTransactions(_usuarioId);
                AplicarFiltrosTransacciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar el historial de transacciones:\n" + ex.Message, "Error de Lectura", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltrosTransacciones()
        {
            if (_transacciones.Rows.Count == 0)
            {
                gridTransacciones.ItemsSource = _transacciones.DefaultView;
                CalcularResumenFinanciero(_transacciones);
                ActualizarIndicadoresFiltrosActivos();
                return;
            }

            if (dpFechaDesde.SelectedDate.HasValue && dpFechaHasta.SelectedDate.HasValue && dpFechaDesde.SelectedDate > dpFechaHasta.SelectedDate)
            {
                MessageBox.Show("La fecha desde no puede ser mayor que la fecha hasta.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filtros = new List<string>();

            var tipoSeleccionado = (cmbFiltroTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            if (!string.Equals(tipoSeleccionado, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                var tipoEscapado = tipoSeleccionado.Replace("'", "''");
                filtros.Add($"Tipo = '{tipoEscapado}'");
            }

            var periodoSeleccionado = (cmbFiltroPeriodo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Historico";
            if (string.Equals(periodoSeleccionado, "Mes actual", StringComparison.OrdinalIgnoreCase))
            {
                var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);
                filtros.Add($"Fecha >= #{inicioMes:MM/dd/yyyy}#");
                filtros.Add($"Fecha <= #{finMes:MM/dd/yyyy}#");
            }

            var categoriaSeleccionada = (cmbFiltroCategoria.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";
            if (!string.Equals(categoriaSeleccionada, "Todas", StringComparison.OrdinalIgnoreCase))
            {
                var categoriaEscapada = categoriaSeleccionada.Replace("'", "''");
                filtros.Add($"Categoria = '{categoriaEscapada}'");
            }

            var textoLibre = (txtFiltroTexto.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(textoLibre))
            {
                var textoEscapado = EscapeRowFilterLikeValue(textoLibre.Replace("'", "''"));
                filtros.Add($"(ISNULL(Descripcion, '') LIKE '%{textoEscapado}%' OR Categoria LIKE '%{textoEscapado}%' OR Tipo LIKE '%{textoEscapado}%')");
            }

            decimal? importeMin = null;
            if (!string.IsNullOrWhiteSpace(txtImporteMin.Text))
            {
                if (!TryParseImporteFiltro(txtImporteMin.Text, out var valorMin))
                {
                    MessageBox.Show("El importe minimo no es valido.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                importeMin = valorMin;
                filtros.Add($"Importe >= {valorMin.ToString(CultureInfo.InvariantCulture)}");
            }

            decimal? importeMax = null;
            if (!string.IsNullOrWhiteSpace(txtImporteMax.Text))
            {
                if (!TryParseImporteFiltro(txtImporteMax.Text, out var valorMax))
                {
                    MessageBox.Show("El importe maximo no es valido.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                importeMax = valorMax;
                filtros.Add($"Importe <= {valorMax.ToString(CultureInfo.InvariantCulture)}");
            }

            if (importeMin.HasValue && importeMax.HasValue && importeMin.Value > importeMax.Value)
            {
                MessageBox.Show("El importe minimo no puede ser mayor que el importe maximo.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpFechaDesde.SelectedDate.HasValue)
            {
                filtros.Add($"Fecha >= #{dpFechaDesde.SelectedDate.Value:MM/dd/yyyy}#");
            }

            if (dpFechaHasta.SelectedDate.HasValue)
            {
                filtros.Add($"Fecha <= #{dpFechaHasta.SelectedDate.Value:MM/dd/yyyy}#");
            }

            var vista = _transacciones.DefaultView;
            vista.RowFilter = string.Join(" AND ", filtros);

            gridTransacciones.ItemsSource = vista;
            CalcularResumenFinanciero(vista.ToTable());
            ActualizarIndicadoresFiltrosActivos();

            if (!_restaurandoEstadoFiltros)
            {
                GuardarEstadoFiltros();
            }
        }

        private void CalcularResumenFinanciero(DataTable transacciones)
        {
            decimal totalIngresos = 0m;
            decimal totalGastos = 0m;
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
                    totalIngresos += importe;
                    saldo += importe;
                }
                else if (string.Equals(tipo, "Gasto", StringComparison.OrdinalIgnoreCase))
                {
                    totalGastos += importe;
                    saldo -= importe;
                }
            }

            txtTotalIngresos.Text = $"{totalIngresos:0.00} €";
            txtTotalGastos.Text = $"{totalGastos:0.00} €";
            txtBalanceNeto.Text = $"{saldo:0.00} €";
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

        private void cmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarCategoriasPorTipo();
        }

        private void btnAplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltrosTransacciones();
        }

        private void cmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarCategoriasFiltroPorTipo();
        }

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroTipo.SelectedIndex = 0;
            cmbFiltroPeriodo.SelectedIndex = 0;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;
            txtFiltroTexto.Clear();
            txtImporteMin.Clear();
            txtImporteMax.Clear();
            ActualizarCategoriasFiltroPorTipo();
            AplicarFiltrosTransacciones();
        }

        private void btnExportarCsv_Click(object sender, RoutedEventArgs e)
        {
            if (gridTransacciones.ItemsSource is not DataView vista || vista.Count == 0)
            {
                MessageBox.Show("No hay transacciones para exportar.", "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"transacciones_usuario_{_usuarioId}_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                if (vista.Table is null)
                {
                    MessageBox.Show("No se pudo acceder a los datos para exportar.", "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var writer = new StreamWriter(dialog.FileName, false);
                var columnas = vista.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
                writer.WriteLine(string.Join(",", columnas.Select(EscapeCsvValue)));

                foreach (DataRowView fila in vista)
                {
                    var valores = columnas.Select(col => fila.Row[col]?.ToString() ?? string.Empty);
                    writer.WriteLine(string.Join(",", valores.Select(EscapeCsvValue)));
                }

                MessageBox.Show("CSV exportado correctamente.", "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar CSV:\n" + ex.Message, "Exportar CSV", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarCategoriasPorTipo()
        {
            if (cmbTipo is null || cmbCategoria is null)
            {
                return;
            }

            var tipo = (cmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Gasto";
            var categorias = string.Equals(tipo, "Ingreso", StringComparison.OrdinalIgnoreCase)
                ? CategoriasIngreso
                : CategoriasGasto;

            cmbCategoria.Items.Clear();
            foreach (var categoria in categorias)
            {
                cmbCategoria.Items.Add(new ComboBoxItem { Content = categoria });
            }

            cmbCategoria.SelectedIndex = 0;
        }

        private static bool EsCategoriaValidaParaTipo(string tipo, string categoria)
        {
            if (string.Equals(tipo, "Ingreso", StringComparison.OrdinalIgnoreCase))
            {
                return CategoriasIngreso.Contains(categoria, StringComparer.OrdinalIgnoreCase);
            }

            return CategoriasGasto.Contains(categoria, StringComparer.OrdinalIgnoreCase);
        }

        private void ActualizarCategoriasFiltroPorTipo()
        {
            if (cmbFiltroTipo is null || cmbFiltroCategoria is null)
            {
                return;
            }

            var categoriaActual = (cmbFiltroCategoria.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var tipoFiltro = (cmbFiltroTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";

            IEnumerable<string> categorias = tipoFiltro switch
            {
                "Ingreso" => CategoriasIngreso,
                "Gasto" => CategoriasGasto,
                _ => CategoriasGasto.Concat(CategoriasIngreso).Distinct(StringComparer.OrdinalIgnoreCase)
            };

            cmbFiltroCategoria.Items.Clear();
            cmbFiltroCategoria.Items.Add(new ComboBoxItem { Content = "Todas" });

            foreach (var categoria in categorias)
            {
                cmbFiltroCategoria.Items.Add(new ComboBoxItem { Content = categoria });
            }

            var indexSeleccionado = 0;
            if (!string.IsNullOrWhiteSpace(categoriaActual))
            {
                for (var i = 0; i < cmbFiltroCategoria.Items.Count; i++)
                {
                    if (cmbFiltroCategoria.Items[i] is ComboBoxItem item
                        && string.Equals(item.Content?.ToString(), categoriaActual, StringComparison.OrdinalIgnoreCase))
                    {
                        indexSeleccionado = i;
                        break;
                    }
                }
            }

            cmbFiltroCategoria.SelectedIndex = indexSeleccionado;
        }

        private void ActualizarIndicadoresFiltrosActivos()
        {
            if (panelFiltrosActivos is null || txtResumenResultados is null || cmbFiltroTipo is null || cmbFiltroPeriodo is null || cmbFiltroCategoria is null)
            {
                return;
            }

            panelFiltrosActivos.Children.Clear();

            var chips = new List<string>();
            var tipo = (cmbFiltroTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            var periodo = (cmbFiltroPeriodo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Historico";
            var categoria = (cmbFiltroCategoria.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas";

            if (!string.Equals(tipo, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add($"Tipo: {tipo}");
            }

            if (!string.Equals(periodo, "Historico", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add($"Periodo: {periodo}");
            }

            if (!string.Equals(categoria, "Todas", StringComparison.OrdinalIgnoreCase))
            {
                chips.Add($"Categoria: {categoria}");
            }

            var textoBusqueda = (txtFiltroTexto.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(textoBusqueda))
            {
                chips.Add($"Texto: {textoBusqueda}");
            }

            if (!string.IsNullOrWhiteSpace(txtImporteMin.Text))
            {
                chips.Add($"Min: {txtImporteMin.Text} €");
            }

            if (!string.IsNullOrWhiteSpace(txtImporteMax.Text))
            {
                chips.Add($"Max: {txtImporteMax.Text} €");
            }

            if (dpFechaDesde.SelectedDate.HasValue)
            {
                chips.Add($"Desde: {dpFechaDesde.SelectedDate.Value:dd/MM/yyyy}");
            }

            if (dpFechaHasta.SelectedDate.HasValue)
            {
                chips.Add($"Hasta: {dpFechaHasta.SelectedDate.Value:dd/MM/yyyy}");
            }

            if (chips.Count == 0)
            {
                chips.Add("Sin filtros (historico completo)");
            }

            foreach (var chipTexto in chips)
            {
                panelFiltrosActivos.Children.Add(CrearChip(chipTexto));
            }

            var total = _transacciones.Rows.Count;
            var visibles = (gridTransacciones.ItemsSource as DataView)?.Count ?? total;
            txtResumenResultados.Text = $"{visibles} de {total} transacciones";
        }

        private static Border CrearChip(string texto)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(232, 240, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(189, 214, 251)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 0, 6, 6),
                Child = new TextBlock
                {
                    Text = texto,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(34, 73, 135))
                }
            };
        }

        private void GuardarEstadoFiltros()
        {
            try
            {
                var ruta = ObtenerRutaEstadoFiltros();
                var mapa = LeerEstadoFiltros(ruta);

                mapa[_usuarioId] = new EstadoFiltros
                {
                    Tipo = (cmbFiltroTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos",
                    Periodo = (cmbFiltroPeriodo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Historico",
                    Categoria = (cmbFiltroCategoria.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todas",
                    FechaDesde = dpFechaDesde.SelectedDate,
                    FechaHasta = dpFechaHasta.SelectedDate,
                    TextoLibre = txtFiltroTexto.Text,
                    ImporteMin = txtImporteMin.Text,
                    ImporteMax = txtImporteMax.Text
                };

                var json = JsonSerializer.Serialize(mapa, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ruta, json);
            }
            catch
            {
                // Fallo silencioso: no debe romper el uso normal de la app.
            }
        }

        private void RestaurarEstadoFiltros()
        {
            try
            {
                if (cmbFiltroTipo is null || cmbFiltroPeriodo is null || cmbFiltroCategoria is null)
                {
                    return;
                }

                var ruta = ObtenerRutaEstadoFiltros();
                var mapa = LeerEstadoFiltros(ruta);
                if (!mapa.TryGetValue(_usuarioId, out var estado))
                {
                    return;
                }

                _restaurandoEstadoFiltros = true;

                SeleccionarComboPorTexto(cmbFiltroTipo, estado.Tipo, 0);
                SeleccionarComboPorTexto(cmbFiltroPeriodo, estado.Periodo, 0);
                ActualizarCategoriasFiltroPorTipo();
                SeleccionarComboPorTexto(cmbFiltroCategoria, estado.Categoria, 0);

                dpFechaDesde.SelectedDate = estado.FechaDesde;
                dpFechaHasta.SelectedDate = estado.FechaHasta;
                txtFiltroTexto.Text = estado.TextoLibre ?? string.Empty;
                txtImporteMin.Text = estado.ImporteMin ?? string.Empty;
                txtImporteMax.Text = estado.ImporteMax ?? string.Empty;
            }
            catch
            {
                // Fallo silencioso: si no hay estado persistido, usamos defaults.
            }
            finally
            {
                _restaurandoEstadoFiltros = false;
            }
        }

        private static string ObtenerRutaEstadoFiltros()
        {
            var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UNUM");
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "filtros-mainwindow.json");
        }

        private static Dictionary<int, EstadoFiltros> LeerEstadoFiltros(string ruta)
        {
            if (!File.Exists(ruta))
            {
                return new Dictionary<int, EstadoFiltros>();
            }

            var json = File.ReadAllText(ruta);
            return JsonSerializer.Deserialize<Dictionary<int, EstadoFiltros>>(json) ?? new Dictionary<int, EstadoFiltros>();
        }

        private static void SeleccionarComboPorTexto(ComboBox combo, string? texto, int indicePorDefecto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                combo.SelectedIndex = indicePorDefecto;
                return;
            }

            for (var i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is ComboBoxItem item
                    && string.Equals(item.Content?.ToString(), texto, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            combo.SelectedIndex = indicePorDefecto;
        }

        private sealed class EstadoFiltros
        {
            public string Tipo { get; set; } = "Todos";
            public string Periodo { get; set; } = "Historico";
            public string Categoria { get; set; } = "Todas";
            public DateTime? FechaDesde { get; set; }
            public DateTime? FechaHasta { get; set; }
            public string? TextoLibre { get; set; }
            public string? ImporteMin { get; set; }
            public string? ImporteMax { get; set; }
        }

        private static bool TryParseImporteFiltro(string texto, out decimal valor)
        {
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out valor))
            {
                return true;
            }

            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        private static string EscapeRowFilterLikeValue(string value)
        {
            return value
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");
        }

        private static string EscapeCsvValue(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
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
