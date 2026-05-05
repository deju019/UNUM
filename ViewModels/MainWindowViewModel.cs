using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using UNUM.Models;
using UNUM.Services;

namespace UNUM.ViewModels;

public enum MainPanelType
{
    Transacciones,
    Simulador
}

/// <summary>
/// ViewModel para MainWindow. Encapsula toda la lógica de negocio y presentación.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly int _usuarioId;
    private readonly TransactionService _transactionService;
    private readonly ObjectiveService _objectiveService;
    private readonly IWindowDialogService _windowDialogService;

    // Estado de transacciones
    private DataTable _transacciones = new();
    private DataView _transaccionesView = new();
    private bool _isLoadingTransactions;
    private string _loadingMessage = "Cargando transacciones...";

    // Estado financiero
    private FinancialSummaryModel _financialSummary = new();

    // Presupuestos
    private Dictionary<string, decimal> _presupuestosCategorias = new(StringComparer.OrdinalIgnoreCase);
    private ObservableCollection<BudgetCategoryModel> _presupuestosUI = new();

    // Objetivos (simulador)
    private ObservableCollection<ObjectiveItemModel> _objetivos = new();
    private ObjectiveItemModel? _objetivoSeleccionado;
    private bool _menuTransaccionesActivo = true;
    private bool _menuSimuladorActivo;
    private bool _isTransaccionesVisible = true;
    private bool _isSimuladorVisible;

    // Filtros y resultados
    private bool _restaurandoEstadoFiltros;
    private string _filtroTipo = "Todos";
    private string _filtroPeriodo = "Historico";
    private string _filtroCategoria = "Todas";
    private DateTime? _filtroFechaDesde;
    private DateTime? _filtroFechaHasta;
    private string _filtroTextoLibre = string.Empty;
    private string _filtroImporteMin = string.Empty;
    private string _filtroImporteMax = string.Empty;
    private string _resumenResultados = "0 de 0 transacciones";
    private ObservableCollection<string> _filtrosActivosDisplay = new();

    // Formulario de transacción
    private string _tipoTransaccion = "Gasto";
    private string _categoriaTransaccion = string.Empty;
    private string _importeTransaccion = string.Empty;
    private string _descripcionTransaccion = string.Empty;
    private ObservableCollection<string> _opcionesTipo = new();
    private ObservableCollection<string> _opcionesCategoria = new();

    // Presupuestos formulario
    private string _presupuestoCategoriaSeleccionada = string.Empty;
    private string _presupuestoLimiteMensual = string.Empty;
    private ObservableCollection<string> _opcionesPresupuestoCategoria = new();

    // Filtros opciones
    private ObservableCollection<string> _opcionesFiltroTipo = new();
    private ObservableCollection<string> _opcionesFiltroCategoria = new();
    private ObservableCollection<string> _opcionesFiltroPeriodo = new();

    // Comandos
    private ICommand? _guardarTransaccionCommand;
    private ICommand? _cerrarSesionCommand;
    private ICommand? _borrarTransaccionCommand;
    private ICommand? _refrescarCommand;
    private ICommand? _aplicarFiltrosCommand;
    private ICommand? _limpiarFiltrosCommand;
    private ICommand? _exportarCsvCommand;
    private ICommand? _guardarPresupuestoCommand;
    private ICommand? _reiniciarPresupuestosCommand;
    private ICommand? _nuevoObjetivoCommand;
    private ICommand? _borrarObjetivoCommand;
    private ICommand? _mostrarTransaccionesCommand;
    private ICommand? _mostrarSimuladorCommand;
    private ICommand? _selectionChangedTipoCommand;
    private ICommand? _selectionChangedFiltroTipoCommand;

    public event EventHandler? RequestClose;
    public event EventHandler<MainPanelType>? RequestPanelNavigation;

    public MainWindowViewModel(int usuarioId, IWindowDialogService windowDialogService)
    {
        _usuarioId = usuarioId;
        _windowDialogService = windowDialogService;
        _transactionService = new TransactionService();
        _objectiveService = new ObjectiveService();

        InitializarOpciones();
        InitializarFiltros();
    }

    #region Propiedades Públicas (Bindables)

    public DataView TransaccionesView
    {
        get => _transaccionesView;
        set => SetProperty(ref _transaccionesView, value);
    }

    public bool IsLoadingTransactions
    {
        get => _isLoadingTransactions;
        set => SetProperty(ref _isLoadingTransactions, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public FinancialSummaryModel FinancialSummary
    {
        get => _financialSummary;
        set => SetProperty(ref _financialSummary, value);
    }

    public ObservableCollection<BudgetCategoryModel> PresupuestosUI
    {
        get => _presupuestosUI;
        set => SetProperty(ref _presupuestosUI, value);
    }

    public ObservableCollection<ObjectiveItemModel> Objetivos
    {
        get => _objetivos;
        set => SetProperty(ref _objetivos, value);
    }

    public ObjectiveItemModel? ObjetivoSeleccionado
    {
        get => _objetivoSeleccionado;
        set => SetProperty(ref _objetivoSeleccionado, value);
    }

    public bool MenuTransaccionesActivo
    {
        get => _menuTransaccionesActivo;
        set => SetProperty(ref _menuTransaccionesActivo, value);
    }

    public bool MenuSimuladorActivo
    {
        get => _menuSimuladorActivo;
        set => SetProperty(ref _menuSimuladorActivo, value);
    }

    public bool IsTransaccionesVisible
    {
        get => _isTransaccionesVisible;
        set => SetProperty(ref _isTransaccionesVisible, value);
    }

    public bool IsSimuladorVisible
    {
        get => _isSimuladorVisible;
        set => SetProperty(ref _isSimuladorVisible, value);
    }

    public string TipoTransaccion
    {
        get => _tipoTransaccion;
        set
        {
            if (SetProperty(ref _tipoTransaccion, value))
            {
                ActualizarCategoriasPorTipo();
            }
        }
    }

    public string CategoriaTransaccion
    {
        get => _categoriaTransaccion;
        set => SetProperty(ref _categoriaTransaccion, value);
    }

    public string ImporteTransaccion
    {
        get => _importeTransaccion;
        set => SetProperty(ref _importeTransaccion, value);
    }

    public string DescripcionTransaccion
    {
        get => _descripcionTransaccion;
        set => SetProperty(ref _descripcionTransaccion, value);
    }

    public ObservableCollection<string> OpcionesTipo
    {
        get => _opcionesTipo;
        set => SetProperty(ref _opcionesTipo, value);
    }

    public ObservableCollection<string> OpcionesCategoria
    {
        get => _opcionesCategoria;
        set => SetProperty(ref _opcionesCategoria, value);
    }

    public string FiltroTipo
    {
        get => _filtroTipo;
        set
        {
            if (SetProperty(ref _filtroTipo, value))
            {
                ActualizarCategoriasFiltro();
            }
        }
    }

    public string FiltroPeriodo
    {
        get => _filtroPeriodo;
        set => SetProperty(ref _filtroPeriodo, value);
    }

    public string FiltroCategoria
    {
        get => _filtroCategoria;
        set => SetProperty(ref _filtroCategoria, value);
    }

    public DateTime? FiltroFechaDesde
    {
        get => _filtroFechaDesde;
        set => SetProperty(ref _filtroFechaDesde, value);
    }

    public DateTime? FiltroFechaHasta
    {
        get => _filtroFechaHasta;
        set => SetProperty(ref _filtroFechaHasta, value);
    }

    public string FiltroTextoLibre
    {
        get => _filtroTextoLibre;
        set => SetProperty(ref _filtroTextoLibre, value);
    }

    public string FiltroImporteMin
    {
        get => _filtroImporteMin;
        set => SetProperty(ref _filtroImporteMin, value);
    }

    public string FiltroImporteMax
    {
        get => _filtroImporteMax;
        set => SetProperty(ref _filtroImporteMax, value);
    }

    public string ResumenResultados
    {
        get => _resumenResultados;
        set => SetProperty(ref _resumenResultados, value);
    }

    public ObservableCollection<string> FiltrosActivosDisplay
    {
        get => _filtrosActivosDisplay;
        set => SetProperty(ref _filtrosActivosDisplay, value);
    }

    public ObservableCollection<string> OpcionesFiltroTipo
    {
        get => _opcionesFiltroTipo;
        set => SetProperty(ref _opcionesFiltroTipo, value);
    }

    public ObservableCollection<string> OpcionesFiltroCategoria
    {
        get => _opcionesFiltroCategoria;
        set => SetProperty(ref _opcionesFiltroCategoria, value);
    }

    public ObservableCollection<string> OpcionesFiltroPeriodo
    {
        get => _opcionesFiltroPeriodo;
        set => SetProperty(ref _opcionesFiltroPeriodo, value);
    }

    public string PresupuestoCategoriaSeleccionada
    {
        get => _presupuestoCategoriaSeleccionada;
        set => SetProperty(ref _presupuestoCategoriaSeleccionada, value);
    }

    public string PresupuestoLimiteMensual
    {
        get => _presupuestoLimiteMensual;
        set => SetProperty(ref _presupuestoLimiteMensual, value);
    }

    public ObservableCollection<string> OpcionesPresupuestoCategoria
    {
        get => _opcionesPresupuestoCategoria;
        set => SetProperty(ref _opcionesPresupuestoCategoria, value);
    }

    #endregion

    #region Comandos

    public ICommand CerrarSesionCommand => _cerrarSesionCommand ??= new RelayCommand(_ => CerrarSesion());

    public ICommand GuardarTransaccionCommand => _guardarTransaccionCommand ??= new RelayCommand(_ => GuardarTransaccion());

    public ICommand BorrarTransaccionCommand => _borrarTransaccionCommand ??= new RelayCommand(p => BorrarTransaccion(p));

    public ICommand RefrescarCommand => _refrescarCommand ??= new AsyncRelayCommand(RefrescarAsync);

    public ICommand AplicarFiltrosCommand => _aplicarFiltrosCommand ??= new RelayCommand(_ => AplicarFiltros());

    public ICommand LimpiarFiltrosCommand => _limpiarFiltrosCommand ??= new RelayCommand(_ => LimpiarFiltros());

    public ICommand ExportarCsvCommand => _exportarCsvCommand ??= new RelayCommand(_ => ExportarCsv());

    public ICommand GuardarPresupuestoCommand => _guardarPresupuestoCommand ??= new RelayCommand(_ => GuardarPresupuesto());

    public ICommand ReiniciarPresupuestosCommand => _reiniciarPresupuestosCommand ??= new RelayCommand(_ => ReiniciarPresupuestos());

    public ICommand NuevoObjetivoCommand => _nuevoObjetivoCommand ??= new RelayCommand(_ => NuevoObjetivo());

    public ICommand BorrarObjetivoCommand => _borrarObjetivoCommand ??= new RelayCommand(_ => BorrarObjetivo());

    public ICommand MostrarTransaccionesCommand => _mostrarTransaccionesCommand ??= new RelayCommand(_ => MostrarPanel(MainPanelType.Transacciones));

    public ICommand MostrarSimuladorCommand => _mostrarSimuladorCommand ??= new RelayCommand(_ => MostrarPanel(MainPanelType.Simulador));

    public ICommand TipoSelectionChangedCommand => _selectionChangedTipoCommand ??= new RelayCommand(_ => ActualizarCategoriasPorTipo());

    public ICommand FiltroTipoSelectionChangedCommand => _selectionChangedFiltroTipoCommand ??= new RelayCommand(_ => ActualizarCategoriasFiltro());

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Carga el historial de transacciones de forma asincrónica.
    /// </summary>
    public async Task CargarHistorialAsync()
    {
        IsLoadingTransactions = true;
        LoadingMessage = "Cargando transacciones...";
        try
        {
            _transacciones = await Task.Run(() => _transactionService.GetUserTransactions(_usuarioId));
            AplicarFiltros();
            ActualizarPanelPresupuestos();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al descargar transacciones: {ex.Message}", "Error de Lectura");
        }
        finally
        {
            IsLoadingTransactions = false;
        }
    }

    /// <summary>
    /// Carga presupuestos del usuario desde disco.
    /// </summary>
    public void CargarPresupuestos()
    {
        try
        {
            var ruta = ObtenerRutaPresupuestos();
            if (!File.Exists(ruta)) return;

            var json = File.ReadAllText(ruta);
            var mapa = JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, decimal>>>(json)
                       ?? new Dictionary<int, Dictionary<string, decimal>>();

            _presupuestosCategorias.Clear();
            if (mapa.TryGetValue(_usuarioId, out var presupuestosUsuario) && presupuestosUsuario is not null)
            {
                foreach (var kv in presupuestosUsuario)
                {
                    _presupuestosCategorias[kv.Key] = kv.Value;
                }
            }
        }
        catch
        {
            // Fallo silencioso
        }
    }

    /// <summary>
    /// Carga objetivos de ahorro para el panel simulador.
    /// </summary>
    public void CargarObjetivos()
    {
        try
        {
            var table = _objectiveService.GetObjectives(_usuarioId);
            Objetivos.Clear();

            foreach (DataRow row in table.Rows)
            {
                var prioridad = row["Prioridad"] == DBNull.Value ? 0 : Convert.ToInt32(row["Prioridad"]);
                Objetivos.Add(new ObjectiveItemModel
                {
                    Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt32(row["Id"]),
                    Nombre = row["Nombre"]?.ToString() ?? string.Empty,
                    CosteTotal = row["CosteTotal"] == DBNull.Value ? 0m : Convert.ToDecimal(row["CosteTotal"]),
                    AhorroActual = row["AhorroActual"] == DBNull.Value ? 0m : Convert.ToDecimal(row["AhorroActual"]),
                    Porcentaje = row["ProgresoPorcentaje"] == DBNull.Value ? 0m : Convert.ToDecimal(row["ProgresoPorcentaje"]),
                    Prioridad = prioridad,
                    PrioridadTexto = PrioridadTexto(prioridad)
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar objetivos: {ex.Message}", "Objetivos", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Restaura el estado de filtros del usuario.
    /// </summary>
    public void RestaurarEstadoFiltros()
    {
        try
        {
            var ruta = ObtenerRutaEstadoFiltros();
            var mapa = LeerEstadoFiltros(ruta);
            if (!mapa.TryGetValue(_usuarioId, out var estado)) return;

            _restaurandoEstadoFiltros = true;

            FiltroTipo = estado.Tipo ?? "Todos";
            FiltroPeriodo = estado.Periodo ?? "Historico";
            FiltroCategoria = estado.Categoria ?? "Todas";
            FiltroFechaDesde = estado.FechaDesde;
            FiltroFechaHasta = estado.FechaHasta;
            FiltroTextoLibre = estado.TextoLibre ?? string.Empty;
            FiltroImporteMin = estado.ImporteMin ?? string.Empty;
            FiltroImporteMax = estado.ImporteMax ?? string.Empty;
        }
        catch
        {
            // Fallo silencioso
        }
        finally
        {
            _restaurandoEstadoFiltros = false;
        }
    }

    #endregion

    #region Métodos Privados

    private void InitializarOpciones()
    {
        OpcionesTipo.Add("Gasto");
        OpcionesTipo.Add("Ingreso");

        OpcionesFiltroPeriodo.Add("Historico");
        OpcionesFiltroPeriodo.Add("Mes actual");

        ActualizarCategoriasPorTipo();
        ActualizarCategoriasFiltro();

        foreach (var catGasto in TransactionCategories.CategoriasGasto)
        {
            OpcionesPresupuestoCategoria.Add(catGasto);
        }

        if (OpcionesPresupuestoCategoria.Count > 0)
        {
            PresupuestoCategoriaSeleccionada = OpcionesPresupuestoCategoria[0];
        }
    }

    private void InitializarFiltros()
    {
        OpcionesFiltroTipo.Add("Todos");
        OpcionesFiltroTipo.Add("Ingreso");
        OpcionesFiltroTipo.Add("Gasto");
    }

    private void ActualizarCategoriasPorTipo()
    {
        OpcionesCategoria.Clear();
        var categorias = TransactionCategories.GetCategoriesForType(TipoTransaccion);
        foreach (var cat in categorias)
        {
            OpcionesCategoria.Add(cat);
        }

        if (OpcionesCategoria.Count > 0)
        {
            CategoriaTransaccion = OpcionesCategoria[0];
        }
    }

    private void ActualizarCategoriasFiltro()
    {
        var categoriaActual = FiltroCategoria;

        OpcionesFiltroCategoria.Clear();
        OpcionesFiltroCategoria.Add("Todas");

        IEnumerable<string> categorias = FiltroTipo switch
        {
            "Ingreso" => TransactionCategories.CategoriasIngreso,
            "Gasto" => TransactionCategories.CategoriasGasto,
            _ => TransactionCategories.CategoriasGasto.Concat(TransactionCategories.CategoriasIngreso).Distinct(StringComparer.OrdinalIgnoreCase)
        };

        foreach (var cat in categorias)
        {
            OpcionesFiltroCategoria.Add(cat);
        }

        if (!string.IsNullOrWhiteSpace(categoriaActual) && OpcionesFiltroCategoria.Contains(categoriaActual))
        {
            FiltroCategoria = categoriaActual;
        }
        else
        {
            FiltroCategoria = "Todas";
        }
    }

    private void GuardarTransaccion()
    {
        if (string.IsNullOrWhiteSpace(TipoTransaccion) || string.IsNullOrWhiteSpace(CategoriaTransaccion))
        {
            System.Windows.MessageBox.Show("Selecciona tipo y categoría para continuar.", "Validación");
            return;
        }

        if (!TransactionCategories.IsValidCategoryForType(TipoTransaccion, CategoriaTransaccion))
        {
            System.Windows.MessageBox.Show("La categoría seleccionada no corresponde con el tipo de transacción.", "Validación");
            ActualizarCategoriasPorTipo();
            return;
        }

        if (!decimal.TryParse(ImporteTransaccion, out decimal importe) || importe <= 0)
        {
            System.Windows.MessageBox.Show("Por favor, introduce un importe numérico válido mayor que cero.", "Error de Validación");
            return;
        }

        try
        {
            _transactionService.AddTransaction(_usuarioId, TipoTransaccion, CategoriaTransaccion, importe, DateTime.Now.Date, DescripcionTransaccion);

            System.Windows.MessageBox.Show("Transacción registrada con éxito.", "Operación Completada");

            ImporteTransaccion = string.Empty;
            DescripcionTransaccion = string.Empty;

            _ = CargarHistorialAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al guardar la transacción: {ex.Message}", "Fallo Crítico");
        }
    }

    private void CerrarSesion()
    {
        _windowDialogService.ShowInicioWindow();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void MostrarPanel(MainPanelType panel)
    {
        MenuTransaccionesActivo = panel == MainPanelType.Transacciones;
        MenuSimuladorActivo = panel == MainPanelType.Simulador;
        IsTransaccionesVisible = panel == MainPanelType.Transacciones;
        IsSimuladorVisible = panel == MainPanelType.Simulador;
        RequestPanelNavigation?.Invoke(this, panel);
    }

    private void BorrarTransaccion(object? param)
    {
        if (param is not DataRowView filaSeleccionada)
        {
            System.Windows.MessageBox.Show("Por favor, selecciona una transacción de la tabla para eliminarla.", "Aviso");
            return;
        }

        int idTransaccion = Convert.ToInt32(filaSeleccionada["Id"]);

        if (System.Windows.MessageBox.Show("¿Estás seguro de que deseas eliminar permanentemente esta transacción?", "Confirmar Borrado", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            bool deleted = _transactionService.DeleteTransaction(idTransaccion, _usuarioId);
            if (deleted)
            {
                _ = CargarHistorialAsync();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al intentar eliminar la transacción: {ex.Message}", "Fallo Crítico");
        }
    }

    private async Task RefrescarAsync()
    {
        await CargarHistorialAsync();
    }

    private void AplicarFiltros()
    {
        if (_transacciones.Rows.Count == 0)
        {
            TransaccionesView = _transacciones.DefaultView;
            ActualizarResumenFinanciero(_transacciones);
            ActualizarIndicadoresFiltrosActivos();
            return;
        }

        if (FiltroFechaDesde.HasValue && FiltroFechaHasta.HasValue && FiltroFechaDesde > FiltroFechaHasta)
        {
            System.Windows.MessageBox.Show("La fecha desde no puede ser mayor que la fecha hasta.", "Validación");
            return;
        }

        var filtros = new List<string>();

        if (!string.Equals(FiltroTipo, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            var tipoEscapado = FiltroTipo.Replace("'", "''");
            filtros.Add($"Tipo = '{tipoEscapado}'");
        }

        if (string.Equals(FiltroPeriodo, "Mes actual", StringComparison.OrdinalIgnoreCase))
        {
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            filtros.Add($"Fecha >= #{inicioMes:MM/dd/yyyy}#");
            filtros.Add($"Fecha <= #{finMes:MM/dd/yyyy}#");
        }

        if (!string.Equals(FiltroCategoria, "Todas", StringComparison.OrdinalIgnoreCase))
        {
            var categoriaEscapada = FiltroCategoria.Replace("'", "''");
            filtros.Add($"Categoria = '{categoriaEscapada}'");
        }

        if (!string.IsNullOrWhiteSpace(FiltroTextoLibre))
        {
            var textoEscapado = EscapeRowFilterLikeValue(FiltroTextoLibre.Replace("'", "''"));
            filtros.Add($"(ISNULL(Descripcion, '') LIKE '%{textoEscapado}%' OR Categoria LIKE '%{textoEscapado}%' OR Tipo LIKE '%{textoEscapado}%')");
        }

        if (!string.IsNullOrWhiteSpace(FiltroImporteMin))
        {
            if (!TryParseImporte(FiltroImporteMin, out var valorMin))
            {
                System.Windows.MessageBox.Show("El importe mínimo no es válido.", "Validación");
                return;
            }
            filtros.Add($"Importe >= {valorMin.ToString(CultureInfo.InvariantCulture)}");
        }

        if (!string.IsNullOrWhiteSpace(FiltroImporteMax))
        {
            if (!TryParseImporte(FiltroImporteMax, out var valorMax))
            {
                System.Windows.MessageBox.Show("El importe máximo no es válido.", "Validación");
                return;
            }
            filtros.Add($"Importe <= {valorMax.ToString(CultureInfo.InvariantCulture)}");
        }

        if (FiltroFechaDesde.HasValue)
        {
            filtros.Add($"Fecha >= #{FiltroFechaDesde.Value:MM/dd/yyyy}#");
        }

        if (FiltroFechaHasta.HasValue)
        {
            filtros.Add($"Fecha <= #{FiltroFechaHasta.Value:MM/dd/yyyy}#");
        }

        var vista = _transacciones.DefaultView;
        vista.RowFilter = string.Join(" AND ", filtros);

        TransaccionesView = vista;
        ActualizarResumenFinanciero(_transacciones);
        ActualizarIndicadoresFiltrosActivos();

        if (!_restaurandoEstadoFiltros)
        {
            GuardarEstadoFiltros();
        }
    }

    private void LimpiarFiltros()
    {
        FiltroTipo = "Todos";
        FiltroPeriodo = "Historico";
        ActualizarCategoriasFiltro();
        FiltroFechaDesde = null;
        FiltroFechaHasta = null;
        FiltroTextoLibre = string.Empty;
        FiltroImporteMin = string.Empty;
        FiltroImporteMax = string.Empty;

        IsLoadingTransactions = true;
        LoadingMessage = "Limpiando filtros...";
        AplicarFiltros();
        IsLoadingTransactions = false;
    }

    private void ExportarCsv()
    {
        if (TransaccionesView?.Count == 0)
        {
            System.Windows.MessageBox.Show("No hay transacciones para exportar.", "Exportar CSV");
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
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
            if (TransaccionesView?.Table is null)
            {
                System.Windows.MessageBox.Show("No se pudo acceder a los datos para exportar.", "Exportar CSV");
                return;
            }

            using var writer = new StreamWriter(dialog.FileName, false);
            var columnas = TransaccionesView.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            writer.WriteLine(string.Join(",", columnas.Select(EscapeCsvValue)));

            foreach (DataRowView fila in TransaccionesView)
            {
                var valores = columnas.Select(col => fila.Row[col]?.ToString() ?? string.Empty);
                writer.WriteLine(string.Join(",", valores.Select(EscapeCsvValue)));
            }

            System.Windows.MessageBox.Show("CSV exportado correctamente.", "Exportar CSV");
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al exportar CSV: {ex.Message}", "Exportar CSV");
        }
    }

    private void GuardarPresupuesto()
    {
        if (string.IsNullOrWhiteSpace(PresupuestoCategoriaSeleccionada))
        {
            System.Windows.MessageBox.Show("Selecciona una categoría para guardar el presupuesto.", "Presupuestos");
            return;
        }

        if (!TryParseImporte(PresupuestoLimiteMensual, out var limite) || limite <= 0)
        {
            System.Windows.MessageBox.Show("Introduce un límite mensual válido mayor que cero.", "Presupuestos");
            return;
        }

        _presupuestosCategorias[PresupuestoCategoriaSeleccionada] = limite;
        GuardarPresupuestosCategorias();
        ActualizarPanelPresupuestos();
        PresupuestoLimiteMensual = string.Empty;
    }

    private void ReiniciarPresupuestos()
    {
        if (System.Windows.MessageBox.Show("¿Quieres borrar todos los presupuestos de categorías?", "Presupuestos", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        _presupuestosCategorias.Clear();
        GuardarPresupuestosCategorias();
        ActualizarPanelPresupuestos();
    }

    private void NuevoObjetivo()
    {
        var result = _windowDialogService.ShowObjectiveModal(_usuarioId);
        if (result == true)
        {
            CargarObjetivos();
        }
    }

    private void BorrarObjetivo()
    {
        if (ObjetivoSeleccionado is null)
        {
            MessageBox.Show("Selecciona un objetivo para eliminar.", "Objetivos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmacion = MessageBox.Show("Se eliminara el objetivo seleccionado. Quieres continuar?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmacion != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var deleted = _objectiveService.DeleteObjective(ObjetivoSeleccionado.Id, _usuarioId);
            if (deleted)
            {
                CargarObjetivos();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar objetivo: {ex.Message}", "Objetivos", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ActualizarPanelPresupuestos()
    {
        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var finMes = inicioMes.AddMonths(1);
        var gastoPorCategoria = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (DataRow row in _transacciones.Rows)
        {
            if (row["Tipo"] == DBNull.Value || row["Categoria"] == DBNull.Value || row["Importe"] == DBNull.Value || row["Fecha"] == DBNull.Value)
            {
                continue;
            }

            var tipo = row["Tipo"]?.ToString() ?? string.Empty;
            if (!string.Equals(tipo, "Gasto", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fecha = Convert.ToDateTime(row["Fecha"]);
            if (fecha < inicioMes || fecha >= finMes)
            {
                continue;
            }

            var categoria = row["Categoria"]?.ToString() ?? "Otros";
            var importe = Convert.ToDecimal(row["Importe"]);

            gastoPorCategoria.TryGetValue(categoria, out var acumulado);
            gastoPorCategoria[categoria] = acumulado + importe;
        }

        var items = new List<BudgetCategoryModel>();
        foreach (var presupuesto in _presupuestosCategorias.OrderBy(x => x.Key))
        {
            gastoPorCategoria.TryGetValue(presupuesto.Key, out var gastado);
            var porcentaje = presupuesto.Value > 0
                ? Math.Min(100m, decimal.Round((gastado / presupuesto.Value) * 100m, 1))
                : 0m;

            var colorHex = porcentaje >= 100m
                ? "#DC3545"
                : porcentaje >= 80m
                    ? "#FFC107"
                    : "#28A745";

            items.Add(new BudgetCategoryModel
            {
                Categoria = presupuesto.Key,
                Gastado = gastado,
                Limite = presupuesto.Value,
                Porcentaje = porcentaje,
                Resumen = $"{gastado:0.00} € / {presupuesto.Value:0.00} €",
                ColorHex = colorHex
            });
        }

        if (items.Count == 0)
        {
            items.Add(new BudgetCategoryModel
            {
                Categoria = "Sin presupuestos",
                Resumen = "Define un límite mensual para empezar.",
                Porcentaje = 0,
                ColorHex = "#6C757D"
            });
        }

        PresupuestosUI.Clear();
        foreach (var item in items)
        {
            PresupuestosUI.Add(item);
        }
    }

    private void ActualizarResumenFinanciero(DataTable transacciones)
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

        FinancialSummary = new FinancialSummaryModel
        {
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            Saldo = saldo
        };
    }

    private void ActualizarIndicadoresFiltrosActivos()
    {
        FiltrosActivosDisplay.Clear();

        if (!string.Equals(FiltroTipo, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            FiltrosActivosDisplay.Add($"Tipo: {FiltroTipo}");
        }

        if (!string.Equals(FiltroPeriodo, "Historico", StringComparison.OrdinalIgnoreCase))
        {
            FiltrosActivosDisplay.Add($"Período: {FiltroPeriodo}");
        }

        if (!string.Equals(FiltroCategoria, "Todas", StringComparison.OrdinalIgnoreCase))
        {
            FiltrosActivosDisplay.Add($"Categoría: {FiltroCategoria}");
        }

        if (!string.IsNullOrWhiteSpace(FiltroTextoLibre))
        {
            FiltrosActivosDisplay.Add($"Texto: {FiltroTextoLibre}");
        }

        if (!string.IsNullOrWhiteSpace(FiltroImporteMin))
        {
            FiltrosActivosDisplay.Add($"Min: {FiltroImporteMin} €");
        }

        if (!string.IsNullOrWhiteSpace(FiltroImporteMax))
        {
            FiltrosActivosDisplay.Add($"Max: {FiltroImporteMax} €");
        }

        if (FiltroFechaDesde.HasValue)
        {
            FiltrosActivosDisplay.Add($"Desde: {FiltroFechaDesde.Value:dd/MM/yyyy}");
        }

        if (FiltroFechaHasta.HasValue)
        {
            FiltrosActivosDisplay.Add($"Hasta: {FiltroFechaHasta.Value:dd/MM/yyyy}");
        }

        var total = _transacciones.Rows.Count;
        var visibles = TransaccionesView?.Count ?? total;
        ResumenResultados = $"{visibles} de {total} transacciones";
    }

    private void GuardarEstadoFiltros()
    {
        try
        {
            var ruta = ObtenerRutaEstadoFiltros();
            var mapa = LeerEstadoFiltros(ruta);

            mapa[_usuarioId] = new FilterStateModel
            {
                Tipo = FiltroTipo,
                Periodo = FiltroPeriodo,
                Categoria = FiltroCategoria,
                FechaDesde = FiltroFechaDesde,
                FechaHasta = FiltroFechaHasta,
                TextoLibre = FiltroTextoLibre,
                ImporteMin = FiltroImporteMin,
                ImporteMax = FiltroImporteMax
            };

            var json = JsonSerializer.Serialize(mapa, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ruta, json);
        }
        catch
        {
            // Fallo silencioso
        }
    }

    private static bool TryParseImporte(string texto, out decimal valor)
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

    private static string PrioridadTexto(int prioridad)
    {
        return prioridad switch
        {
            1 => "Alta",
            2 => "Media",
            3 => "Baja",
            _ => "-"
        };
    }

    private static string ObtenerRutaPresupuestos()
    {
        var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UNUM");
        return Path.Combine(carpeta, "presupuestos-mainwindow.json");
    }

    private static string ObtenerRutaEstadoFiltros()
    {
        var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UNUM");
        Directory.CreateDirectory(carpeta);
        return Path.Combine(carpeta, "filtros-mainwindow.json");
    }

    private static Dictionary<int, FilterStateModel> LeerEstadoFiltros(string ruta)
    {
        if (!File.Exists(ruta))
        {
            return new Dictionary<int, FilterStateModel>();
        }

        var json = File.ReadAllText(ruta);
        return JsonSerializer.Deserialize<Dictionary<int, FilterStateModel>>(json) ?? new Dictionary<int, FilterStateModel>();
    }

    private void GuardarPresupuestosCategorias()
    {
        try
        {
            var ruta = ObtenerRutaPresupuestos();
            var carpeta = Path.GetDirectoryName(ruta);
            if (!string.IsNullOrWhiteSpace(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            Dictionary<int, Dictionary<string, decimal>> mapa;
            if (File.Exists(ruta))
            {
                var jsonActual = File.ReadAllText(ruta);
                mapa = JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, decimal>>>(jsonActual)
                       ?? new Dictionary<int, Dictionary<string, decimal>>();
            }
            else
            {
                mapa = new Dictionary<int, Dictionary<string, decimal>>();
            }

            mapa[_usuarioId] = _presupuestosCategorias.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            var json = JsonSerializer.Serialize(mapa, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ruta, json);
        }
        catch
        {
            // Fallo silencioso
        }
    }

    #endregion
}
