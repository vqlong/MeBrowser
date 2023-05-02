using MeBrowser.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MeBrowser.Model
{
    public class DialogBoxParams 
    { 
        public static string CloseTitle => (string)Application.Current.Resources["Dialog.Close.Title"];
        public static string CloseMessage => (string)Application.Current.Resources["Dialog.Close.Message"];
        public static string ErrorMessage => (string)Application.Current.Resources["Dialog.Error.Message"];
        public static string ClearHistoryTitle => (string)Application.Current.Resources["Dialog.ClearHistory.Title"];
        public static string ClearHistoryMessage => (string)Application.Current.Resources["Dialog.ClearHistory.Message"];
        public static string ClearHistoryCompleteTitle => (string)Application.Current.Resources["Dialog.ClearHistoryComplete.Title"];
        public static string ClearHistoryCompleteMessage => (string)Application.Current.Resources["Dialog.ClearHistoryComplete.Message"];
        public static string CancelDownloadTitle => (string)Application.Current.Resources["Dialog.CancelDownload.Title"];
        public static string CancelDownloadMessage => (string)Application.Current.Resources["Dialog.CancelDownload.Message"];
        public static string RequestFullScreenTitle => (string)Application.Current.Resources["Dialog.RequestFullScreen.Title"];
        public static string RequestFullScreenMessage => (string)Application.Current.Resources["Dialog.RequestFullScreen.Message"];
        public static string RequestExitFullScreenTitle => (string)Application.Current.Resources["Dialog.RequestExitFullScreen.Title"];
        public static string RequestExitFullScreenMessage => (string)Application.Current.Resources["Dialog.RequestExitFullScreen.Message"];
    }
}
