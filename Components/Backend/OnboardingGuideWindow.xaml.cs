using System.Windows;

namespace UNUM
{
    public enum TutorialPaso
    {
        ExplicarRegistro = 0,
        EsperarAñadir = 1,
        MostrarHistorial = 2,
        ExplicarAcciones = 3,
        PedirBorrar = 4,
        Completado = 5
    }

    public partial class OnboardingGuideWindow : Window
    {
        private TutorialPaso _pasoActual = TutorialPaso.ExplicarRegistro;
        public event Action<TutorialPaso>? PasoCambiado;
        public TutorialPaso PasoActual
        {
            get { return _pasoActual; }
            set
            {
                _pasoActual = value;
                ActualizarContenidoPaso();
                PasoCambiado?.Invoke(_pasoActual);
            }
        }

        public OnboardingGuideWindow()
        {
            InitializeComponent();
            ActualizarContenidoPaso();
        }

        private void ActualizarContenidoPaso()
        {
            var (paso, titulo) = _pasoActual switch
            {
                TutorialPaso.ExplicarRegistro => (1, "Registra tu primer gasto o ahorro\n\nHaz clic en 'Tipo' y selecciona si es Ingreso o Gasto, luego completa los demás campos y presiona 'Añadir'."),
                TutorialPaso.EsperarAñadir => (2, "Espera a que registres una transacción...\n\n(Haz clic en 'Añadir' para continuar)"),
                TutorialPaso.MostrarHistorial => (3, "¡Perfecto! Tu transacción aparece aquí\n\nEl historial muestra todas tus operaciones. Puedes exportar a CSV o eliminar filas individual."),
                TutorialPaso.ExplicarAcciones => (4, "Acciones disponibles:\n\n📥 Exportar: Descarga datos como CSV\n🗑️ Eliminar: Borra la fila seleccionada"),
                TutorialPaso.PedirBorrar => (5, "Última parte: Haz clic en la fila que acabas de crear y presiona 'Eliminar Seleccionada'"),
                _ => (0, "Tutorial completado. ¡Listo para usar UNUM!")
            };

            txtPaso.Text = $"{paso}/5";
            txtGuiaTexto.Text = titulo;
            btnSiguiente.Content = _pasoActual == TutorialPaso.Completado ? "Cerrar" : "Siguiente →";

            // Desabilitar botón en pasos automáticos
            btnSiguiente.IsEnabled = _pasoActual != TutorialPaso.EsperarAñadir && _pasoActual != TutorialPaso.PedirBorrar;
        }

        private void btnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (_pasoActual == TutorialPaso.EsperarAñadir || _pasoActual == TutorialPaso.PedirBorrar)
            {
                // Estos pasos se avanzan programáticamente desde MainWindow
                return;
            }

            if (_pasoActual == TutorialPaso.Completado)
            {
                this.Close();
                return;
            }

            PasoActual = (TutorialPaso)((int)_pasoActual + 1);
        }

        private void btnSaltar_Click(object sender, RoutedEventArgs e)
        {
            PasoActual = TutorialPaso.Completado;
            this.Close();
        }

        public void AvanzarPaso()
        {
            if (_pasoActual < TutorialPaso.Completado)
            {
                PasoActual = (TutorialPaso)((int)_pasoActual + 1);
                if (_pasoActual == TutorialPaso.Completado)
                {
                    Close();
                }
            }
        }
    }
}
