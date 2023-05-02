using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MeBrowser.ChromiumComponents
{
    public class SampleContextMenuHandler : IContextMenuHandler
    {
        public event EventHandler<string>? OpenLinkInNewTabClick;
        public event EventHandler<string>? SearchGoogleForClick;

        //Khi bắt đầu click chuột phải
        public void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            // Remove any existent option using the Clear method of the model
            //
            model.Clear();

            model.AddItem((CefMenuCommand)26501, "Mở địa chỉ trong tab mới");
            model.SetEnabled((CefMenuCommand)26501, !string.IsNullOrWhiteSpace(parameters.LinkUrl));

            model.AddItem((CefMenuCommand)26502, $"Tìm kiếm Google cho \"{parameters.SelectionText}\"" + new string(' ', 10));
            model.SetEnabled((CefMenuCommand)26502, !string.IsNullOrWhiteSpace(parameters.SelectionText));
            model.AddSeparator();

            model.AddItem(CefMenuCommand.Back, "Trang trước");
            model.SetEnabled(CefMenuCommand.Back, browser.CanGoBack);

            model.AddItem(CefMenuCommand.Forward, "Trang sau");
            model.SetEnabled(CefMenuCommand.Forward, browser.CanGoForward);

            model.AddItem(CefMenuCommand.Reload, "Tải lại");
            model.AddSeparator();

            model.AddItem(CefMenuCommand.Cut, "Cắt");
            model.SetEnabled(CefMenuCommand.Cut, !string.IsNullOrWhiteSpace(parameters.SelectionText) && parameters.IsEditable);

            model.AddItem(CefMenuCommand.Copy, "Sao chép");
            model.SetEnabled(CefMenuCommand.Copy, !string.IsNullOrWhiteSpace(parameters.SelectionText));

            model.AddItem(CefMenuCommand.Paste, "Dán");
            model.SetEnabled(CefMenuCommand.Paste, Clipboard.ContainsText() && parameters.IsEditable);
            model.AddSeparator();

            model.AddItem(CefMenuCommand.SelectAll, "Chọn tất cả");
            model.AddSeparator();

            model.AddItem(CefMenuCommand.Find, "Tìm với Bing");
            model.SetEnabled(CefMenuCommand.Find, !string.IsNullOrWhiteSpace(parameters.SelectionText));

            model.AddItem(CefMenuCommand.Print, "In trang...");
            model.AddItem((CefMenuCommand)26503, "Xem nguồn trang...");
            //model.SetEnabledAt(0, !string.IsNullOrWhiteSpace(parameters.SelectionText));
            Debug.WriteLine("OnBeforeContextMenu!");
        }

        //Khi click vào 1 option trên menu
        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            Debug.WriteLine("OnContextMenuCommand!");

            if (commandId == (CefMenuCommand)26501)
            {
                OpenLinkInNewTabClick?.Invoke(chromiumWebBrowser, parameters.LinkUrl);
                return true;
            }

            if (commandId == (CefMenuCommand)26502)
            {
                SearchGoogleForClick?.Invoke(chromiumWebBrowser, parameters.SelectionText);
                return true;
            }

            if (commandId == (CefMenuCommand)26503)
            {
                if (chromiumWebBrowser is ChromiumWebBrowser chromium)
                    chromium.Dispatcher.BeginInvoke(() => browser.GetHost().ShowDevTools());
                return true;
            }

            if (commandId == CefMenuCommand.Find)
            {
                //Format the Url with the search query using the 'Bing' service
                string searchAddress = "https://www.bing.com/search?q=" + parameters.SelectionText;
                //And open new popup using the previously formatted Url
                frame.ExecuteJavaScriptAsync($"window.open('{searchAddress}', '_blank')");
                //Notify if the context menu click is handled
                return true;
            }

            //return false để những command có sẵn của CefSharp chạy theo mặc định
            return false;
        }

        //Khi menu biến mất
        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            Debug.WriteLine("OnContextMenuDismissed!");
        }

        //Chạy ngay sau khi OnBeforeContextMenu để xác định xem có custom display cho menu hay không
        public bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            Debug.WriteLine("RunContextMenu!");
            //return true sẽ không xổ ra context menu đã tạo ở OnBeforeContextMenu
            //tức là ta sẽ phải tạo 1 context menu thủ công (như của Wpf) để gán cho chromiumWebBrowser và xử lý các command cho nó
            return false;
        }

        //OnBeforeContextMenu => RunContextMenu => OnContextMenuCommand => OnContextMenuDismissed => xổ ra custom menu nếu có
        //***Chú ý: Tất cả các hàm này đều được gọi từ UI Thread của CEF, không phải UI Thread của application
    }
}
