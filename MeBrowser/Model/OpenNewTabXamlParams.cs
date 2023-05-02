using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MeBrowser.Model
{
    public class OpenNewTabXamlParams : DependencyObject
    {
        public static readonly DependencyProperty LinkUrlProperty =
            DependencyProperty.Register("LinkUrl", typeof(string), typeof(OpenNewTabXamlParams), new PropertyMetadata("about:blank"));
        public string LinkUrl { get => (string)GetValue(LinkUrlProperty); set => SetValue(LinkUrlProperty, value); }

        public static readonly DependencyProperty IsNewTabSelectedProperty =
            DependencyProperty.Register("IsNewTabSelected", typeof(bool), typeof(OpenNewTabXamlParams), new PropertyMetadata(true));
        public bool IsNewTabSelected { get => (bool)GetValue(IsNewTabSelectedProperty); set => SetValue(IsNewTabSelectedProperty, value); }
    }
}
