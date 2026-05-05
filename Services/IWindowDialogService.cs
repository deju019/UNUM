namespace UNUM.Services;

public interface IWindowDialogService
{
    bool? ShowObjectiveModal(int usuarioId);
    void ShowInicioWindow();
}
