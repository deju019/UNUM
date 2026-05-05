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
/// Representa un presupuesto de categoría.
/// </summary>
public class BudgetCategoryModel
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Gastado { get; set; }
    public decimal Limite { get; set; }
    public decimal Porcentaje { get; set; }
    public string Resumen { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#28A745";
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
