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
    public class MeRequestHandler : RequestHandler
    {
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new MeResourceRequestHandler();
        }

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            //Return true to cancel the navigation or false to allow the navigation to proceed.
            //If the navigation is allowed FrameLoadStart and FrameLoadEnd will be called.
            //If the navigation is canceled LoadError will be called with an ErrorCode value of Aborted.

            //if (frame.IsValid && !string.IsNullOrWhiteSpace(frame.Url))
            //{
            //    if (frame.Url.IsAd())
            //    {
            //        return true;
            //    }
            //}
            //return false;

            //if (request.TransitionType == TransitionType.LinkClicked) return true;
            return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
        }
    }
}
