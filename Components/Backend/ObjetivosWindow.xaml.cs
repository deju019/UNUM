using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using UNUM.Services;

namespace UNUM;

public partial class ObjetivosWindow : Window
{
    private readonly int _usuarioId;
    private readonly ObjectiveService _objectiveService = new();

    public ObjetivosWindow(int usuarioId)
    {
        InitializeComponent();
        _usuarioId = usuarioId;
        txtTituloObjetivos.Text = $"Objetivos de ahorro (Usuario {_usuarioId})";
        CargarObjetivos();
    }

    private void btnAgregarObjetivo_Click(object sender, RoutedEventArgs e)
    {
        var nombre = txtNombreObjetivo.Text.Trim();
        var prioridadItem = cmbPrioridad.SelectedItem as ComboBoxItem;
        var prioridadText = prioridadItem?.Content?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show("Introduce un nombre para el objetivo.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(txtCosteTotal.Text, out var costeTotal) || costeTotal <= 0)
        {
            MessageBox.Show("Introduce un coste total valido mayor que cero.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var prioridad = ParsePrioridad(prioridadText);
        if (prioridad == 0)
        {
            MessageBox.Show("Selecciona una prioridad valida.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _objectiveService.AddObjective(_usuarioId, nombre, costeTotal, prioridad);
            txtNombreObjetivo.Clear();
            txtCosteTotal.Clear();
            CargarObjetivos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar objetivo:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnActualizarAhorro_Click(object sender, RoutedEventArgs e)
    {
        if (gridObjetivos.SelectedItem is not DataRowView fila)
        {
            MessageBox.Show("Selecciona un objetivo para actualizar su ahorro.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(txtNuevoAhorro.Text, out var ahorroActual) || ahorroActual < 0)
        {
            MessageBox.Show("Introduce un ahorro valido (0 o mayor).", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var idObjetivo = Convert.ToInt32(fila["Id"]);

        try
        {
            var updated = _objectiveService.UpdateCurrentSavings(idObjetivo, _usuarioId, ahorroActual);
            if (updated)
            {
                txtNuevoAhorro.Clear();
                CargarObjetivos();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar ahorro:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnEliminarObjetivo_Click(object sender, RoutedEventArgs e)
    {
        if (gridObjetivos.SelectedItem is not DataRowView fila)
        {
            MessageBox.Show("Selecciona un objetivo para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var idObjetivo = Convert.ToInt32(fila["Id"]);
        var confirmacion = MessageBox.Show("Se eliminara el objetivo seleccionado. Quieres continuar?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirmacion != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var deleted = _objectiveService.DeleteObjective(idObjetivo, _usuarioId);
            if (deleted)
            {
                CargarObjetivos();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar objetivo:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CargarObjetivos()
    {
        try
        {
            var table = _objectiveService.GetObjectives(_usuarioId);
            gridObjetivos.ItemsSource = table.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar objetivos:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static int ParsePrioridad(string prioridadText)
    {
        if (prioridadText.StartsWith("1", StringComparison.Ordinal))
        {
            return 1;
        }

        if (prioridadText.StartsWith("2", StringComparison.Ordinal))
        {
            return 2;
        }

        if (prioridadText.StartsWith("3", StringComparison.Ordinal))
        {
            return 3;
        }

        return 0;
    }
}
