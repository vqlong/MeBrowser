using CefSharp;
using MeBrowser.ViewModel;
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
    public class JsDialogHandler : IJsDialogHandler
    {
        public bool OnBeforeUnloadDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string messageText, bool isReload, IJsDialogCallback callback)
        {
            //Custom implementation would look something like
            // - Create/Show dialog on UI Thread
            // - execute callback once user has responded
            // - callback.Continue(true);
            // - return true

            //NOTE: Returning false will trigger the default behaviour, no need to execute the callback if you return false.
            return false;
        }

        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            //nếu OnJSDialog cho phép hiện dialog, hàm này sẽ không được gọi đến

            //DialogBox.Show("OnDialogClosed");
        }

        public bool IsDialogHandled { get; private set; } = true;
        public bool OnJSDialog(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, CefJsDialogType dialogType, string messageText, string defaultPromptText, IJsDialogCallback callback, ref bool suppressMessage)
        {
            //không hiện dialog và tự nhấn OK/Cancel 
            //parameter 1: true/false => OK/Cancel
            //parameter 2: value return nếu dialog là prompt và nhấn OK
            //callback.Continue(true, "value");
            //if(dialogType == CefJsDialogType.Prompt) callback.Continue(true, "this is the return");

            //không hiện dialog
            //suppressMessage = true;

            //mặc định return false để hiện dialog như bình thường
            //return true để custom dialog
            //return false;

            if (!IsDialogHandled)
            {
                suppressMessage = true;
                return false;
            }

            IsDialogHandled = false; 

            if(dialogType == CefJsDialogType.Alert)
            {
                var operation = Application.Current.Dispatcher.InvokeAsync(() => DialogBox.Show(messageText, chromiumWebBrowser.Address, Model.DialogBoxButton.OK, Model.DialogBoxIcon.Warning));

                IsDialogHandled = true;
                return true;
            }

            if (dialogType == CefJsDialogType.Confirm)
            {
                var operation = Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if(DialogBox.Show(messageText, chromiumWebBrowser.Address, Model.DialogBoxButton.OKCancel, Model.DialogBoxIcon.Question) == Model.DialogBoxResult.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });

                if (operation.Result == true)
                {
                    callback.Continue(true);
                }
                else
                {
                    callback.Continue(false);
                }

                IsDialogHandled = true;

                return true;
            }

            if (dialogType == CefJsDialogType.Prompt)
            {
                var operation = Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    TextBlock textBlock = new TextBlock { Text = messageText };
                    TextBox textBox = new TextBox { Text = defaultPromptText };
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Children.Add(textBlock);
                    stackPanel.Children.Add(textBox);
                    if (DialogBox.Show(stackPanel, chromiumWebBrowser.Address, Model.DialogBoxButton.OKCancel, Model.DialogBoxIcon.Question) == Model.DialogBoxResult.OK)
                    {
                        return textBox.Text;
                    }
                    else
                    {
                        return string.Empty;
                    }
                });

                if (string.IsNullOrWhiteSpace(operation.Result))
                {
                    callback.Continue(false); 
                }
                else
                {
                    callback.Continue(true, operation.Result);
                }

                IsDialogHandled = true;

                return true;
            }

            return false;
        }

        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            //nếu OnJSDialog cho phép hiện dialog, hàm này sẽ không được gọi đến

            //DialogBox.Show("OnResetDialogState");
        }
    }
}
