namespace UNUM.Services;

using UNUM.Models;

public class WindowDialogService : IWindowDialogService
{
    public bool? ShowObjectiveModal(int usuarioId, ObjectiveItemModel? objective = null)
    {
        var modal = new ObjetivoModalWindow(usuarioId, objective);
        return modal.ShowDialog();
    }

    public void ShowInicioWindow()
    {
        new InicioWindow().Show();
    }
}
