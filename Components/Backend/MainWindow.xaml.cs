using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UNUM.Services;
using UNUM.ViewModels;

namespace UNUM
{
    public partial class MainWindow : Window
    {
        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel(idUsuarioLogueado, new WindowDialogService());
            viewModel.RequestClose += (_, _) => Close();
            if (viewModel is System.ComponentModel.INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += ViewModel_PropertyChanged;
            }
            DataContext = viewModel;
            Loaded += MainWindow_Loaded;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;

            if (e.PropertyName == nameof(MainWindowViewModel.IsTransaccionesVisible) && vm.IsTransaccionesVisible)
            {
                AnimarCambioPanel(panelSimulador, panelTransacciones, true);
            }

            if (e.PropertyName == nameof(MainWindowViewModel.IsSimuladorVisible) && vm.IsSimuladorVisible)
            {
                AnimarCambioPanel(panelTransacciones, panelSimulador, false);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel viewModel) return;
            viewModel.CargarPresupuestos();
            viewModel.CargarObjetivos();
            viewModel.RestaurarEstadoFiltros();
            await viewModel.CargarHistorialAsync();
            IniciarAnimacionesIniciales();
        }

        private void NavigateToPanel(MainPanelType panel)
        {
            if (panel == MainPanelType.Transacciones)
            {
                AnimarCambioPanel(panelSimulador, panelTransacciones, true);
                return;
            }

            AnimarCambioPanel(panelTransacciones, panelSimulador, false);
        }

        private void IniciarAnimacionesIniciales()
        {
            AnimarEntradaElemento(borderRegistroTransaccion, 0);
            AnimarEntradaElemento(borderFiltrosBusqueda, 90);
            AnimarEntradaElemento(panelHistorialTransacciones, 180);
            AnimarEntradaElemento(panelAccionesRapidas, 260);
        }

        private static void AnimarEntradaElemento(UIElement elemento, int delayMs)
        {
            if (elemento is not FrameworkElement framework) return;
            framework.Opacity = 0;
            framework.RenderTransform = new TranslateTransform(0, 14);
            var storyboard = new Storyboard();
            var fade = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(320), BeginTime = TimeSpan.FromMilliseconds(delayMs), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var slide = new DoubleAnimation { From = 14, To = 0, Duration = TimeSpan.FromMilliseconds(320), BeginTime = TimeSpan.FromMilliseconds(delayMs), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(fade, framework);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(slide, framework);
            Storyboard.SetTargetProperty(slide, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(fade);
            storyboard.Children.Add(slide);
            storyboard.Begin();
        }

        private static void AnimarCambioPanel(Grid panelOcultar, Grid panelMostrar, bool desdeIzquierda)
        {
            if (panelOcultar == panelMostrar) return;
            panelMostrar.Visibility = Visibility.Visible;
            panelMostrar.Opacity = 0;
            panelMostrar.RenderTransform = new TranslateTransform(desdeIzquierda ? -24 : 24, 0);
            if (panelOcultar.Visibility == Visibility.Visible)
            {
                panelOcultar.RenderTransform = new TranslateTransform(0, 0);
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
                fadeOut.Completed += (_, _) =>
                {
                    panelOcultar.Visibility = Visibility.Collapsed;
                    panelOcultar.Opacity = 1;
                    panelOcultar.RenderTransform = new TranslateTransform(0, 0);
                };
                panelOcultar.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            else panelOcultar.Visibility = Visibility.Collapsed;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var slideIn = new DoubleAnimation(desdeIzquierda ? -24 : 24, 0, TimeSpan.FromMilliseconds(220)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            panelMostrar.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            (panelMostrar.RenderTransform as TranslateTransform)?.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }
    }
}
