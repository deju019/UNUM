using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace UNUM
{
    public partial class InicioWindow : Window
    {
        public InicioWindow()
        {
            InitializeComponent();
            Loaded += InicioWindow_Loaded;
        }

        private void InicioWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AnimarEntradaLogo();

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new System.Uri("pack://application:,,,/Assets/logo.png", System.UriKind.Absolute);
                bitmap.DecodePixelWidth = 256;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();

                imgInicioLogo.Source = bitmap;
            }
            catch
            {
                // If logo cannot be loaded, keep decorative circle only.
            }
        }

        private void AnimarEntradaLogo()
        {
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(420),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            logoContainer.BeginAnimation(UIElement.OpacityProperty, fade);

            if (logoContainer.RenderTransform is not TranslateTransform translate)
            {
                translate = new TranslateTransform(0, 14);
                logoContainer.RenderTransform = translate;
            }

            var slide = new DoubleAnimation
            {
                From = 14,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(420),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            translate.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void btnIrLogin_Click(object sender, RoutedEventArgs e)
        {
            // Abre la ventana de Login
            LoginWindow login = new LoginWindow();
            login.Show();
            
            // Cierra la ventana de Inicio
            this.Close();
        }

        private void btnIrRegistro_Click(object sender, RoutedEventArgs e)
        {
            // Nota arquitectónica: MainWindow es la ventana que programamos para el Registro
            RegisterWindow registro = new RegisterWindow();
            registro.Show();
            
            // Cierra la ventana de Inicio
            this.Close();
        }
    }
}