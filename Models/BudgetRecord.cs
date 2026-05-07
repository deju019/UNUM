namespace UNUM.Models;

/// <summary>
/// Fila de presupuesto tal como viene de la tabla Presupuestos.
/// </summary>
public sealed class BudgetRecord
{
    public int Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public decimal LimiteMensual { get; set; }
}
