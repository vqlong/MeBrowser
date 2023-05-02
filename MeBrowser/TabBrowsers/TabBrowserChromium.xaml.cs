using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MeBrowser.Model;
using CefSharp;
using MeBrowser.ChromiumComponents;
using System.Windows.Data;
using System.Windows.Input;
using MeBrowser.ViewModel;
using System.IO;
using CefSharp.DevTools;
using CefSharp.DevTools.Page;
using System.Windows.Threading;
using System.Threading;
using MaterialDesignThemes.Wpf;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Net.Http;
using System.Net;
using System.Drawing;
using MeBrowser.Helpers;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Windows.Themes;

namespace MeBrowser.TabBrowsers
{
    public partial class TabBrowserChromium : TabBrowserBase
    {
        public TabBrowserChromium()
        {
            LoadUrl = new RelayCommand(LoadUrl_Execute);
            OpenHistory = new RelayCommand(obj => IsHistoryOpen = true);
            CloseHistory = new RelayCommand(obj => IsHistoryOpen = false);
            OpenNavigation = new RelayCommand(obj => IsNavigationOpen = true);
            CloseNavigation = new RelayCommand(obj => IsNavigationOpen = false);
            PauseDownload = new RelayCommand(PauseDownload_Execute);
            CancelDownload = new RelayCommand(obj => CurrentDownload?.Cancel());
            ShowInFolder = new RelayCommand(ShowInFolder_Execute);
            RefreshHistoryView = new RelayCommand(obj => HistoryView.Refresh());
            KeyPress = new RelayCommand(KeyPress_Execute);
            SelectAll = new RelayCommand(obj => { if (obj is TextBoxBase textBoxBase) textBoxBase.SelectAll(); });
            ClickToLoad = new RelayCommand(obj => IsViewClicked = true);
            OpenDebugger = new RelayCommand(OpenDebugger_Execute);
            CloseDebugger = new RelayCommand(CloseDebugger_Execute);
            ChangeDebuggerWidth = new RelayCommand(ChangeDebuggerWidth_Execute);
            ChangeDebuggerHeight = new RelayCommand(ChangeDebuggerHeight_Execute);
            FindPrevious = new RelayCommand(obj => Chromium.Find(FindText, false, false, true));
            FindNext = new RelayCommand(obj => Chromium.Find(FindText, true, false, true));
            FindStop = new RelayCommand(obj => Chromium.StopFinding(true));
            Translate = new RelayCommand(Translate_Execute);
            TranslatorSwap = new RelayCommand(TranslatorSwap_Execute);
            TranslatorSpeak = new RelayCommand(TranslatorSpeak_Execute);

            NavigationView = new ListCollectionView(NavigationEntries);
            TranslatorVoicesView = new ListCollectionView(TranslatorVoices);

            InitializeComponent();

            Screenshot = new VisualBrush(gridChromium); 
        }

        #region Properties
        public static int CurrentId { get; set; } 
        public int Id { get; } = CurrentId++;
        public ChromiumWebBrowser Chromium { get; } = new ChromiumWebBrowser();
        public ListCollectionView HistoryView { get; } = new ListCollectionView(BrowsingData.History);
        public ListCollectionView NavigationView { get; }
        public Stream? FaviconStream { get; private set; }
        /// <summary>
        /// Đánh dấu khi nhấn up/down lên textbox để thay đổi currentitem của historyview
        /// </summary>
        public bool IsCurrentChangedByUpDown { get; private set; }
        /// <summary>
        /// Đánh dấu khi click lên listview để chọn 1 địa chỉ.
        /// </summary>
        public bool IsViewClicked { get; private set; }  
        /// <summary>
        /// Danh sách tất cả các mục download của tab này.
        /// </summary>
        public List<DownloadEntry> DownloadEntries { get; } = new List<DownloadEntry>();
        /// <summary>
        /// Chứa các url error do nhập từ khoá lên UrlBox để tìm kiếm.
        /// </summary>
        public List<string> ErrorUrls { get; } = new List<string>();
        /// <summary>
        /// Chứa các địa chỉ có thể navigate đến (Back/Forward) trong tab này.
        /// </summary>
        public ObservableCollection<TabNavigationEntry> NavigationEntries { get; } = new ObservableCollection<TabNavigationEntry>();
        public DispatcherTimer AdBlockTimer { get; } = new DispatcherTimer();
        public ListCollectionView TranslatorSourceView { get; } = new ListCollectionView(CultureInfo.GetCultures(CultureTypes.AllCultures));
        public ListCollectionView TranslatorTargetView { get; } = new ListCollectionView(CultureInfo.GetCultures(CultureTypes.AllCultures));
        public ListCollectionView TranslatorVoicesView { get; }
        public ObservableCollection<SpeechSynthesisVoice> TranslatorVoices { get; } = new ObservableCollection<SpeechSynthesisVoice>();

        #endregion

        #region Dependency properties
        public static readonly DependencyProperty FaviconSourceProperty = DependencyProperty.Register("FaviconSource", typeof(ImageSource), typeof(TabBrowserBase), new PropertyMetadata(new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/icon.png", UriKind.Absolute))));
        public static readonly DependencyProperty LoadingProgressProperty = DependencyProperty.Register("LoadingProgress", typeof(double), typeof(TabBrowserChromium), new PropertyMetadata(0.0));
        public static readonly DependencyProperty HoverLinkProperty = DependencyProperty.Register("HoverLink", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register("Address", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsHistoryOpenProperty = DependencyProperty.Register("IsHistoryOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty IsNavigationOpenProperty = DependencyProperty.Register("IsNavigationOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty DebuggerProperty = DependencyProperty.Register("Debugger", typeof(ChromiumWebBrowser), typeof(TabBrowserChromium), new PropertyMetadata(null));
        public static readonly DependencyProperty DebuggerWidthProperty = DependencyProperty.Register("DebuggerWidth", typeof(double), typeof(TabBrowserChromium), new PropertyMetadata(0.0));
        public static readonly DependencyProperty DebuggerHeightProperty = DependencyProperty.Register("DebuggerHeight", typeof(double), typeof(TabBrowserChromium), new PropertyMetadata(0.0));
        public static readonly DependencyProperty IsDebuggerOpenProperty = DependencyProperty.Register("IsDebuggerOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty CurrentDownloadProperty = DependencyProperty.Register("CurrentDownload", typeof(DownloadEntry), typeof(TabBrowserChromium), new PropertyMetadata(null));
        public static readonly DependencyProperty IsDownloadBarOpenProperty = DependencyProperty.Register("IsDownloadBarOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty ErrorCaptionProperty = DependencyProperty.Register("ErrorCaption", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ErrorUrlProperty = DependencyProperty.Register("ErrorUrl", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FindTextProperty = DependencyProperty.Register("FindText", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsFindingBarOpenProperty = DependencyProperty.Register("IsFindingBarOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTranslatorOpenProperty = DependencyProperty.Register("IsTranslatorOpen", typeof(bool), typeof(TabBrowserChromium), new PropertyMetadata(false));
        public static readonly DependencyProperty TranslatorSourceTextProperty = DependencyProperty.Register("TranslatorSourceText", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty TranslatorTargetTextProperty = DependencyProperty.Register("TranslatorTargetText", typeof(string), typeof(TabBrowserChromium), new PropertyMetadata(string.Empty));

        public ImageSource FaviconSource { get => (ImageSource)GetValue(FaviconSourceProperty); set => SetValue(FaviconSourceProperty, value); }
        public double LoadingProgress { get => (double)GetValue(LoadingProgressProperty); set => SetValue(LoadingProgressProperty, value); } 
        public string HoverLink { get => (string)GetValue(HoverLinkProperty); set => SetValue(HoverLinkProperty, value); }
        public string Address { get => (string)GetValue(AddressProperty); set => SetValue(AddressProperty, value); }
        public bool IsHistoryOpen { get => (bool)GetValue(IsHistoryOpenProperty); set => SetValue(IsHistoryOpenProperty, value); }
        public bool IsNavigationOpen { get => (bool)GetValue(IsNavigationOpenProperty); set => SetValue(IsNavigationOpenProperty, value); }
        public ChromiumWebBrowser Debugger { get => (ChromiumWebBrowser)GetValue(DebuggerProperty); set => SetValue(DebuggerProperty, value); }
        public double DebuggerWidth { get => (double)GetValue(DebuggerWidthProperty); set => SetValue(DebuggerWidthProperty, value); }
        public double DebuggerHeight { get => (double)GetValue(DebuggerHeightProperty); set => SetValue(DebuggerHeightProperty, value); }
        public bool IsDebuggerOpen { get => (bool)GetValue(IsDebuggerOpenProperty); set => SetValue(IsDebuggerOpenProperty, value); }
        public DownloadEntry CurrentDownload { get => (DownloadEntry)GetValue(CurrentDownloadProperty); set => SetValue(CurrentDownloadProperty, value); }
        public bool IsDownloadBarOpen { get => (bool)GetValue(IsDownloadBarOpenProperty); set => SetValue(IsDownloadBarOpenProperty, value); }
        public bool HasError { get => (bool)GetValue(HasErrorProperty); set => SetValue(HasErrorProperty, value); }
        public string ErrorCaption { get => (string)GetValue(ErrorCaptionProperty); set => SetValue(ErrorCaptionProperty, value); }
        public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); set => SetValue(ErrorMessageProperty, value); }
        public string ErrorUrl { get => (string)GetValue(ErrorUrlProperty); set => SetValue(ErrorUrlProperty, value); }
        public string FindText { get => (string)GetValue(FindTextProperty); set => SetValue(FindTextProperty, value); }
        public bool IsFindingBarOpen { get => (bool)GetValue(IsFindingBarOpenProperty); set => SetValue(IsFindingBarOpenProperty, value); }
        public bool IsTranslatorOpen { get => (bool)GetValue(IsTranslatorOpenProperty); set => SetValue(IsTranslatorOpenProperty, value); }
        public string TranslatorSourceText { get => (string)GetValue(TranslatorSourceTextProperty); set => SetValue(TranslatorSourceTextProperty, value); }
        public string TranslatorTargetText { get => (string)GetValue(TranslatorTargetTextProperty); set => SetValue(TranslatorTargetTextProperty, value); }
        #endregion

        #region Commands

        public ICommand LoadUrl { get; }
        public ICommand OpenHistory { get; }
        public ICommand CloseHistory { get; }
        public ICommand OpenNavigation { get; }
        public ICommand CloseNavigation { get; }
        public ICommand PauseDownload { get; }
        public ICommand CancelDownload { get; }
        public ICommand ShowInFolder { get; }
        public ICommand RefreshHistoryView { get; }
        public ICommand KeyPress { get; }
        public ICommand SelectAll { get; }
        public ICommand ClickToLoad { get; }
        public ICommand OpenDebugger { get; }
        public ICommand CloseDebugger { get; }
        public ICommand ChangeDebuggerWidth { get; }
        public ICommand ChangeDebuggerHeight { get; }
        public ICommand FindPrevious { get; }
        public ICommand FindNext { get; }
        public ICommand FindStop { get; }
        public ICommand Translate { get; }
        public ICommand TranslatorSwap { get; }
        public ICommand TranslatorSpeak { get; }
        #endregion

        #region Command_Execute

        private void LoadUrl_Execute(object? obj)
        {
            //var url = Address;
            //if (!url.StartsWith("http://") || !url.StartsWith("https://")) url = "https://" + url;
            //bool isUrlValid = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && uri is Uri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "data" || uri.Scheme == "about");
 
            if (/*Address.StartsWith("about:") || Address.StartsWith("data:") || */Address.IsUrl())
            {
                Chromium.Load(Address);
            }
            else
            {
                Chromium.Load(Settings.Current.SearchAddress + Address);
            }

        }
        private void ShowInFolder_Execute(object? obj)
        {
            if(obj is string filePath && File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = "explorer",
                    Arguments = $"/e, /select, \"{filePath}\""
                });
            }
            else
            {
                DialogBox.Show("File not found.", "ERROR");
            }
        }
        private void KeyPress_Execute(object? obj)
        {
            //Wpf đã handle hết các event của phím điều hướng, chỉ còn lại preview xài tạm :((

            if (obj is Key.Enter)
            { 
                IsHistoryOpen = false;
                LoadUrl_Execute(null);

                //focus sang thằng khác để close popup của textBoxUrl
                //Chromium?.Focus();

                return;
            }
            //Nhấn up/down trên UrlBox sẽ thay đổi CurrentItem của HistoryView
            if ((obj is Key.Up || obj is Key.Down) && HistoryView.CurrentPosition == -1)
            {
                HistoryView.MoveCurrentToPosition(0);
                return;
            }
            if (obj is Key.Up && HistoryView.CurrentPosition > -1)
            {
                IsCurrentChangedByUpDown = true;
                HistoryView.MoveCurrentToPrevious();
                return;
            }
            if (obj is Key.Down && HistoryView.CurrentPosition < HistoryView.Count - 1)
            {
                IsCurrentChangedByUpDown = true;
                HistoryView.MoveCurrentToNext();
                return;
            }
            //search những từ đang nhập vào UrlBox
            if(obj is not Key.Up and not Key.Down)
            {
                HistoryView.Refresh();
            }
        }
        private void OpenDebugger_Execute(object? obj)
        {
            Debugger = new ChromiumWebBrowser(Settings.REMOTEDEBUGGER_ADDRESS);
            DebuggerWidth = TabMain.ActualWidth / 2;
            DebuggerHeight = TabMain.ActualHeight / 2;
            IsDebuggerOpen = true;
        }
        private void CloseDebugger_Execute(object? obj)
        {
            Debugger.Dispose();
            IsDebuggerOpen = false; 
        }
        private void ChangeDebuggerWidth_Execute(object? obj)
        {
            if (obj is double widthChange) DebuggerWidth -= widthChange;
        }
        private void ChangeDebuggerHeight_Execute(object? obj)
        {
            if (obj is double heightChange) DebuggerHeight -= heightChange;
        }
        private void PauseDownload_Execute(object? obj)
        {
            if (CurrentDownload is DownloadEntry entry)
            {
                if (entry.IsPaused) entry.Resume();
                else entry.Pause();
            }
        }
        private void Translate_Execute(object? obj)
        {
            TranslateText(TranslatorSourceText);
        }
        private void TranslatorSwap_Execute(object? obj)
        {
            var temp = TranslatorSourceView.CurrentItem;
            TranslatorSourceView.MoveCurrentTo(TranslatorTargetView.CurrentItem);
            TranslatorTargetView.MoveCurrentTo(temp);
            SetCurrentValue(TranslatorSourceTextProperty, TranslatorTargetText);
            Translate.Execute(null);
        }
        private void TranslatorSpeak_Execute(object? obj)
        {
            string script = @"(function(text, voiceName)
            {
                const utterThis = new SpeechSynthesisUtterance(text);
                const voices = window.speechSynthesis.getVoices();
  	            for (let i = 0; i < voices.length; i++) 
                {
                    if (voices[i].name === voiceName) 
    	            {
      		            utterThis.voice = voices[i];
    	            }
  	            }
                window.speechSynthesis.speak(utterThis);
            })('" + TranslatorSourceText + "','" + ((SpeechSynthesisVoice)TranslatorVoicesView.CurrentItem).Name + "'); ";

            Chromium.ExecuteScriptAsync(script);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Huỷ bỏ tất cả download của tab này.
        /// </summary>
        public void CancelAllDownloadsBeforeClose() => DownloadEntries.ForEach(entry => entry.CancelBeforeClose());
        /// <summary>
        /// Khởi tạo các event cho chromium và load ngay địa chỉ được truyền vào.
        /// </summary>
        /// <param name="address"></param>
        public void InitializeChromium(string address)
        {

            HistoryView.Filter = obj =>
            {
                if (obj is HistoryEntry entry)
                    return entry.Url.Contains(Address ??= string.Empty, StringComparison.CurrentCultureIgnoreCase);
                return false;
            };
            HistoryView.SortDescriptions.Add(new System.ComponentModel.SortDescription("Time", System.ComponentModel.ListSortDirection.Descending));
            HistoryView.CurrentChanged += (s, e) =>
            {
                if (HistoryView.CurrentItem is HistoryEntry entry)
                {
                    if (IsCurrentChangedByUpDown)
                    {
                        IsCurrentChangedByUpDown = false;
                        //Truyền url của SelectedItem cho textboxurl
                        SetCurrentValue(AddressProperty, entry.Url);
                    }
                    
                    //Nếu SelectedItem thay đổi do bị click
                    if (IsViewClicked)
                    {
                        IsViewClicked = false;
                        IsHistoryOpen = false;
                        Chromium.Load(entry.Url); 
                    }
                }
            };

            NavigationView.CurrentChanged += (s, e) =>
            {
                if (NavigationView.CurrentItem is TabNavigationEntry entry)
                {
                    //Nếu SelectedItem thay đổi do bị click
                    if (IsViewClicked)
                    {
                        IsViewClicked = false;
                        IsNavigationOpen = false;
                        Chromium.Load(entry.Url);
                    }
                }
            };

            if (Chromium.BrowserSettings != null)
            {
                if (Settings.Current.IsJavaScriptEnabled) Chromium.BrowserSettings.Javascript = CefState.Enabled;
                else Chromium.BrowserSettings.Javascript = CefState.Disabled;
            }

            var contextMenu = new MeContextMenuHandler();
            //Với ContextMenu mặc định của CefSharp các event này được fire trên 1 CEF UI Thread
            //Sau khi custom lại ContextMenu các event này đã được fire trên Application UI Thread
            contextMenu.OpenLinkInNewTab += (s, e) => Dispatcher.BeginInvoke(() => OpenNewTab.Execute(e));
            contextMenu.SearchGoogle += (s, e) => Dispatcher.BeginInvoke(() => OpenNewTab.Execute(new OpenNewTabParams(Settings.GOOGLE_SEARCHADDRESS + e)));
            contextMenu.ScreenshotRequest += /*async*/ (s, e) =>
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + Settings.SCREENSHOT_DIRECTORYPATH);
                string filePath = Environment.CurrentDirectory + Settings.SCREENSHOT_DIRECTORYPATH + DateTime.Now.ToString("HHmmssddMMyyyy") + ".png";

                //Dùng đồ sẵn có của wpf
                var rec = new Rect(Chromium.RenderSize);
                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)rec.Width, (int)rec.Height + 1, 96, 96, PixelFormats.Default);
                bitmap.Render(Chromium);

                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));
                using FileStream stream = new(filePath, FileMode.Create);
                pngEncoder.Save(stream);

                ////Dùng api của cefsharp
                //using DevToolsClient devToolsClient = Chromium.GetDevToolsClient(); 
                //var layoutMetricsResponse = await devToolsClient.Page.GetLayoutMetricsAsync();
                //var contentSize = layoutMetricsResponse.ContentSize;

                //var viewPort = new Viewport()
                //{
                //    Height = contentSize.Height,
                //    Width = contentSize.Width,
                //    X = 0,
                //    Y = 0,
                //    Scale = 1
                //}; 
                //var result = await devToolsClient.Page.CaptureScreenshotAsync(clip: viewPort, fromSurface: true, captureBeyondViewport: true);

                //File.WriteAllBytes(filePath, result.Data);
            };
            contextMenu.SaveImageAs += (s, e) =>
            {
                Chromium.StartDownload(e);

                //var directoryInfo = Directory.CreateDirectory(Environment.CurrentDirectory + IMAGEDOWNLOAD_DIRECTORYPATH);
                //string fileName = DateTime.Now.ToString("HHmmssddMMyyyy");

                //var dialog = new SaveFileDialog();
                //dialog.InitialDirectory = directoryInfo.FullName;//"Saves/Pictures/";
                //dialog.Title = "Lưu hình ảnh dưới dạng...";
                //dialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|GIF Image|*.gif|ICON Image|*.ico|BMP Image|*.bmp|All files|*.*";
                //dialog.FileName = fileName; 
                //if (dialog.ShowDialog() == true)
                //{
                //    WebClient client = new WebClient();//HttpClient
                //    Stream stream = client.OpenRead(e);
                //    Bitmap bitmap = new Bitmap(stream);

                //    if (bitmap != null)
                //    {
                //        ImageFormat format = ImageFormat.Png;
                //        if (dialog.FileName.EndsWith(".jpg")) format = ImageFormat.Jpeg;
                //        if (dialog.FileName.EndsWith(".gif")) format = ImageFormat.Gif;
                //        if (dialog.FileName.EndsWith(".ico")) format = ImageFormat.Icon;
                //        if (dialog.FileName.EndsWith(".bmp")) format = ImageFormat.Bmp;

                //        bitmap.Save(dialog.FileName, format);
                //    }
                //}
            };
            contextMenu.DebuggerRequest += (s, e) => Dispatcher.BeginInvoke(() => OpenDebugger.Execute(null));
            contextMenu.FindOnPage += (s, e) => Dispatcher.BeginInvoke(() =>
            {
                SetCurrentValue(FindTextProperty, e);
                SetCurrentValue(IsFindingBarOpenProperty, true);
            });
            contextMenu.Translate += (s, e) => Dispatcher.BeginInvoke(() =>
            {
                SetCurrentValue(TranslatorSourceTextProperty, e);
                SetCurrentValue(IsTranslatorOpenProperty, false);
                SetCurrentValue(IsTranslatorOpenProperty, true);
                Translate.Execute(null);
            });
            Chromium.MenuHandler = contextMenu;

            LifeSpanHandler lifeSpanHandler = new LifeSpanHandler();
            lifeSpanHandler.PopupRequest += (s, e) => Dispatcher.BeginInvoke(() => OpenNewTab.Execute(new OpenNewTabParams(e)));
            Chromium.LifeSpanHandler = lifeSpanHandler;

            FindHandler findHandler = new FindHandler();
            Chromium.FindHandler = findHandler;

            DisplayHandler displayHandler = new DisplayHandler();
            displayHandler.AddressChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (e.Address.Contains("youtube.com")) AdBlockTimer.Start();
                    else AdBlockTimer.Stop();

                    SetCurrentValue(AddressProperty, e.Address);

                    if (e.Address == "about:blank") ClearValue(FaviconSourceProperty);
                    else BrowsingData.AddHistoryEntry(e.Address);
                });

                Dispatcher.BeginInvoke(async () =>
                {
                    //Đồng bộ NavigationEntries của tab này với list NavigationEntry hiện tại của chromium
                    using DevToolsClient devToolsClient = ((IWebBrowser)s).GetDevToolsClient();
                    PageClient pageClient = devToolsClient.Page;
                    var response = await pageClient.GetNavigationHistoryAsync();
                    var entries = response.Entries;

                    var navigationEntryIds = entries.Select(ne => ne.Id);
                    var tabNavigationEntryIds = NavigationEntries.Select(tne => tne.Id);
                    foreach (var id in tabNavigationEntryIds)
                    {
                        //xoá những thằng thừa
                        if (!navigationEntryIds.Contains(id))
                        {
                            NavigationEntries.RemoveWith(tne => tne.Id == id);
                        }
                    }

                    foreach (var id in navigationEntryIds)
                    {
                        //thêm những thằng thiếu
                        if (!tabNavigationEntryIds.Contains(id))
                        {
                            var entry = entries.First(ne => ne.Id == id);
                            TabNavigationEntry tabNavigationEntry = new TabNavigationEntry { Id = entry.Id, Url = entry.Url };
                            if (entry.Url == "about:blank")
                            {
                                tabNavigationEntry.Title = (string)Application.Current.Resources["BlankTab.Title"];
                                tabNavigationEntry.FaviconUrl = "pack://siteoforigin:,,,/Images/icon.png";
                            }
                            else
                            {
                                tabNavigationEntry.Title = entry.Title; 
                            }
                            NavigationEntries.Add(tabNavigationEntry);
                        }
                    }

                    NavigationView.MoveCurrentToPosition(response.CurrentIndex);
                });
            };
            displayHandler.FullscreenChanged += (s, e) => Dispatcher.BeginInvoke(() =>
            {
                if (e == true && Application.Current.MainWindow.WindowState == WindowState.Normal)
                {
                    if (DialogBox.Show(DialogBoxParams.RequestFullScreenMessage, DialogBoxParams.RequestFullScreenTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes) TabMain.Fullscreen.Execute(true);
                }
                if (e == false && Application.Current.MainWindow.WindowState == WindowState.Maximized)
                {
                    if (DialogBox.Show(DialogBoxParams.RequestExitFullScreenMessage, DialogBoxParams.RequestExitFullScreenTitle, DialogBoxButton.YesNo, DialogBoxIcon.Question) == DialogBoxResult.Yes) TabMain.Fullscreen.Execute(false);
                }
            });
            displayHandler.FaviconUrlChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var url = ((IWebBrowser)s).GetBrowser().MainFrame.Url;
                    var entry = NavigationEntries.FirstOrDefault(tne => tne.Url == url);
                    if (entry != null) entry.FaviconUrl = e[^1];
                });

                Dispatcher.BeginInvoke(async () =>
                {
                    //BitmapFrame đã tự Dispose stream sau khi Create
                    //FaviconStream?.Dispose();
                    //FaviconStream = new WebClient().OpenRead(e[0]);
                    //Mấy trang như tuoitre thanhnien lỗi không hiện favicon vì chỉ là file .png đổi tên sang .ico 
                    FaviconStream = await new HttpClient().GetStreamAsync(e[^1]);           
                    FaviconSource = BitmapFrame.Create(FaviconStream);
                });
            };
            displayHandler.TitleChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    SetCurrentValue(TitleProperty, e.Title);

                    var url = ((IWebBrowser)s).GetBrowser().MainFrame.Url;
                    var entry = NavigationEntries.FirstOrDefault(tne => tne.Url == url);
                    if (entry != null) entry.Title = e.Title;
                });
            };
            displayHandler.LoadingProgressChanged += (s, e) => Dispatcher.BeginInvoke(() => LoadingProgress = e);
            displayHandler.StatusMessage += (s, e) => Dispatcher.BeginInvoke(() => HoverLink = e.Value);
            Chromium.DisplayHandler = displayHandler;

            LoadHandler loadHandler = new LoadHandler();
            loadHandler.FrameLoadEnd += (s, e) =>
            {
                if (e.Frame.IsMain)
                {
                    //Tra từ bôi đen bằng từ điển Laban
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (Settings.Current.CanLookHighLightTextUpInTheDictionary && e.Url != "about:blank" && !HasError)
                        {
                            string scriptLabanClick = @"(function()
                            {
                               var lbplugin_event_opt={""extension_enable"":true,""dict_type"":1,""dbclk_event"":{""trigger"":""none"",""enable"":true,""display"":1},""select_event"":{""trigger"":""none"",""enable"":true,""display"":1}};function loadScript(t,e){var n=document.createElement(""script"");n.type=""text/javascript"",n.readyState?n.onreadystatechange=function(){(""loaded""===n.readyState||""complete""===n.readyState)&&(n.onreadystatechange=null,e())}:n.onload=function(){e()},n.src=t,document.getElementsByTagName(""head"")[0].appendChild(n)}setTimeout(function(){null==document.getElementById(""lbdictex_find_popup"")&&loadScript(""https://stc-laban.zdn.vn/dictionary/js/plugin/lbdictplugin.min.js?""+Date.now()%1e4,function(){lbDictPlugin.init(lbplugin_event_opt)})},1e3);
                            })();";
                            Chromium.ExecuteScriptAsync(scriptLabanClick);
                        }
                    });

                    ////Đồng bộ NavigationEntries của tab này với list NavigationEntry hiện tại của chromium
                    //using DevToolsClient devToolsClient = ((IWebBrowser)s).GetDevToolsClient();
                    //PageClient pageClient = devToolsClient.Page;
                    //var response = await pageClient.GetNavigationHistoryAsync();
                    //var entries = response.Entries;
                    //var index = response.CurrentIndex;
                    //var entry = response.Entries[index];
                    //_ = Dispatcher.BeginInvoke(() =>
                    //{
                    //    List<TabNavigationEntry> temp = new List<TabNavigationEntry>(NavigationEntries);
                    //    foreach (var item in temp)
                    //    {
                    //        if (entries.All(e => e.Id != item.Id)) NavigationEntries.Remove(item);
                    //    }

                    //    if (NavigationEntries.All(ne => ne.Id != entry.Id))
                    //    {
                    //        TabNavigationEntry tabNavigationEntry = new TabNavigationEntry { Id = entry.Id, Url = entry.Url };
                    //        if (entry.Url == "about:blank")
                    //        {
                    //            tabNavigationEntry.Title = (string)Application.Current.Resources["BlankTab.Title"];
                    //            tabNavigationEntry.FaviconUrl = "pack://siteoforigin:,,,/Images/icon.png";
                    //        }
                    //        else
                    //        {
                    //            tabNavigationEntry.Title = entry.Title;
                    //        }
                    //        NavigationEntries.Add(tabNavigationEntry);
                    //    }

                    //    NavigationView.MoveCurrentToPosition(index);
                    //});
                }
            };
            loadHandler.FrameLoadStart += (s, e) =>
            {
                if (e.Frame.IsMain)
                {
                    Dispatcher.BeginInvoke(() => HasError = false);
                }
            };
            loadHandler.LoadingStateChanged += (s, e) =>
            {
                Dispatcher.BeginInvoke(() => TabMain.IsLoading = e.IsLoading);
                Dispatcher.BeginInvoke(() =>
                {
                    if (e.IsLoading == false) GetVoicesAsync();
                });
            };
            loadHandler.LoadError += (s, e) =>
            {
                if (e.ErrorCode == CefErrorCode.Aborted) return;
                if (e.Frame.IsMain)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if(Address != "about:blank")
                        {
                            HasError = true;
                            ErrorCaption = e.ErrorText;
                            ErrorMessage = DialogBoxParams.ErrorMessage;
                            ErrorUrl = e.FailedUrl;
                        }
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        DialogBox.ShowError($"{DialogBoxParams.ErrorMessage}\n" +
                                   $"ErrorCode: {e.ErrorCode}\n" +
                                   $"FailedUrl: {(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}",
                                   e.ErrorText,
                                   Settings.Current.ShowErrorTime);
                    });
                }
            };
            Chromium.LoadHandler = loadHandler;

            DownloadHandler downloadHandler = new DownloadHandler();
            downloadHandler.BeforeDownload += (s, e) => Dispatcher.BeginInvoke(() =>
            {
                //Chromium có thể download nhiều item cùng lúc, mỗi khi bắt đầu 1 download nó sẽ gọi các hàm trong DownloadHandler
                //OnBeforeDownload chỉ được gọi 1 lần khi bắt đầu 1 download mới

                //IsValid returns true if this object is valid. Do not call any other methods if this function returns false.
                if (e.Item.IsValid == false) return;

                //Bắt đầu 1 download mới sẽ được gán là CurrentDownload
                //Thằng nào là CurrentDownload mới có slot update lên UI (download status bar)
                var entry = BrowsingData.GetDownloadEntry(Id, e.Item.Id, e.Item.StartTime);
                if (entry != null)
                {
                    //Khi 1 download bị pause quá lâu sẽ không resume được, nó sẽ bắt đầu 1 download mới và gọi lại OnBeforeDownload với Id và StartTime y như cũ
                    CurrentDownload = entry;
                }
                else
                {
                    CurrentDownload = BrowsingData.AddDownloadEntry(e.Item, Id);
                }
                //Danh sách chứa các download của riêng tab này
                DownloadEntries.Add(CurrentDownload);
                //xổ xuống thanh download status
                SetCurrentValue(IsDownloadBarOpenProperty, true); 
            });
            downloadHandler.DownloadUpdated += (s, e) => Dispatcher.BeginInvoke(() =>
            {
                //Với mỗi download đang chạy nó sẽ gọi liên tục OnDownloadUpdated

                //IsValid returns true if this object is valid. Do not call any other methods if this function returns false.
                if (e.Item.IsValid == false) return;

                //CurrentDownload == null tức là đã có thằng bỏ slot (do nó completed hoặc bị cancel) 
                if (CurrentDownload == null && e.Item.IsInProgress)
                {
                    //cướp ngay slot này
                    CurrentDownload = BrowsingData.GetDownloadEntry(Id, e.Item.Id, e.Item.StartTime);
                    SetCurrentValue(IsDownloadBarOpenProperty, true); 
                }

                //item nào cũng được update lên tab setup, nhưng chỉ update khi tab này hoặc tab setup đang được chọn hoặc việc download đã kết thúc
                if (e.Item.IsCancelled || e.Item.IsComplete || (TabMain != null && (TabMain.SelectedItem.Equals(this) || TabMain.SelectedItem.Equals(TabBrowserSetup.Default))))
                {
                    var entry = BrowsingData.GetDownloadEntry(Id, e.Item.Id, e.Item.StartTime);
                    if (entry != null) entry.Update(e);
                }

                //nếu đã xong việc thì nhả slot ra cho thằng khác
                if (CurrentDownload != null && e.Item.Id == CurrentDownload.Id && (e.Item.IsComplete || e.Item.IsCancelled))
                {
                    CurrentDownload = null;
                    SetCurrentValue(IsDownloadBarOpenProperty, false);
                }

            });
            Chromium.DownloadHandler = downloadHandler;

            Chromium.RequestHandler = new MeRequestHandler();

            Chromium.JsDialogHandler = new JsDialogHandler();
            
            AdBlocker.Load();
            AdBlockTimer.Interval = TimeSpan.FromMilliseconds(500);
            AdBlockTimer.Tick += (s, e) => { if (!Chromium.IsDisposed) Chromium.ExecuteScriptAsync(AdBlocker.YoutubeScript); };

            Chromium.Load(address);

            //MenuItem menuItem = new() { Header = "setInterval" };
            //menuItem.Click += (s, e) => Chromium.ExecuteScriptAsync("setInterval(() => {alert(location.hostname);}, 6000);");
            //ContextMenu menu = new ContextMenu();
            //menu.Items.Add(menuItem);
            //this.ContextMenu = menu;
        }
        private async void TranslateText(string text)
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={TranslatorSourceView.CurrentItem}&tl={TranslatorTargetView.CurrentItem}&dt=t&q={/*HttpUtility.UrlEncode(word)*//*Uri.EscapeUriString(word)*/ Uri.EscapeDataString(text)}";

            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(url);
            try
            {
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                TranslatorTargetText = result;
            }
            catch (Exception e)
            {
                DialogBox.Show(e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : ""), "ERROR");
            }

        }
        private async void GetVoicesAsync()
        {
            if (Chromium.CanExecuteJavascriptInMainFrame == false) return;

            string script = @"(function()
            {
   	            const voices = window.speechSynthesis.getVoices(); 
                const arrayVoices = [];
                for (const voice of voices) 
                { 
                    arrayVoices.push(voice.name + '@' + voice.lang);
                }
                return arrayVoices;
            })();";

            var response = await Chromium.EvaluateScriptAsync(script);

            if (response.Success)
            {
                var voices = (IList<object>)response.Result;
                if (voices.Count == 0)
                {
                    GetVoicesAsync();
                }
                else
                {
                    TranslatorVoices.Clear();
                    foreach (var voice in voices)
                    {
                        string[] array = voice.ToString().Split("@");
                        TranslatorVoices.Add(new SpeechSynthesisVoice { Name = array[0], Language = array[1] });
                    }
                    TranslatorVoicesView.MoveCurrentToFirst();
                }
            }
        }
        #endregion
    }
}
