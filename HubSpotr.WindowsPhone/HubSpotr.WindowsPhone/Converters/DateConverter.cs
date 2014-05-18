using System;
using System.Globalization;
using System.Windows.Data;
using Humanizer;

namespace HubSpotr.WindowsPhone.Converters
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;

            return date.Humanize(false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
