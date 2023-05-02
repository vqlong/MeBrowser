using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MeBrowser.Model
{
    public class Settings : BindableBase
    {
        public SearchEngine SearchEngine { get; set; } = SearchEngine.Google;
        public string PrimaryColor { get; set; } = "DeepPurple";
        public string SecondaryColor { get; set; } = "Yellow";
        /// <summary>
        /// Hiện tại không tạo UI cho việc thay đổi TabItemWidth, app luôn khởi động với TabItemWidth mặc định.
        /// </summary>
        public double TabItemWidth { get; set; } = 250; 
        public string NewTabDefaultAddress { get; set; } = "www.google.com";
        /// <summary>
        /// user thay đổi SearchEngine trên UI, app sẽ thay đổi SearchAddress theo SearchEngine được chọn.
        /// </summary>
        public string SearchAddress { get; set; } = "https://www.google.com/search?q="; 
        public bool IsJavaScriptEnabled { get; set; } = true;
        public int ShowErrorTime { get; set; } = 0;
        public DisplayLanguage Language { get; set; } = DisplayLanguage.Vietnamese;
        public bool CanLookHighLightTextUpInTheDictionary { get; set; } = true;
        public string BackgroundPath { get; set; } = string.Empty;
        public bool IsBackgroundValid { get => GetProperty<bool>(); set => SetProperty(value); }

        public const string GOOGLE_SEARCHADDRESS = "https://www.google.com/search?q=";
        public const string SAVES_DIRECTORYPATH = "/Saves/";
        public const string SCREENSHOT_DIRECTORYPATH = "/Saves/Screenshots/";
        public const string IMAGEDOWNLOAD_DIRECTORYPATH = "/Saves/Pictures/";
        public const string FILEDOWNLOAD_DIRECTORYPATH = "/Saves/Downloads/";
        public const string SETTINGS_FILEPATH = "Saves/Settings.xml";
        public const string HISTORY_FILEPATH = "Saves/History.xml";
        public const string DOWNLOADS_FILEPATH = "Saves/Downloads.xml";
        public const string ADS_FILEPATH = "Saves/Ads.txt";
        public const string SCRIPTS_FILEPATH = "Saves/Scripts.xml";
        public const string REMOTEDEBUGGER_ADDRESS = "http://localhost:6969";
        public const int REMOTEDEBUGGER_PORT = 6969;

        /// <summary>
        /// Settings mặc định.
        /// </summary>
        public static Settings Default { get; } = new Settings();
        /// <summary>
        /// Settings khi load lên từ xml sẽ nhét vào đây, khi tab setup cập nhật giá trị mới của các setting cũng gán ngay vào đây.
        /// </summary>
        public static Settings Current { get; set; }

        /// <summary>
        /// Đặt giá trị cho Settings.Current và các setting ban đầu cho app.
        /// </summary>
        /// <param name="settings"></param>
        public static void InitializeSettings(Settings settings)
        {
            Current = settings;

            //Khởi tạo các mục setting khi tab setup còn chưa được open
            //Khởi tạo color
            string primary = Current.PrimaryColor;            
            Application.Current.Resources["Primary.Dark"] = Application.Current.Resources[primary + ".Dark"];
            Application.Current.Resources["Primary.Light"] = Application.Current.Resources[primary + ".Light"];
            BundledTheme theme = (BundledTheme)Application.Current.Resources.MergedDictionaries[1];
            theme.PrimaryColor = (PrimaryColor)Enum.Parse(typeof(PrimaryColor), primary);

            string secondary = Current.SecondaryColor;
            Application.Current.Resources["Secondary.Dark"] = Application.Current.Resources[secondary + ".Dark"];
            Application.Current.Resources["Secondary.Light"] = Application.Current.Resources[secondary + ".Light"];
            if (secondary != "BlueGrey") theme.SecondaryColor = (SecondaryColor)Enum.Parse(typeof(SecondaryColor), secondary);

            //Khởi tạo language
            var language = Current.Language;

            var info = typeof(DisplayLanguage).GetField(language.ToString());
            var description = info?.GetCustomAttribute<DescriptionAttribute>();
            if (description != null)
            {
                var resource = new ResourceDictionary { Source = new Uri(description.Description, UriKind.Relative) };
                Application.Current.Resources.MergedDictionaries[0] = resource;
            }

            //Khởi tạo background
            if (string.IsNullOrWhiteSpace(Current.BackgroundPath))
            {
                Current.IsBackgroundValid = false;
                Application.Current.Resources["BlankTab.Background"] = new ImageBrush();
            }
            else
            {
                Current.IsBackgroundValid = true;
                Application.Current.Resources["BlankTab.Background"] = new ImageBrush(new BitmapImage(new Uri(Current.BackgroundPath)));
            }
        }
    }
}
