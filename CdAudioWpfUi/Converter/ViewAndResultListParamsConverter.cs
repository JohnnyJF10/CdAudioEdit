using System.Globalization;
using System.Windows.Data;

namespace CdAudioWpfUi.Converter
{
    public class ViewAndResultListParamsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length == 2 ?
            (View: values[0], SelectedItems: values[1])
            :
            throw new ArgumentException("Exactly two values are expected.");

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
