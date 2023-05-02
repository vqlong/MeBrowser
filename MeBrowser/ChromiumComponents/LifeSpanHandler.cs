using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.ChromiumComponents
{
    public class LifeSpanHandler : ILifeSpanHandler
    {
        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {

        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {

        }

        public event EventHandler<string>? PopupRequest;
        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser? newBrowser)
        {
            PopupRequest?.Invoke(chromiumWebBrowser, targetUrl);

            // Load new URL (when clicking a link with target=_blank) in the same frame (mở ngay tại tab hiện tại)
            //browser.MainFrame.LoadUrl(targetUrl);

            // Set to null for default behaviour. If you return true (cancel popup creation) then his property **MUST** be null, an exception will be thrown otherwise.
            newBrowser = null;

            // By default the popup (browser) is opened in a new native window. If you return true then creation of the popup (browser) is cancelled, no further action will occur.
            // Otherwise return false to allow creation of the popup (browser). 
            return true;
        }
    }
}
