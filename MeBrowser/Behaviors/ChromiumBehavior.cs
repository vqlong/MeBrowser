using CefSharp;
using CefSharp.Wpf;
using MeBrowser.ViewModel;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MeBrowser.Behaviors
{
    /// <summary>
    /// Hiện tại không dùng.
    /// </summary>
    public class ChromiumBehavior : Behavior<ChromiumWebBrowser>
    {
        public static readonly DependencyProperty HoverLinkProperty = DependencyProperty.Register("HoverLink", typeof(string), typeof(ChromiumBehavior), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(ChromiumBehavior), new PropertyMetadata(false));
        public static readonly DependencyProperty LoadingAddressProperty = DependencyProperty.Register("LoadingAddress", typeof(string), typeof(ChromiumBehavior), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty SearchAddressProperty = DependencyProperty.Register("SearchAddress", typeof(string), typeof(ChromiumBehavior), new PropertyMetadata(string.Empty));

        public string HoverLink
        {
            get { return (string)GetValue(HoverLinkProperty); }
            set { SetValue(HoverLinkProperty, value); }
        }
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }
        //link đang gõ trên UrlBox, có thể chưa truy cập thành công
        public string LoadingAddress
        {
            get { return (string)GetValue(LoadingAddressProperty); }
            set { SetValue(LoadingAddressProperty, value); }
        }
        //link search của google, bing...
        public string SearchAddress
        {
            get { return (string)GetValue(SearchAddressProperty); }
            set { SetValue(SearchAddressProperty, value); }
        }
        private void OnLoadError(LoadErrorEventArgs e)
        {
            //if(e.ErrorCode == CefErrorCode.NameNotResolved)
            //{
            //    AssociatedObject.Load(SearchAddress + LoadingAddress);
            //}
            //else
            //{
            //    //Dispatcher.BeginInvoke(() =>
            //    //{
            //    //    //DialogBox.Show($"Không thể mở địa chỉ này:\n" +
            //    //    //            $"Code:\t{e.ErrorCode}\n" +
            //    //    //            $"Url:\t{(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}",
            //    //    //            e.ErrorText);

            //    //    //Debug.WriteLine($"Không thể mở địa chỉ này:\n" + $"Code:\t{e.ErrorCode}\n" + $"Url:\t{(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}");
            //    //});

            //    //Thread thread = new Thread(() =>
            //    //{
            //    //    //DialogBox.Show($"Không thể mở địa chỉ này:\n" +
            //    //    //            $"Code:\t{e.ErrorCode}\n" +
            //    //    //            $"Url:\t{(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}",
            //    //    //            e.ErrorText);

            //    //    MessageBox.Show($"Không thể mở địa chỉ này:\n" +
            //    //                $"Code:\t{e.ErrorCode}\n" +
            //    //                $"Url:\t{(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}",
            //    //                e.ErrorText,
            //    //                MessageBoxButton.OK,
            //    //                MessageBoxImage.Error);
            //    //});
            //    //thread.SetApartmentState(ApartmentState.STA);
            //    //thread.IsBackground = true;
            //    //thread.Start();

            //    //chuyên trị các thể loại đơ window
            //    DialogBox.ShowError($"Không thể mở địa chỉ này:\n" +
            //                $"ErrorCode: {e.ErrorCode}\n" +
            //                $"FailedUrl: {(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}",
            //                e.ErrorText);

            //}

            HoverLink = e.ErrorText + $" {(e.FailedUrl.Length < 100 ? e.FailedUrl : e.FailedUrl.Substring(0, 100) + "...")}";
        }
        protected override void OnDetaching()
        {
            AssociatedObject.StatusMessage -= OnStatusMessageChanged;
            AssociatedObject.LoadingStateChanged -= AssociatedObject_LoadingStateChanged;
            AssociatedObject.LoadError -= AssociatedObject_LoadError;
        }
        protected override void OnAttached()
        {
            AssociatedObject.StatusMessage += OnStatusMessageChanged;
            AssociatedObject.LoadingStateChanged += AssociatedObject_LoadingStateChanged;
            AssociatedObject.LoadError += AssociatedObject_LoadError;
        }

        private void AssociatedObject_LoadError(object? sender, LoadErrorEventArgs e)
        {
            Dispatcher.BeginInvoke(() => OnLoadError(e), System.Windows.Threading.DispatcherPriority.Input);
        }
        private void OnStatusMessageChanged(object? sender, StatusMessageEventArgs e)
        {
            //event StatusMessage này được fire ở UI thread của CEF, không phải UI thread của application
            //dùng BeginInvoke để mò vào UI thread của application
            //tức là cả browser và browserbehavior cùng cái HoverLink này thuộc UI thread của application
            //nhưng cái event StatusMessage và cái hàm này lại được gọi từ thread khác (UI của CEF)
            if (sender is ChromiumWebBrowser chromiumWebBrowser)
                chromiumWebBrowser.Dispatcher.BeginInvoke((Action)(() => HoverLink = e.Value));
        }
        private void AssociatedObject_LoadingStateChanged(object? sender, LoadingStateChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => SetCurrentValue(IsLoadingProperty, e.IsLoading)); 
        }
    }
}
