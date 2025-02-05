using System.Globalization;
using System.Windows.Data;

namespace CdAudioWpfUi.Converter
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string parameterString = parameter.ToString();
            if (parameterString == null)
                return false;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return false;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            string parameterString = parameter.ToString();
            if (parameterString == null)
                return null;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
