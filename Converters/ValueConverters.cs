using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace UNUM.Converters;

/// <summary>
/// Formatea un valor decimal como moneda con símbolo €
/// </summary>
public class DecimalToEuroStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return $"{decimalValue:0.00} €";
        }

        return "0.00 €";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un valor booleano a un Brush (color).
/// True → Verde, False → Rojo
/// </summary>
public class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))   // Verde
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));  // Rojo
        }

        return new SolidColorBrush(Color.FromRgb(108, 117, 125)); // Gris por defecto
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un porcentaje (0-100) a un Brush de color.
/// 0-79% → Verde, 80-99% → Amarillo, 100%+ → Rojo
/// </summary>
public class PercentageToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var percentage = value switch
        {
            decimal d => (double)d,
            double db => db,
            _ => (double?)null
        };

        if (percentage.HasValue)
        {
            if (percentage.Value >= 100)
            {
                return new SolidColorBrush(Color.FromRgb(220, 53, 69));    // Rojo
            }

            if (percentage.Value >= 80)
            {
                return new SolidColorBrush(Color.FromRgb(255, 193, 7));    // Amarillo
            }

            return new SolidColorBrush(Color.FromRgb(40, 167, 69));        // Verde
        }

        return new SolidColorBrush(Color.FromRgb(108, 117, 125));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un booleano a Visibility.
/// True → Visible, False → Collapsed.
/// Si parameter == "Invert", invierte el resultado.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var invert = string.Equals(parameter?.ToString(), "Invert", StringComparison.OrdinalIgnoreCase);

        if (invert)
        {
            boolValue = !boolValue;
        }

        return boolValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte un booleano de estado activo de menú a Brush.
/// True -> color activo, False -> transparente.
/// </summary>
public class MenuActiveBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            return new SolidColorBrush(Color.FromRgb(52, 73, 94));
        }

        return Brushes.Transparent;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Convierte Visibility basado en si una colección está vacía.
/// Count == 0 → Visible, Count > 0 → Collapsed
/// </summary>
public class EmptyCollectionToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is System.Collections.ICollection collection)
        {
            return collection.Count == 0
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        return System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
