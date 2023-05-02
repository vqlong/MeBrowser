using MaterialDesignThemes.Wpf;
using MeBrowser.Helpers;
using MeBrowser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeBrowser.ViewModel
{
    /// <summary>
    /// Hiện tại không dùng.
    /// </summary>
    class TitleViewModel : BindableBase
    {
        public ICommand Close { get; } = new RelayCommand((obj) => { if (DialogBox.Show("Bạn có thực sự muốn thoát?", "Thông báo", DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes) ((Window)obj).Close(); });
        public ICommand Maximize { get; } = new RelayCommand((obj) =>
        {
            if(obj is Button button && button.FindAncestor<Window>() is Window window)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    //button.Content = new PackIcon { Kind = PackIconKind.CheckboxBlankOutline, Width = 20 };
                }
                else if (window.WindowState == WindowState.Normal)
                {
                    window.WindowState = WindowState.Maximized;
                    //button.Content = new PackIcon { Kind = PackIconKind.CheckboxMultipleBlankOutline, Width = 20 };
                }
            }

        });
        public ICommand Minimize { get; } = new RelayCommand((obj) => ((Window)obj).WindowState = WindowState.Minimized);
        public ICommand DragMove { get; } = new RelayCommand((obj) => ((Window)obj).DragMove());

    }
}
