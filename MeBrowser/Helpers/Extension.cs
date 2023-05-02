using CefSharp.Wpf;
using MeBrowser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MeBrowser.Helpers
{
    public static partial class Extension
    {
        public static T? FindAncestor<T>(this FrameworkElement element)
        {
            var parent = element.Parent;
            while (parent is not null)
            {
                if (parent is T t) return t;
                if (parent is FrameworkElement e) parent = e.Parent;
                else return default;
            }
            return default;
        }

        public static void RunAsyncInUI(this ChromiumWebBrowser browser, Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (browser.Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!browser.Dispatcher.HasShutdownStarted)
            {
                browser.Dispatcher.BeginInvoke(action, priority);
            }
        }

        public static void RunSyncInUI(this ChromiumWebBrowser browser, Action action, DispatcherPriority priority = DispatcherPriority.DataBind)
        {
            if (browser.Dispatcher.CheckAccess())
            {
                action();
            }
            else if (!browser.Dispatcher.HasShutdownStarted)
            {
                browser.Dispatcher.Invoke(action, priority);
            }
        }

        public static bool IsUrl(this string url)
        {
            return UrlExpression().IsMatch(url);
        }
        [GeneratedRegex(@"^(about:|data:|(^((http:\/\/)|(https:\/\/))?\w+[\-\w]*\.\w+[\-\w]*))[~%&',;=?#@!:\/\w\(\)\*\+\[\]\$\-\.]*$")]
        private static partial Regex UrlExpression();

        public static void RemoveWith<T>(this ICollection<T> collection, Predicate<T> canRemove)
        {
            List<T> temp = new List<T>(collection);

            foreach (T item in temp)
            {
                if (canRemove(item)) collection.Remove(item);
            }
        }
    }
}
