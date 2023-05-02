using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MeBrowser.Converters
{
    public class DateTimeToDateConverter : IValueConverter
    {
        public static DateTimeToDateConverter Default { get; } = new DateTimeToDateConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime) return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            else return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
