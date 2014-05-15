using System;
using System.Globalization;
using System.Windows.Data;

namespace HubSpotr.WindowsPhone.Converters
{
    public class FacebookIdPhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("https://graph.facebook.com/{0}/picture?width=90&height=90", value.ToString().Substring("facebook:".Length));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
