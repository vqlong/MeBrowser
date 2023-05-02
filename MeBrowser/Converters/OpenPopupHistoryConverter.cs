using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MeBrowser.Converters
{
    /// <summary>
    /// Hiện tại không dùng.
    /// </summary>
    public class OpenPopupHistoryConverter : IMultiValueConverter
    {
        public static OpenPopupHistoryConverter Default { get; } = new OpenPopupHistoryConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] is int intValue && intValue == 0) return false;
            if (values[2] is true) return false;

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
