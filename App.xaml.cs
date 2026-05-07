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
            // Control manual de la ventana de arranque para soportar autorelogin
            base.OnStartup(e);

            try
            {
                var saved = UNUM.Services.SessionService.LoadSession();
                if (saved.HasValue)
                {
                    // Intentar abrir MainWindow con el id guardado
                    try
                    {
                        var main = new MainWindow(saved.Value);
                        main.Show();
                        return;
                    }
                    catch
                    {
                        // Si falla (BD, usuario eliminado...), limpiar sesión y caer al inicio
                        UNUM.Services.SessionService.ClearSession();
                    }
                }
            }
            catch { }

            // Si no hay sesión válida, abrir la pantalla de inicio
            var inicio = new InicioWindow();
            inicio.Show();
        }
    }

}
