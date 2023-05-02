using CefSharp;
using CefSharp.Handler;
using MeBrowser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.ChromiumComponents
{
    public class MeResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (frame.IsValid && !string.IsNullOrWhiteSpace(frame.Url))
            {
                //Debug.WriteLine(frame.IsMain + " " + frame.Url);

                if (frame.Url.IsAd())
                {                   
                    return CefReturnValue.Cancel; 
                }

                //Debug.WriteLine(frame.IsMain + " " + frame.Url);

                if (frame.IsMain && !chromiumWebBrowser.IsDisposed)
                {
                    AdBlocker.BlockByPage(frame.Url, chromiumWebBrowser);
                }
            }

            return CefReturnValue.Continue;
        }
    }
}
