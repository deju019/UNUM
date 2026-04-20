using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace UNUM
{
    public partial class ObjetivoModalWindow : Window
    {
        private int _usuarioId;

        public ObjetivoModalWindow(int usuarioId)
        {
            InitializeComponent();
            _usuarioId = usuarioId;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || !decimal.TryParse(txtCoste.Text, out decimal coste) || !decimal.TryParse(txtAhorro.Text, out decimal ahorro))
            {
                MessageBox.Show("Rellena todos los campos con valores numéricos válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int prioridad = Convert.ToInt32(((ComboBoxItem)cmbPrioridad.SelectedItem).Tag);

            string connectionString = "Server=127.0.0.1; Port=3306; Database=UNUM; Uid=root; Pwd=admin123;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Objetivos (UsuarioId, Nombre, CosteTotal, AhorroActual, Prioridad) VALUES (@uId, @nom, @coste, @ahorro, @prio)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uId", _usuarioId);
                        cmd.Parameters.AddWithValue("@nom", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@coste", coste);
                        cmd.Parameters.AddWithValue("@ahorro", ahorro);
                        cmd.Parameters.AddWithValue("@prio", prioridad);
                        cmd.ExecuteNonQuery();
                    }
                    this.DialogResult = true; // Cierra el modal e indica éxito al Dashboard
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message);
                }
            }
        }
    }
}