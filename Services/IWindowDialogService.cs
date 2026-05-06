namespace UNUM.Services;

using UNUM.Models;

public interface IWindowDialogService
{
    bool? ShowObjectiveModal(int usuarioId, ObjectiveItemModel? objective = null);
    void ShowInicioWindow();
}
