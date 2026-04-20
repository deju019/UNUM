using System.Windows;
using UNUM.Services;

namespace UNUM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var authService = new AuthService();
                authService.EnsureDemoUser("unum_demo", "Demo1234");
            }
            catch
            {
                // No bloquear el arranque de la app si la BD no esta disponible.
            }

            base.OnStartup(e);
        }
    }

}
