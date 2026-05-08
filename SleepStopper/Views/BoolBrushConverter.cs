using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SleepStopper.Views;

public class BoolBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = Brushes.LimeGreen;
    public IBrush FalseBrush { get; set; } = Brushes.Gray;

    public static readonly BoolBrushConverter SuccessOrMuted = new()
    {
        TrueBrush = new SolidColorBrush(Color.Parse("#3ecf8e")),
        FalseBrush = new SolidColorBrush(Color.Parse("#5f6573"))
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TrueBrush : FalseBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
