using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MeBrowser.TabBrowsers
{
    /// <summary>
    /// Interaction logic for TabSetup.xaml
    /// </summary>
    public partial class TabBrowserSetup : TabBrowserBase 
    {
        public TabBrowserSetup()
        {
            SelectBackground = new RelayCommand(SelectBackground_Execute);
            RemoveBackground = new RelayCommand(RemoveBackground_Execute);
            FilterHistory = new RelayCommand(FilterHistory_Execute);
            ClearHistory = new RelayCommand(ClearHistory_Execute);
            DeleteSelectedHistoryEntries = new RelayCommand(DeleteSelectedHistoryEntries_Execute);

            InitializeComponent();

            if (Content is Border border) Screenshot = new VisualBrush(border);
            else Screenshot = Brushes.WhiteSmoke;

            //set title để hiện lên thumbnail của taskbar window10
            SetResourceReference(TitleProperty, "SetupTab.Title");
        }

        #region Properties
        public static TabBrowserSetup? Default { get; set; } 
        #endregion

        #region Dependency properties
        private static void SearchEngineChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Settings.Current.SearchEngine = (SearchEngine)e.NewValue;
            var info = typeof(SearchEngine).GetField(e.NewValue.ToString());
            var description = info?.GetCustomAttribute<DescriptionAttribute>();
            Settings.Current.SearchAddress = description != null ? description.Description : "";
        }
        private static void PrimaryColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string primary = e.NewValue.ToString();
            Settings.Current.PrimaryColor = primary;

            string darkKey = primary + ".Dark";
            string lightKey = primary + ".Light";
            Application.Current.Resources["Primary.Dark"] = Application.Current.Resources[darkKey];
            Application.Current.Resources["Primary.Light"] = Application.Current.Resources[lightKey];

            BundledTheme theme = (BundledTheme)Application.Current.Resources.MergedDictionaries[1];
            theme.PrimaryColor = (PrimaryColor)Enum.Parse(typeof(PrimaryColor), primary);
        }
        private static void SecondaryColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string secondary = e.NewValue.ToString();
            Settings.Current.SecondaryColor = secondary;

            string darkKey = secondary + ".Dark";
            string lightKey = secondary + ".Light";
            Application.Current.Resources["Secondary.Dark"] = Application.Current.Resources[darkKey];
            Application.Current.Resources["Secondary.Light"] = Application.Current.Resources[lightKey];

            BundledTheme theme = (BundledTheme)Application.Current.Resources.MergedDictionaries[1];
            if(secondary != "BlueGrey") theme.SecondaryColor = (SecondaryColor)Enum.Parse(typeof(SecondaryColor), secondary);
        }
        private static void JavaScriptEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Settings.Current.IsJavaScriptEnabled = (bool)e.NewValue;
        }
        private static void ShowErrorTimeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Settings.Current.ShowErrorTime = Convert.ToInt32(e.NewValue);
        }
        private static void LanguageChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var language = (DisplayLanguage)e.NewValue;
            Settings.Current.Language = language;

            var info = typeof(DisplayLanguage).GetField(language.ToString());
            var description = info?.GetCustomAttribute<DescriptionAttribute>();
            if (description != null)
            {
                var resource = new ResourceDictionary { Source = new Uri(description.Description, UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries[0] = resource;
            }
        }
        private static void CanLookHighLightTextUpInTheDictionaryChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Settings.Current.CanLookHighLightTextUpInTheDictionary = (bool)e.NewValue;
        }
        private static void BackgroundPathChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string filePath = (string)e.NewValue;
            Settings.Current.BackgroundPath = filePath; 
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Settings.Current.IsBackgroundValid = false;
                Application.Current.Resources["BlankTab.Background"] = new ImageBrush();
            }
            else
            {
                Settings.Current.IsBackgroundValid = true;
                Application.Current.Resources["BlankTab.Background"] = new ImageBrush(new BitmapImage(new Uri(filePath)));
            }
        }

        public static readonly DependencyProperty SearchEngineProperty = DependencyProperty.Register("SearchEngine", typeof(SearchEngine), typeof(TabBrowserSetup), new PropertyMetadata(SearchEngine.Google, SearchEngineChangedCallback));
        public static readonly DependencyProperty PrimaryColorProperty = DependencyProperty.Register("PrimaryColor", typeof(string), typeof(TabBrowserSetup), new PropertyMetadata("DeepPurple", PrimaryColorChangedCallback));
        public static readonly DependencyProperty SecondaryColorProperty = DependencyProperty.Register("SecondaryColor", typeof(string), typeof(TabBrowserSetup), new PropertyMetadata("Yellow", SecondaryColorChangedCallback));
        public static readonly DependencyProperty TabItemWidthProperty = DependencyProperty.Register("TabItemWidth", typeof(double), typeof(TabBrowserSetup), new PropertyMetadata(250.0));
        public static readonly DependencyProperty NewTabDefaultAddressProperty = DependencyProperty.Register("NewTabDefaultAddress", typeof(string), typeof(TabBrowserSetup), new PropertyMetadata("www.google.com"));
        public static readonly DependencyProperty IsJavaScriptEnabledProperty = DependencyProperty.Register("IsJavaScriptEnabled", typeof(bool), typeof(TabBrowserSetup), new PropertyMetadata(true, JavaScriptEnabledChangedCallback));
        public static readonly DependencyProperty ShowErrorTimeProperty = DependencyProperty.Register("ShowErrorTime", typeof(double), typeof(TabBrowserSetup), new PropertyMetadata(10.0, ShowErrorTimeChangedCallback));
        public static readonly DependencyProperty DisplayLanguageProperty = DependencyProperty.Register("DisplayLanguage", typeof(DisplayLanguage), typeof(TabBrowserSetup), new PropertyMetadata(DisplayLanguage.Vietnamese, LanguageChangedCallback));
        public static readonly DependencyProperty CanLookHighLightTextUpInTheDictionaryProperty = DependencyProperty.Register("CanLookHighLightTextUpInTheDictionary", typeof(bool), typeof(TabBrowserSetup), new PropertyMetadata(true, CanLookHighLightTextUpInTheDictionaryChangedCallback));
        public static readonly DependencyProperty BackgroundPathProperty = DependencyProperty.Register("BackgroundPath", typeof(string), typeof(TabBrowserSetup), new PropertyMetadata(string.Empty, BackgroundPathChangedCallback));

        public static readonly DependencyProperty TextFilterProperty = DependencyProperty.Register("TextFilter", typeof(string), typeof(TabBrowserSetup), new PropertyMetadata(string.Empty));

        public SearchEngine SearchEngine { get => (SearchEngine)GetValue(SearchEngineProperty); set => SetValue(SearchEngineProperty, value); }
        public string PrimaryColor { get => (string)GetValue(PrimaryColorProperty); set => SetValue(PrimaryColorProperty, value); }
        public string SecondaryColor { get => (string)GetValue(SecondaryColorProperty); set => SetValue(SecondaryColorProperty, value); }
        public double TabItemWidth { get => (double)GetValue(TabItemWidthProperty); set => SetValue(TabItemWidthProperty, value); }
        public string NewTabDefaultAddress { get => (string)GetValue(NewTabDefaultAddressProperty); set => SetValue(NewTabDefaultAddressProperty, value); }
        public bool IsJavaScriptEnabled { get => (bool)GetValue(IsJavaScriptEnabledProperty); set => SetValue(IsJavaScriptEnabledProperty, value); }
        public double ShowErrorTime { get => (double)GetValue(ShowErrorTimeProperty); set => SetValue(ShowErrorTimeProperty, value); }
        public DisplayLanguage DisplayLanguage { get => (DisplayLanguage)GetValue(DisplayLanguageProperty); set => SetValue(DisplayLanguageProperty, value); }
        public bool CanLookHighLightTextUpInTheDictionary { get => (bool)GetValue(CanLookHighLightTextUpInTheDictionaryProperty); set => SetValue(CanLookHighLightTextUpInTheDictionaryProperty, value); }
        public string BackgroundPath { get => (string)GetValue(BackgroundPathProperty); set => SetValue(BackgroundPathProperty, value); }

        public string TextFilter { get => (string)GetValue(TextFilterProperty); set => SetValue(TextFilterProperty, value); }
        #endregion

        #region Methods
        /// <summary>
        /// Property trong XAML binding đến property trong code-behind. <br/>
        /// Property trong code-behind gán lại giá trị cho Settings.Current...
        /// </summary>
        /// <param name="settings"></param>
        public void InitializeSettings(Settings settings)
        {
            SearchEngine = settings.SearchEngine;
            PrimaryColor = settings.PrimaryColor;
            SecondaryColor = settings.SecondaryColor;
            TabItemWidth = settings.TabItemWidth;
            NewTabDefaultAddress = settings.NewTabDefaultAddress; 
            IsJavaScriptEnabled = settings.IsJavaScriptEnabled;
            ShowErrorTime = settings.ShowErrorTime;
            DisplayLanguage = settings.Language;
            CanLookHighLightTextUpInTheDictionary = settings.CanLookHighLightTextUpInTheDictionary;
            BackgroundPath = settings.BackgroundPath;
        }
 

        #endregion

        #region Commands
        public ICommand SelectBackground { get; }
        public ICommand RemoveBackground { get; }
        public ICommand FilterHistory { get; }
        public ICommand RefreshView { get; } = new RelayCommand(obj => { if (obj is CollectionViewSource viewSource) viewSource.View.Refresh(); });
        public ICommand ClearHistory { get; }
        public ICommand DeleteHistoryEntry { get; } = new RelayCommand(obj => { if (obj is HistoryEntry entry) BrowsingData.History.Remove(entry); });
        public ICommand DeleteSelectedHistoryEntries { get; }
        public static ICommand ShowInFolder { get; } = new RelayCommand(obj =>
        {
            if (obj is DownloadEntry entry && File.Exists(entry.FullPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = "explorer",
                    Arguments = $"/e, /select, \"{entry.FullPath}\""
                });
            }
            else
            {
                DialogBox.Show("File not found.", "ERROR");
            }
        });
        public static ICommand PauseDownload { get; } = new RelayCommand(obj =>
        {
            if (obj is DownloadEntry entry)
            {
                if (entry.IsPaused) entry.Resume();
                else entry.Pause();
            }
        }, obj => obj is DownloadEntry entry && entry.Callback is not null);
        public static ICommand CancelDownload { get; } = new RelayCommand(obj => { if (obj is DownloadEntry entry) entry.Cancel(); }, obj => obj is DownloadEntry entry && entry.Callback is not null);
        public static ICommand DeleteDownload { get; } = new RelayCommand(obj => 
        { 
            if (obj is DownloadEntry entry)
            { 
                entry.Cancel();
                BrowsingData.Downloads.Remove(entry);
            }
        });
        #endregion

        #region Command_Execute
        private void SelectBackground_Execute(object? obj)
        { 
            var dialog = new OpenFileDialog(); 
            dialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg"; 
            if (dialog.ShowDialog() == true)
            {
                BackgroundPath = dialog.FileName;
            }
        }
        private void RemoveBackground_Execute(object? obj)
        {
            BackgroundPath = string.Empty;
        }
        private void FilterHistory_Execute(object? obj)
        {
            if (obj is FilterEventArgs eventArgs)
            {
                if (eventArgs.Item is HistoryEntry entry && entry.Url.Contains(TextFilter)) eventArgs.Accepted = true;
                else eventArgs.Accepted = false;
            }
        }
        private void ClearHistory_Execute(object? obj)
        {
            if (DialogBox.Show(DialogBoxParams.ClearHistoryMessage, DialogBoxParams.ClearHistoryTitle, DialogBoxButton.YesNo, DialogBoxIcon.Warning) == DialogBoxResult.Yes)
            {
                var count = BrowsingData.History.Count;
                SystemSounds.Hand.Play();
                BrowsingData.History.Clear();
                TextBlock textBlock = new TextBlock();
                textBlock.Inlines.Add(new Run { Text = $"{count} ", Foreground = Brushes.Crimson });
                textBlock.Inlines.Add(new Run { Text = DialogBoxParams.ClearHistoryCompleteMessage });
                DialogBox.Show(textBlock, DialogBoxParams.ClearHistoryCompleteTitle, DialogBoxButton.OK, DialogBoxIcon.Information);
            }
        }
        private void DeleteSelectedHistoryEntries_Execute(object? obj)
        {
            var temp = new List<HistoryEntry>(BrowsingData.History);
            foreach (var item in temp)
            {
                if (item.IsMarked) BrowsingData.History.Remove(item);
            }
        }
        #endregion
    }
}
