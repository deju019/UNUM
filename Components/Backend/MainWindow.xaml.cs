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
        private readonly int _usuarioId;
        private OnboardingGuideWindow? _onboardingGuideWindow;
        private OnboardingService? _onboardingService;

        public MainWindow(int idUsuarioLogueado)
        {
            InitializeComponent();
            _usuarioId = idUsuarioLogueado;
            var viewModel = new MainWindowViewModel(idUsuarioLogueado, new WindowDialogService());
            viewModel.RequestClose += (_, _) => Close();
            viewModel.TransactionSaved += ViewModel_TransactionSaved;
            viewModel.TransactionsLoaded += ViewModel_TransactionsLoaded;
            viewModel.TransactionDeleted += ViewModel_TransactionDeleted;
            if (viewModel is System.ComponentModel.INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += ViewModel_PropertyChanged;
            }
            DataContext = viewModel;
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
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
            MostrarOnboardingSiCorresponde();
            IniciarAnimacionesIniciales();
        }

        private void MostrarOnboardingSiCorresponde()
        {
            var onboardingService = new OnboardingService();
            if (!onboardingService.ShouldShowOnboarding(_usuarioId))
            {
                return;
            }

            var onboardingWindow = new OnboardingWindow
            {
                Owner = this
            };

            if (onboardingWindow.ShowDialog() != true)
            {
                return;
            }

            AbrirGuiaInteractiva(onboardingService);
        }

        private void AbrirGuiaInteractiva(OnboardingService? onboardingService = null)
        {
            _onboardingService = onboardingService;

            _onboardingGuideWindow = new OnboardingGuideWindow
            {
                Owner = this
            };

            _onboardingGuideWindow.PasoCambiado += OnboardingGuideWindow_PasoCambiado;
            _onboardingGuideWindow.Closed += (_, _) =>
            {
                if (_onboardingService is not null)
                {
                    _onboardingService.MarkDismissed(_usuarioId, true);
                }

                RestaurarFocoTutorial();
                _onboardingGuideWindow = null;
                _onboardingService = null;
            };

            _onboardingGuideWindow.Show();
            _onboardingGuideWindow.Activate();
            ActualizarVistaTutorial(_onboardingGuideWindow.PasoActual);
        }

        private void ViewModel_TransactionSaved(object? sender, EventArgs e)
        {
            AvanzarTutorialSiAplica(TutorialPaso.ExplicarRegistro);
        }

        private void ViewModel_TransactionsLoaded(object? sender, EventArgs e)
        {
            AvanzarTutorialSiAplica(TutorialPaso.EsperarAñadir);
        }

        private void ViewModel_TransactionDeleted(object? sender, EventArgs e)
        {
            AvanzarTutorialSiAplica(TutorialPaso.PedirBorrar);
        }

        private void AvanzarTutorialSiAplica(TutorialPaso pasoEsperado)
        {
            if (_onboardingGuideWindow is null || _onboardingGuideWindow.PasoActual != pasoEsperado)
            {
                return;
            }

            _onboardingGuideWindow.AvanzarPaso();
        }

        private void OnboardingGuideWindow_PasoCambiado(TutorialPaso paso)
        {
            if (_onboardingGuideWindow is null)
            {
                return;
            }

            ActualizarVistaTutorial(paso);
        }

        private void AplicarFocoTutorial(TutorialPaso paso)
        {
            FrameworkElement? objetivo = paso switch
            {
                TutorialPaso.ExplicarRegistro or TutorialPaso.EsperarAñadir => borderRegistroTransaccion,
                TutorialPaso.MostrarHistorial => panelHistorialTransacciones,
                TutorialPaso.ExplicarAcciones or TutorialPaso.PedirBorrar => panelAccionesRapidas,
                TutorialPaso.MostrarFiltros => borderFiltrosBusqueda,
                TutorialPaso.MostrarPresupuestos => borderPresupuestos,
                TutorialPaso.MostrarObjetivos => panelSimulador,
                TutorialPaso.ExplicarSaldo => borderSaldo,
                _ => null
            };

            var secciones = new FrameworkElement[]
            {
                panelSidebar,
                borderSaldo,
                borderRegistroTransaccion,
                borderFiltrosBusqueda,
                borderPresupuestos,
                panelHistorialTransacciones,
                panelAccionesRapidas,
                panelSimulador
            };

            foreach (var seccion in secciones)
            {
                seccion.Opacity = objetivo is null ? 1.0 : (ReferenceEquals(seccion, objetivo) ? 1.0 : 0.45);
            }
        }

        private void RestaurarFocoTutorial()
        {
            panelSidebar.Opacity = 1.0;
            borderSaldo.Opacity = 1.0;
            borderRegistroTransaccion.Opacity = 1.0;
            borderFiltrosBusqueda.Opacity = 1.0;
            borderPresupuestos.Opacity = 1.0;
            panelHistorialTransacciones.Opacity = 1.0;
            panelAccionesRapidas.Opacity = 1.0;
            panelSimulador.Opacity = 1.0;
        }

        private void PosicionarGuiaEnPaso(TutorialPaso paso)
        {
            if (_onboardingGuideWindow is null)
            {
                return;
            }

            FrameworkElement? objetivo = paso switch
            {
                TutorialPaso.ExplicarRegistro or TutorialPaso.EsperarAñadir => borderRegistroTransaccion,
                TutorialPaso.MostrarHistorial => panelHistorialTransacciones,
                TutorialPaso.ExplicarAcciones or TutorialPaso.PedirBorrar => panelAccionesRapidas,
                TutorialPaso.MostrarFiltros => borderFiltrosBusqueda,
                TutorialPaso.MostrarPresupuestos => borderPresupuestos,
                TutorialPaso.MostrarObjetivos => panelSimulador,
                TutorialPaso.ExplicarSaldo => borderSaldo,
                _ => panelTransacciones
            };

            if (objetivo is null || !objetivo.IsLoaded)
            {
                return;
            }

            objetivo.UpdateLayout();

            var screenPoint = objetivo.PointToScreen(new Point(objetivo.ActualWidth + 12, 0));
            var dpi = VisualTreeHelper.GetDpi(this);
            var left = screenPoint.X / dpi.DpiScaleX;
            var top = screenPoint.Y / dpi.DpiScaleY;

            var workArea = SystemParameters.WorkArea;
            var maxLeft = Math.Max(workArea.Left, workArea.Right - _onboardingGuideWindow.Width - 12);
            var maxTop = Math.Max(workArea.Top, workArea.Bottom - _onboardingGuideWindow.Height - 12);

            _onboardingGuideWindow.Left = Math.Max(workArea.Left + 8, Math.Min(left, maxLeft));
            _onboardingGuideWindow.Top = Math.Max(workArea.Top + 8, Math.Min(top, maxTop));
        }

        private void ActualizarVistaTutorial(TutorialPaso paso)
        {
            AplicarFocoTutorial(paso);
            MostrarSeccionTutorial(paso);
            PosicionarGuiaEnPaso(paso);
        }

        private void MostrarSeccionTutorial(TutorialPaso paso)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FrameworkElement? objetivo = paso switch
                {
                    TutorialPaso.ExplicarRegistro or TutorialPaso.EsperarAñadir => borderRegistroTransaccion,
                    TutorialPaso.MostrarHistorial => panelHistorialTransacciones,
                    TutorialPaso.ExplicarAcciones or TutorialPaso.PedirBorrar => panelAccionesRapidas,
                    TutorialPaso.MostrarFiltros => borderFiltrosBusqueda,
                    TutorialPaso.MostrarPresupuestos => borderPresupuestos,
                    TutorialPaso.MostrarObjetivos => panelSimulador,
                    TutorialPaso.ExplicarSaldo => borderSaldo,
                    _ => null
                };

                if (paso == TutorialPaso.MostrarObjetivos)
                {
                    if (DataContext is MainWindowViewModel viewModel && viewModel.IsTransaccionesVisible)
                    {
                        NavigateToPanel(MainPanelType.Simulador);
                    }

                    mainScrollViewer.ScrollToHome();
                    return;
                }

                if (paso == TutorialPaso.ExplicarSaldo)
                {
                    mainScrollViewer.ScrollToHome();
                    return;
                }

                if (DataContext is MainWindowViewModel vm && vm.IsSimuladorVisible)
                {
                    NavigateToPanel(MainPanelType.Transacciones);
                }

                objetivo?.BringIntoView();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.TransactionSaved -= ViewModel_TransactionSaved;
                viewModel.TransactionsLoaded -= ViewModel_TransactionsLoaded;
                viewModel.TransactionDeleted -= ViewModel_TransactionDeleted;
            }

            if (_onboardingGuideWindow is not null)
            {
                _onboardingGuideWindow.PasoCambiado -= OnboardingGuideWindow_PasoCambiado;
                _onboardingGuideWindow.Close();
                _onboardingGuideWindow = null;
            }
        }

        private void btnTutorial_Click(object sender, RoutedEventArgs e)
        {
            AbrirGuiaInteractiva();
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
