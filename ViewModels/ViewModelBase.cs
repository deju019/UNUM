using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UNUM.ViewModels;

/// <summary>
/// Clase base para ViewModels que implementa INotifyPropertyChanged.
/// Proporciona la infraestructura para binding en WPF.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Establece una propiedad y notifica cambios si el valor cambió.
    /// </summary>
    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return false;
        }

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Notifica que una propiedad ha cambiado.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
