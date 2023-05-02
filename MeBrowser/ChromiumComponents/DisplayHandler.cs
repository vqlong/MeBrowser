using CefSharp;
using CefSharp.Enums;
using CefSharp.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.ChromiumComponents
{
    public class DisplayHandler : IDisplayHandler
    {
        public event EventHandler<AddressChangedEventArgs>? AddressChanged;
        public void OnAddressChanged(IWebBrowser chromiumWebBrowser, AddressChangedEventArgs addressChangedArgs)
        {
            AddressChanged?.Invoke(chromiumWebBrowser, addressChangedArgs);
        }

        public bool OnAutoResize(IWebBrowser chromiumWebBrowser, IBrowser browser, Size newSize)
        {
            //Return true if the resize was handled or false for default handling.
            return false;
        }

        public bool OnConsoleMessage(IWebBrowser chromiumWebBrowser, ConsoleMessageEventArgs consoleMessageArgs)
        {
            //Return true to stop the message from being output to the console.
            return false;
        }

        public bool OnCursorChange(IWebBrowser chromiumWebBrowser, IBrowser browser, nint cursor, CursorType type, CursorInfo customCursorInfo)
        {
            //Return true if the cursor change was handled or false for default handling.
            return false;
        }
        public event EventHandler<IList<string>>? FaviconUrlChanged;
        public void OnFaviconUrlChange(IWebBrowser chromiumWebBrowser, IBrowser browser, IList<string> urls)
        {
            FaviconUrlChanged?.Invoke(chromiumWebBrowser, urls);
        }

        public event EventHandler<bool>? FullscreenChanged;
        public void OnFullscreenModeChange(IWebBrowser chromiumWebBrowser, IBrowser browser, bool fullscreen)
        {
            FullscreenChanged?.Invoke(chromiumWebBrowser, fullscreen);
        }
        public event EventHandler<double>? LoadingProgressChanged;
        public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
        {
            LoadingProgressChanged?.Invoke(chromiumWebBrowser, progress);
        }
        public event EventHandler<StatusMessageEventArgs>? StatusMessage;
        public void OnStatusMessage(IWebBrowser chromiumWebBrowser, StatusMessageEventArgs statusMessageArgs)
        {
            StatusMessage?.Invoke(chromiumWebBrowser, statusMessageArgs);
        }
        public event EventHandler<TitleChangedEventArgs>? TitleChanged;
        public void OnTitleChanged(IWebBrowser chromiumWebBrowser, TitleChangedEventArgs titleChangedArgs)
        {
            TitleChanged?.Invoke(chromiumWebBrowser, titleChangedArgs);
        }

        public bool OnTooltipChanged(IWebBrowser chromiumWebBrowser, ref string text)
        {
            //Only called when using Off-screen rendering (WPF and OffScreen).
            //To handle the display of the tooltip yourself return true otherwise return false to allow the browser to display the tooltip.
            return false;
        }
    }
}
