using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using MeBrowser.Model;
using MeBrowser.TabBrowsers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MeBrowser.ViewModel
{
    public partial class MainViewModel : BindableBase, ITabBrowser
    {
        public static MainViewModel Default { get; } = new MainViewModel();
        public TabNotify TabMain { get; set; } 
        /// <summary>
        /// Biểu thức để xác định chuỗi nhập vào UrlBox có phải là 1 địa chỉ hợp lệ. [Hiện tại không dùng]
        /// </summary>
        public Regex AddressExpression { get; } = AddressRegex();
        public double TabItemWidth { get => GetProperty<double>(); set => SetProperty(value); } 

        #region Commands
        public ICommand Close { get; } = new RelayCommand(obj =>
        { 
            if (DownloadEntry.IsDownloadInProgress)
            {
                if (DialogBox.Show(DialogBoxParams.CancelDownloadMessage + DialogBoxParams.CloseMessage, DialogBoxParams.CancelDownloadTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes)
                {
                    BrowsingData.CancelAllDownloadsBeforeClose();
                    Application.Current.MainWindow.Close();
                }
                else
                {
                    return;
                }
            }
            else if (DialogBox.Show(DialogBoxParams.CloseMessage, DialogBoxParams.CloseTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes)
            {
                Application.Current.MainWindow.Close();
            }
        });
        public ICommand Maximize { get; } = new RelayCommand(obj =>
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized) Application.Current.MainWindow.WindowState = WindowState.Normal;
            else if (Application.Current.MainWindow.WindowState == WindowState.Normal) Application.Current.MainWindow.WindowState = WindowState.Maximized;
        });
        public ICommand Minimize { get; } = new RelayCommand(obj => Application.Current.MainWindow.WindowState = WindowState.Minimized);
        public ICommand DragMove { get; } = new RelayCommand(obj => Application.Current.MainWindow.DragMove());
        public ICommand SetTabMain { get; }
        /// <summary>
        /// Command OpenNewTab có thể nhận các parameter kiểu string, OpenNewTabParams, OpenNewTabXamlParams, RoutedEventArgs từ event của 1 Selector
        /// </summary>
        public ICommand OpenNewTab { get; }
        public ICommand CloseThisTab { get; }
        public ICommand ResizeTab { get; }
        public ICommand OpenSetupTab { get; }
        public ICommand LoadData { get; }
        public ICommand SaveData { get; }
        #endregion
        private MainViewModel()
        {
            SetTabMain = new RelayCommand(SetTabMain_Execute);
            ResizeTab = new RelayCommand(ResizeTab_Execute);
            OpenNewTab = new RelayCommand(OpenNewTab_Execute); 
            CloseThisTab = new RelayCommand(CloseThisTab_Execute);
            OpenSetupTab = new RelayCommand(OpenSetupTab_Execute);
            LoadData = new RelayCommand(LoadData_Execute);
            SaveData = new RelayCommand(SaveData_Execute);

            //Back = new RelayCommand(obj =>
            //{
            //    //if (CurrentBrowser.CanExecuteJavascriptInMainFrame)
            //    //{
            //    //    //Evaluate javascript and remember the evaluation result
            //    //    JavascriptResponse response = await CurrentBrowser.EvaluateScriptAsync("history.back()");

            //    //    //if (response.Result != null)
            //    //    //{
            //    //    //    //Display the evaluation result if it is not empty
            //    //    //    MessageBox.Show(response.Result.ToString(), "JavaScript Result");
            //    //    //}
            //    //}
            //});
            //Forward = new RelayCommand(obj =>
            //{
            //    //var task = await CurrentBrowser.GetVisibleNavigationEntryAsync();
            //    //task.Wait();
            //    //var entries = task;

            //    //using DevToolsClient devToolsClient = CurrentBrowser.GetDevToolsClient();
            //    //PageClient pageClient = devToolsClient.Page;
            //    //var getNavigationHistoryResponse = await pageClient.GetNavigationHistoryAsync();
            //    //var entries = getNavigationHistoryResponse.Entries;

            //    //if (CurrentBrowser.CanExecuteJavascriptInMainFrame)
            //    //{
            //    //    CurrentBrowser.ExecuteScriptAsync("history.forward()"); 
            //    //}
            //});            
        }

        private void InitializeSettings(Settings settings)
        {
            TabItemWidth = settings.TabItemWidth;
        }
        private void SetTabMain_Execute(object? obj)
        {
            if(obj is TabNotify tabNotify)
            {
                TabMain = tabNotify;
                tabNotify.ItemsChanged += (s, e) =>
                {
                    ResizeTab_Execute();
                };

            }
        }
        private void SaveData_Execute(object? obj)
        {
            //Taọ thư mục Saves nếu chưa có
            Directory.CreateDirectory(Environment.CurrentDirectory + Settings.SAVES_DIRECTORYPATH);
            SaveSettings();
            SaveHistory();
            SaveDownloads();
        }
        private void SaveDownloads()
        {
            using var stream = new FileStream(Settings.DOWNLOADS_FILEPATH, FileMode.Create, FileAccess.Write);
            var serializer = new XmlSerializer(typeof(ObservableCollection<DownloadEntry>));
            serializer.Serialize(stream, BrowsingData.Downloads);
        }
        private void SaveSettings()
        { 
            using var stream = new FileStream(Settings.SETTINGS_FILEPATH, FileMode.Create, FileAccess.Write);
            var serializer = new XmlSerializer(typeof(Settings)); 
            serializer.Serialize(stream, Settings.Current);
        }
        private void SaveHistory()
        { 
            using var stream = new FileStream(Settings.HISTORY_FILEPATH, FileMode.Create, FileAccess.Write); 
            var serializer = new XmlSerializer(typeof(ObservableCollection<HistoryEntry>));
            serializer.Serialize(stream, BrowsingData.History);
        }
        private void LoadData_Execute(object? para)
        {
            LoadSettings();
            LoadHistory();
            LoadDownloads();
        }
        private void LoadDownloads()
        {
            if (File.Exists(Settings.DOWNLOADS_FILEPATH))
            {
                using var stream = new FileStream(Settings.DOWNLOADS_FILEPATH, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(ObservableCollection<DownloadEntry>));
                var obj = serializer.Deserialize(stream);
                if (obj is ObservableCollection<DownloadEntry> downloads)
                {
                    BrowsingData.Downloads = downloads;
                    return;
                }
            }

            BrowsingData.Downloads = new ObservableCollection<DownloadEntry>();
        }
        private void LoadSettings()
        {
            if (File.Exists(Settings.SETTINGS_FILEPATH))
            {
                using var stream = new FileStream(Settings.SETTINGS_FILEPATH, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(Settings));
                var obj = serializer.Deserialize(stream);
                if (obj is Settings settings)
                { 
                    Settings.InitializeSettings(settings);
                    //Khởi tạo TabBrowserSetup khi window chưa lên hình sẽ lỗi cmn popup
                    //TabBrowserSetup.Default.InitializeSettings(settings);
                    InitializeSettings(settings);
                    return;
                }
            }
            Settings.InitializeSettings(Settings.Default);
            //TabBrowserSetup.Default.InitializeSettings(Settings.Default);
            InitializeSettings(Settings.Default);
        }
        private void LoadHistory()
        {
            if (File.Exists(Settings.HISTORY_FILEPATH))
            {
                using var stream = new FileStream(Settings.HISTORY_FILEPATH, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(ObservableCollection<HistoryEntry>));
                var obj = serializer.Deserialize(stream);
                if (obj is ObservableCollection<HistoryEntry> history)
                {
                    BrowsingData.History = history;
                    return;
                }
            }

            BrowsingData.History = new ObservableCollection<HistoryEntry>();
        }
        private void OpenSetupTab_Execute(object? tabItem)
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
        private void OpenNewTab_Execute(object? obj = null)
        {
            var address = Settings.Current.NewTabDefaultAddress;
            var isNewTabSelected = true;
            if (obj is OpenNewTabParams openNewTabParams)
            {
                address = openNewTabParams.LinkUrl;
                isNewTabSelected = openNewTabParams.IsNewTabSelected;
            }
            else if(obj is OpenNewTabXamlParams openNewTabXamlParams)
            {
                address = openNewTabXamlParams.LinkUrl;
                isNewTabSelected = openNewTabXamlParams.IsNewTabSelected;
            }
            else if(obj is RoutedEventArgs args && args.Source is Selector selector && selector.SelectedItem is string selectedString)
            {
                address = selectedString;
            }
            else if(obj is string stringUrl)
            {
                address = stringUrl;
            }

            TabBrowserChromium tabBrowserChromium = new TabBrowserChromium();
            //tabItemBrowser.History = BrowsingData.History;
            tabBrowserChromium.InitializeChromium(address);             

            TabMain.Items.Add(tabBrowserChromium);
            
            //select sang tab mới để browser của nó bắt đầu load
            //nếu tab mới là background thì select quay lại
            var currentIndex = TabMain.SelectedIndex;
            TabMain.SelectedIndex = TabMain.Items.Count - 1;
            if (!isNewTabSelected) TabMain.SelectedIndex = currentIndex;
        }        
        private void CloseThisTab_Execute(object? tabItem)
        {
            if (tabItem is TabBrowserChromium tabBrowserChromium && tabBrowserChromium.CurrentDownload != null && (tabBrowserChromium.CurrentDownload.IsInProgress || tabBrowserChromium.CurrentDownload.IsPaused))
            {
                if (DialogBox.Show(DialogBoxParams.CancelDownloadMessage + DialogBoxParams.CloseMessage, DialogBoxParams.CancelDownloadTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes)
                {
                    tabBrowserChromium.CancelAllDownloadsBeforeClose();
                    tabBrowserChromium.Chromium.Dispose();
                }
                else
                {
                    return;
                }
            }

            //Chromium.Dispose();

            if (TabMain.Items.Count == 1)
            {
                Application.Current.MainWindow.Close();
            }

            TabMain.Items.Remove(tabItem);
        }
        private void ResizeTab_Execute(object? obj = null)
        {
            if (TabMain == null) throw new Exception("SetTabMain phải được gọi trước với parameter là 1 TabControl.");
            //Khi actualwidth của panel đạt tới maxwidth của nó sẽ không còn không gian cho thằng canvas con bên trong hiển thị các item con nữa 
            //nên ta chỉ lấy giới hạn là MaxWidth - 10 là sẽ resize ngay
            var itemsPanelMaxWidth = TabMain.ActualWidth - 200;
            var tabItemMaxWidth = (itemsPanelMaxWidth - 10) / TabMain.Items.Count;
            if (tabItemMaxWidth > Settings.Current.TabItemWidth)
            {
                if (TabItemWidth != Settings.Current.TabItemWidth) TabItemWidth = Settings.Current.TabItemWidth;
            }
            else
            {
                TabItemWidth = tabItemMaxWidth;
            }
        }
        [GeneratedRegex(@"^((http:\/\/)|(https:\/\/))?\w+[\-\w]*\.\w+[\-\w]*[~%&',;=?#@!:\/\w\(\)\*\+\[\]\$\-\.]*$")]
        private static partial Regex AddressRegex();
    }
}
