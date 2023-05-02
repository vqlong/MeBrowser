using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeBrowser.TabBrowsers
{
    /// <summary>
    /// Có thể raise event thông báo mỗi khi add/remove child.
    /// </summary>
    public class TabNotify : TabControl
    {
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(TabNotify), new PropertyMetadata(false));
        public static readonly DependencyProperty TabBarVisibilityProperty = DependencyProperty.Register("TabBarVisibility", typeof(Visibility), typeof(TabNotify), new PropertyMetadata(Visibility.Visible));

        public bool IsLoading { get => (bool)GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
        public Visibility TabBarVisibility { get => (Visibility)GetValue(TabBarVisibilityProperty); set => SetValue(TabBarVisibilityProperty, value); }

        public ICommand Fullscreen { get; }

        public event EventHandler<NotifyCollectionChangedEventArgs>? ItemsChanged;

        public TabNotify()
        {
            Fullscreen = new RelayCommand(Fullscreen_Execute);
        }

        private void Fullscreen_Execute(object? obj)
        {
            if (obj is true or null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized; 
                SetCurrentValue(TabBarVisibilityProperty, Visibility.Collapsed);
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal; 
                SetCurrentValue(TabBarVisibilityProperty, Visibility.Visible);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged?.Invoke(this, e);
            base.OnItemsChanged(e);
        }
    }
}
