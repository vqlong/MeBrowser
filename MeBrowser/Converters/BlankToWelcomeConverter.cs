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
    public class BlankToWelcomeConverter : IValueConverter
    {
        public static BlankToWelcomeConverter Default { get; } = new BlankToWelcomeConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && stringValue == "about:blank") return (string)Application.Current.Resources["BlankTab.Title"];
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
