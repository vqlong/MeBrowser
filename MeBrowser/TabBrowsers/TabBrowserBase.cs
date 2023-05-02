using CefSharp;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MeBrowser.TabBrowsers
{
    public class TabBrowserBase : TabItem, ITabBrowser
    {
        public static readonly DependencyProperty ScreenshotProperty = DependencyProperty.Register("Screenshot", typeof(Brush), typeof(TabBrowserBase), new PropertyMetadata(Brushes.WhiteSmoke));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TabBrowserBase), new PropertyMetadata("Me~~"));

        public Brush Screenshot { get => (Brush)GetValue(ScreenshotProperty); set => SetValue(ScreenshotProperty, value); }
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public ICommand OpenNewTab { get; }
        public ICommand CloseThisTab { get; }
        public ICommand OpenSetupTab { get; } 
        public TabNotify TabMain { get => (TabNotify)Parent; }
        public TabBrowserBase()
        {
            OpenNewTab = new RelayCommand(OpenNewTab_Execute);
            CloseThisTab = new RelayCommand(CloseThisTab_Execute);
            OpenSetupTab = new RelayCommand(OpenSetupTab_Execute);

            Task.Run(() =>
            {
                while (true)
                {
                    var operation = Dispatcher.BeginInvoke(() =>
                    {
                        if (IsLoaded)
                        {
                            if (Parent is not TabNotify) throw new Exception("TabBrowserBase chỉ dùng cho TabNotify.");
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    operation.Wait();
                    if (operation.Result is false) Thread.Sleep(100);
                    else break;
                }
            });
        }
        public override string ToString()
        {
            return GetType().FullName + " " + Title;
        }

        private void OpenNewTab_Execute(object? obj = null)
        {
            var address = Settings.Current.NewTabDefaultAddress;
            var isNewTabSelected = true;
            if (obj is OpenNewTabParams openNewTabParams)
            {
                address = openNewTabParams.LinkUrl;
                isNewTabSelected = openNewTabParams.IsNewTabSelected;
            }
            else if (obj is OpenNewTabXamlParams openNewTabXamlParams)
            {
                address = openNewTabXamlParams.LinkUrl;
                isNewTabSelected = openNewTabXamlParams.IsNewTabSelected;
            }
            else if (obj is RoutedEventArgs args && args.Source is Selector selector && selector.SelectedItem is string selectedString)
            {
                address = selectedString;
            }
            else if (obj is string stringUrl)
            {
                address = stringUrl;
            }

            TabBrowserChromium tabBrowserChromium = new TabBrowserChromium();  
            tabBrowserChromium.InitializeChromium(address);

            TabMain.Items.Add(tabBrowserChromium);

            //select sang tab mới để browser của nó bắt đầu load
            //nếu tab mới là background thì select quay lại
            var currentIndex = TabMain.SelectedIndex;
            TabMain.SelectedIndex = TabMain.Items.Count - 1;
            if (!isNewTabSelected) TabMain.SelectedIndex = currentIndex;
        }
        private void CloseThisTab_Execute(object? obj)
        {
            if (this is TabBrowserChromium tabBrowserChromium)
            {
                //Nếu có download chưa hoàn thành
                if(tabBrowserChromium.CurrentDownload != null && (tabBrowserChromium.CurrentDownload.IsInProgress || tabBrowserChromium.CurrentDownload.IsPaused))
                {
                    if (DialogBox.Show(DialogBoxParams.CancelDownloadMessage + DialogBoxParams.CloseMessage, DialogBoxParams.CancelDownloadTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes)
                    {
                        tabBrowserChromium.CancelAllDownloadsBeforeClose();
                    }
                    else
                    {
                        return;
                    }
                }
                //nếu đang load mà bị close thì sẽ set lại IsLoading cho TabMain để con bọ dừa không xoay nữa
                TabMain.IsLoading = false;
                tabBrowserChromium.Chromium.Dispose();
            }

            if (TabMain.Items.Count == 1)
            {
                Application.Current.MainWindow.Close();
            }

            TabMain.Items.Remove(this);
        }
        private void OpenSetupTab_Execute(object? obj)
        {
            if (TabBrowserSetup.Default == null) 
            {
                TabBrowserSetup.Default = new TabBrowserSetup();
                TabBrowserSetup.Default.InitializeSettings(Settings.Current); 
            }

            if (TabMain.Items.Contains(TabBrowserSetup.Default))
            {
                TabMain.SelectedItem = TabBrowserSetup.Default;
                return;
            }

            TabMain.Items.Add(TabBrowserSetup.Default);
            TabMain.SelectedItem = TabBrowserSetup.Default;
        }
 
    }
}
