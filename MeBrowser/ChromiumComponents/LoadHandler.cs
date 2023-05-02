using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.DevTools;
using CefSharp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Controls;
using System.Xml.Linq;
using MeBrowser.Model;

namespace MeBrowser.ChromiumComponents
{
    public class LoadHandler : ILoadHandler
    {
        //Khi browser load 1 địa chỉ nó sẽ gọi liên tục các hàm của class này
        //nếu load bình thường: OnFrameLoadStart => OnFrameLoadEnd
        //nếu load error: OnLoadError => OnFrameLoadEnd
        //dù error hay không main frame luôn được lưu vào list navigation entry
        //nếu mở địa chỉ mới khi đang navigate ở vị trí giữa list, frame mới load sẽ được add vào thay vị trí hiện tại,
        //các frame sau nó sẽ bị xoá hết khỏi list, frame mới này trở thành index cao nhất

        public event EventHandler<FrameLoadEndEventArgs>? FrameLoadEnd;
        public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
        {
            //if (frameLoadEndArgs.Frame.IsValid) Debug.WriteLine(frameLoadEndArgs.Frame.IsMain + "-FrameLoadEnd- " + frameLoadEndArgs.Frame.Url);

            FrameLoadEnd?.Invoke(chromiumWebBrowser, frameLoadEndArgs); 
        }
        public event EventHandler<FrameLoadStartEventArgs>? FrameLoadStart;
        public void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
        {
            //if (frameLoadStartArgs.Frame.IsValid) Debug.WriteLine(frameLoadStartArgs.Frame.IsMain + "-FrameLoadStart- " + frameLoadStartArgs.Frame.Url);

            FrameLoadStart?.Invoke(chromiumWebBrowser, frameLoadStartArgs); 
        }
        public event EventHandler<LoadErrorEventArgs>? LoadError;
        public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
        {
            LoadError?.Invoke(chromiumWebBrowser, loadErrorArgs);
        }
        public event EventHandler<LoadingStateChangedEventArgs>? LoadingStateChanged;
        public void OnLoadingStateChange(IWebBrowser chromiumWebBrowser, LoadingStateChangedEventArgs loadingStateChangedArgs)
        {
            //if (loadingStateChangedArgs.Browser.MainFrame.IsValid) Debug.WriteLine(loadingStateChangedArgs.Browser.MainFrame.IsMain + $"-LoadingState: {loadingStateChangedArgs.IsLoading}- " + loadingStateChangedArgs.Browser.MainFrame.Url);

            AdBlocker.BlockByPage(loadingStateChangedArgs.Browser.MainFrame.Url, chromiumWebBrowser);

            LoadingStateChanged?.Invoke(chromiumWebBrowser, loadingStateChangedArgs);
        }
    }
}
