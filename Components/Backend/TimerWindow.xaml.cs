using System;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace UNUM
{
    public partial class TimerWindow : Window
    {
        private DispatcherTimer _timer;
        private int _segundosRestantes = 20;
        private int _slideActual = 1;

        public TimerWindow()
        {
            InitializeComponent();

            // Configuramos el reloj para que haga "Tick" cada 1 segundo exacto
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _segundosRestantes--;

            // UX: Aviso visual. Si quedan 5 segundos o menos, se pone rojo
            if (_segundosRestantes <= 5)
            {
                txtTiempo.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69)); // Rojo
            }
            else
            {
                txtTiempo.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69)); // Verde
            }

            // Lógica de Auto-Reset
            if (_segundosRestantes == 0)
            {
                _slideActual++;

                if (_slideActual > 20)
                {
                    _timer.Stop();
                    txtTiempo.Text = "FIN";
                    txtSlide.Text = "¡Presentación completada!";
                    return;
                }

                _segundosRestantes = 20; // Reseteamos el reloj
                txtSlide.Text = $"Diapositiva: {_slideActual} / 20";
            }

            if (_segundosRestantes > 0)
            {
                txtTiempo.Text = _segundosRestantes.ToString();
            }
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void btnPausar_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }
    }
}
