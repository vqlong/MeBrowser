using CefSharp.Wpf;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeBrowser.TabBrowsers
{
    /// <summary>
    /// Interaction logic for TabBrowserTest.xaml
    /// </summary>
    public partial class TabBrowserTest : TabItem
    {
        public static readonly DependencyProperty ScreenshotProperty = DependencyProperty.Register("Screenshot", typeof(Brush), typeof(TabBrowserTest), new PropertyMetadata(Brushes.WhiteSmoke));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TabBrowserTest), new PropertyMetadata("MeBrowser ~.~"));
        public static readonly DependencyProperty FaviconSourceProperty = DependencyProperty.Register("FaviconSource", typeof(ImageSource), typeof(TabBrowserTest), new PropertyMetadata(new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/icon.png", UriKind.Absolute))));
        public static readonly DependencyProperty HistoryProperty = DependencyProperty.Register("History", typeof(ObservableCollection<HistoryEntry>), typeof(TabBrowserTest), new PropertyMetadata(null));

        public Brush Screenshot { get => (Brush)GetValue(ScreenshotProperty); set => SetValue(ScreenshotProperty, value); }
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public ImageSource FaviconSource { get => (ImageSource)GetValue(FaviconSourceProperty); set => SetValue(FaviconSourceProperty, value); }
        public ObservableCollection<HistoryEntry> History { get => (ObservableCollection<HistoryEntry>)GetValue(HistoryProperty); set => SetValue(HistoryProperty, value); }

        public ICommand OpenNewTab { get; }
        public ICommand CloseThisTab { get; }
        public ICommand OpenSetupTab { get; }
        public TabControl TabMain { get => (TabControl)Parent; }
        //public ChromiumWebBrowser Chromium => chromium;
        public static TabBrowserTest Default { get; } = new TabBrowserTest();
        public TabBrowserTest()
        {
            OpenNewTab = new RelayCommand(OpenNewTab_Execute);
            CloseThisTab = new RelayCommand(CloseThisTab_Execute);
            OpenSetupTab = new RelayCommand(OpenSetupTab_Execute);

            InitializeComponent();
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
            //tabBrowserChromium.History = History;
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
 
            //Chromium.Dispose();

            if (TabMain.Items.Count == 1)
            {
                Application.Current.MainWindow.Close();
            }

            TabMain.Items.Remove(this);
        }
        private void OpenSetupTab_Execute(object? obj)
        {
            if (TabMain.Items.Contains(TabBrowserSetup.Default))
            {
                TabMain.SelectedItem = TabBrowserSetup.Default;
                return;
            }

            TabMain.Items.Add(TabBrowserSetup.Default);
            //TabMain.SelectedItem = TabSetup.Default;
        }
    }
}
