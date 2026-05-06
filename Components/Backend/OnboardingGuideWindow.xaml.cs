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
        MostrarFiltros = 5,
        MostrarPresupuestos = 6,
        MostrarObjetivos = 7,
        ExplicarSaldo = 8,
        Completado = 9
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
                TutorialPaso.ExplicarRegistro => (1, "Primero: registra una transacción\n\nEl formulario de arriba te permite elegir tipo, categoría, importe, descripción y fecha. Al pulsar 'Añadir' se guarda el movimiento."),
                TutorialPaso.EsperarAñadir => (2, "Haz una prueba con una transacción\n\nCuando pulses 'Añadir', el tutorial avanzará solo al siguiente paso."),
                TutorialPaso.MostrarHistorial => (3, "Aquí aparece el historial\n\nLa app baja sola a esta zona para enseñarte dónde ver lo que acabas de crear. Desde aquí puedes exportar y revisar todo."),
                TutorialPaso.ExplicarAcciones => (4, "Acciones rápidas\n\nExporta a CSV, borra la transacción marcada o edítala. Los botones están pensados para trabajar sobre la fila seleccionada."),
                TutorialPaso.PedirBorrar => (5, "Prueba la eliminación\n\nSelecciona una fila y pulsa 'Eliminar Seleccionada' para ver cómo funciona el borrado con confirmación."),
                TutorialPaso.MostrarFiltros => (6, "Filtros de búsqueda\n\nLa vista baja a esta parte para mostrarte cómo filtrar por tipo, categoría, período, texto y, si lo necesitas, filtros avanzados."),
                TutorialPaso.MostrarPresupuestos => (7, "Presupuestos mensuales\n\nAquí defines un límite por categoría, editas presupuestos existentes y eliminas uno individual sin tocar los demás."),
                TutorialPaso.MostrarObjetivos => (8, "Objetivos de ahorro\n\nCambia al panel de objetivos para crear metas, editar su progreso y seguir cuánto te falta para cumplir cada una."),
                TutorialPaso.ExplicarSaldo => (9, "Saldo y resumen\n\nMira el saldo actual en la barra lateral. Si baja de cero, la app lo pinta en rojo para avisarte de inmediato."),
                _ => (0, "Tutorial completado. Ya conoces las partes principales de UNUM.")
            };

            txtPaso.Text = $"{paso}/9";
            txtGuiaTexto.Text = titulo;
            btnSiguiente.Content = _pasoActual == TutorialPaso.Completado ? "Cerrar" : "Siguiente →";

            // Deshabilitar botón en pasos automáticos
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
