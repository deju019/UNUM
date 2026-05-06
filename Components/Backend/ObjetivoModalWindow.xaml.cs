using System;
using System.Windows;
using System.Windows.Controls;
using UNUM.Models;
using UNUM.Services;

namespace UNUM
{
    public partial class ObjetivoModalWindow : Window
    {
        private readonly int _usuarioId;
        private readonly int? _objectiveId;
        private readonly ObjectiveService _objectiveService = new();

        public ObjetivoModalWindow(int usuarioId, ObjectiveItemModel? objective = null)
        {
            InitializeComponent();
            _usuarioId = usuarioId;

            if (objective is null)
            {
                return;
            }

            _objectiveId = objective.Id;
            txtNombre.Text = objective.Nombre;
            txtCoste.Text = objective.CosteTotal.ToString("0.##");
            txtAhorro.Text = objective.AhorroActual.ToString("0.##");
            SetPrioridad(objective.Prioridad);
            Title = "Editar Objetivo";
            btnGuardar.Content = "Guardar cambios";
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || !decimal.TryParse(txtCoste.Text, out var coste) || !decimal.TryParse(txtAhorro.Text, out var ahorro))
            {
                MessageBox.Show("Rellena todos los campos con valores numéricos válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (coste <= 0)
            {
                MessageBox.Show("El coste total debe ser mayor que cero.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ahorro < 0)
            {
                MessageBox.Show("El ahorro actual no puede ser negativo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbPrioridad.SelectedItem is not ComboBoxItem prioridadItem || !int.TryParse(prioridadItem.Tag?.ToString(), out var prioridad))
            {
                MessageBox.Show("Selecciona una prioridad válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_objectiveId.HasValue)
                {
                    _objectiveService.UpdateObjective(_objectiveId.Value, _usuarioId, nombre, coste, ahorro, prioridad);
                }
                else
                {
                    _objectiveService.AddObjective(_usuarioId, nombre, coste, prioridad, ahorro);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SetPrioridad(int prioridad)
        {
            foreach (var item in cmbPrioridad.Items)
            {
                if (item is not ComboBoxItem comboItem)
                {
                    continue;
                }

                var isMatch = int.TryParse(comboItem.Tag?.ToString(), out var value) && value == prioridad;
                comboItem.IsSelected = isMatch;
                if (isMatch)
                {
                    cmbPrioridad.SelectedItem = comboItem;
                    return;
                }
            }
        }
    }
}
