using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MeBrowser.Converters
{
    /// <summary>
    /// Hiện tại không dùng.
    /// </summary>
    public class OpenPopupNavigationConverter : IMultiValueConverter
    {
        public static OpenPopupNavigationConverter Default { get; } = new OpenPopupNavigationConverter();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is true || values[1] is true) return true;
            if (values[2] is true) return false;
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
