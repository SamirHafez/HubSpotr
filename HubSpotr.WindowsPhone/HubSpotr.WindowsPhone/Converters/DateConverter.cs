using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace HubSpotr.WindowsPhone.Converters
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (DateTime)value;

            if (date.Date < DateTime.Today.AddDays(-1))
                return string.Format("{0:d}", date);
            else if (date.Date < DateTime.Today)
                return "yesterday";
            else
                return string.Format("{0:t}", date);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
