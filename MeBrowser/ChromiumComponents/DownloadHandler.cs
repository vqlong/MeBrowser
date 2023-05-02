using CefSharp;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.ChromiumComponents
{
    public class DownloadHandler : IDownloadHandler
    {
        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            //Return true to proceed with the download or false to cancel the download.
            return true;
        }
        string[] imageExtensions = new string[] { "png", "jpg", "ico", "gif", "bmp", "jpeg" };
        public event EventHandler<DownloadArgs>? BeforeDownload;
        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                var directoryInfo = Directory.CreateDirectory(Environment.CurrentDirectory + Settings.FILEDOWNLOAD_DIRECTORYPATH);
                var fileName = downloadItem.SuggestedFileName.Split(".")[1];
                
                if (downloadItem.MimeType.StartsWith("image") || imageExtensions.Contains(fileName))
                {
                    directoryInfo = Directory.CreateDirectory(Environment.CurrentDirectory + Settings.IMAGEDOWNLOAD_DIRECTORYPATH);
                    fileName = directoryInfo.FullName + "Image_" + DateTime.Now.ToString("HHmmssddMMyy") + "_" + downloadItem.SuggestedFileName;
                }
                else
                {
                    fileName = directoryInfo.FullName + "File_" + DateTime.Now.ToString("HHmmssddMMyy") + "_" + downloadItem.SuggestedFileName;
                }

                using (callback)
                {
                    BeforeDownload?.Invoke(chromiumWebBrowser, new DownloadArgs { Item = downloadItem });
                    callback.Continue(fileName, true);
                }
            }
        }
        public event EventHandler<DownloadArgs>? DownloadUpdated;
        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            DownloadUpdated?.Invoke(chromiumWebBrowser, new DownloadArgs { Item = downloadItem, Callback = callback });
        }
    }
}
