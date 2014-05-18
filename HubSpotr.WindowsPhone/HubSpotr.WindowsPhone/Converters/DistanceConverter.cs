using System;
using System.Globalization;
using System.Windows.Data;

namespace HubSpotr.WindowsPhone.Converters
{
    public class DistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var distance = (double)value;

            var weight = "m";

            if (distance > 1000)
            {
                distance /= 1000;
                weight = "km";
            }

            return string.Format("{0} {1}", (int)distance, weight);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
