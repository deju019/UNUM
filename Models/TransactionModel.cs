namespace UNUM.Models;

/// <summary>
/// Representa una transacción financiera.
/// </summary>
public class TransactionModel
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// Resumen financiero de transacciones.
/// </summary>
public class FinancialSummaryModel
{
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal Saldo { get; set; }
    public bool IsSaldoNegativo => Saldo < 0;
}

/// <summary>
/// Resumen del mes actual para analítica rápida.
/// </summary>
public class MonthlyAnalyticsModel
{
    public decimal IngresosMes { get; set; }
    public decimal GastosMes { get; set; }
    public decimal BalanceMes { get; set; }
}

/// <summary>
/// Proyección simple al cierre del mes actual.
/// </summary>
public class MonthlyProjectionModel
{
    public decimal IngresosProyectados { get; set; }
    public decimal GastosProyectados { get; set; }
    public decimal BalanceProyectado { get; set; }
    public decimal BalanceMesAnterior { get; set; }
    public decimal VariacionVsMesAnterior { get; set; }
    public bool TieneAlertaDeficit => BalanceProyectado < 0;
    public string AlertaDeficit { get; set; } = string.Empty;
    public string ResumenEscenario { get; set; } = string.Empty;
}

/// <summary>
/// Badge visual para indicar el nivel de riesgo financiero.
/// </summary>
public class MonthlyRiskBadgeModel
{
    public string Nivel { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string BackgroundHex { get; set; } = "#E9ECEF";
    public string BorderHex { get; set; } = "#CED4DA";
    public string ForegroundHex { get; set; } = "#6C757D";
}

/// <summary>
/// Métrica de gasto por categoría para el mes actual.
/// </summary>
public class CategoryAnalyticsModel
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Porcentaje { get; set; }
    public string Resumen { get; set; } = string.Empty;
}

/// <summary>
/// Representa un presupuesto de categoría.
/// </summary>
public class BudgetCategoryModel
{
    public int Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public decimal Gastado { get; set; }
    public decimal Limite { get; set; }
    public decimal Porcentaje { get; set; }
    public string Resumen { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#28A745";
    public bool TienePresupuestoReal => Id > 0;
}

/// <summary>
/// Representa un objetivo de ahorro mostrado en el simulador.
/// </summary>
public class ObjectiveItemModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CosteTotal { get; set; }
    public decimal AhorroActual { get; set; }
    public decimal Porcentaje { get; set; }
    public int Prioridad { get; set; }
    public string PrioridadTexto { get; set; } = string.Empty;
}

/// <summary>
/// Estado de filtros persistente.
/// </summary>
public class FilterStateModel
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

/// <summary>
/// Opciones de categoría para transacciones.
/// </summary>
public static class TransactionCategories
{
    public static readonly string[] CategoriasGasto = { "Ocio", "Supermercado", "Facturas", "Otros" };
    public static readonly string[] CategoriasIngreso = { "Nómina", "Otros" };

    public static IEnumerable<string> GetCategoriesForType(string tipo)
    {
        return tipo.Equals("Ingreso", StringComparison.OrdinalIgnoreCase)
            ? CategoriasIngreso
            : CategoriasGasto;
    }

    public static bool IsValidCategoryForType(string tipo, string categoria)
    {
        var validCategories = GetCategoriesForType(tipo);
        return validCategories.Contains(categoria, StringComparer.OrdinalIgnoreCase);
    }
}
