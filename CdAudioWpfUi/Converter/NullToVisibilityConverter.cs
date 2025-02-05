using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CdAudioWpfUi.Converter
{
    class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            null => Visibility.Hidden,
            _ => Visibility.Visible,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class NullToVisibilityConverterCol : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            null => Visibility.Collapsed,
            _ => Visibility.Visible,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class NullToVisibilityConverterRev : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            null => Visibility.Visible,
            _ => Visibility.Hidden,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
