namespace UNUM.Services;

public class WindowDialogService : IWindowDialogService
{
    public bool? ShowObjectiveModal(int usuarioId)
    {
        var modal = new ObjetivoModalWindow(usuarioId);
        return modal.ShowDialog();
    }

    public void ShowInicioWindow()
    {
        new InicioWindow().Show();
    }
}
