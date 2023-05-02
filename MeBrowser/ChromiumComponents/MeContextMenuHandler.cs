using CefSharp;
using CefSharp.DevTools.Page;
using CefSharp.Wpf;
using CefSharp.Wpf.Handler;
using MaterialDesignThemes.Wpf;
using MeBrowser.Helpers;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace MeBrowser.ChromiumComponents
{
    public class MeContextMenuHandler : ContextMenuHandler
    {
        //public MeContextMenuHandler() : base(true) { }
        protected override void OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            //Gọi hàm từ base class sẽ add thêm 2 menu item Show DevTools và Close DevTools
            //base.OnBeforeContextMenu(chromiumWebBrowser, browser, frame, parameters, model);

            // Remove any existent option using the Clear method of the model
            //
            model.Clear();

            var resources = Application.Current.Resources;

            //Nếu bắt được link
            if (!string.IsNullOrWhiteSpace(parameters.LinkUrl))
            {
                model.AddItem((CefMenuCommand)26501, (string)resources["Chromium.ContextMenu.OpenLinkInNewTab"]);
                model.AddItem((CefMenuCommand)26508, (string)resources["Chromium.ContextMenu.OpenLinkInBackgroundTab"]);
                model.AddItem((CefMenuCommand)26506, (string)resources["Chromium.ContextMenu.CopyLink"]);
                model.AddSeparator();
            } 

            if(parameters.MediaType == ContextMenuMediaType.Image)
            { 
                model.AddItem((CefMenuCommand)26509, (string)resources["Chromium.ContextMenu.OpenImageInNewTab"]);
                model.AddItem((CefMenuCommand)26510, (string)resources["Chromium.ContextMenu.CopyLinkImage"]);
                model.AddItem((CefMenuCommand)26511, (string)resources["Chromium.ContextMenu.SaveImageAs"]);
                model.AddSeparator();
            }

            //Nếu có text bôi đen
            if (!string.IsNullOrWhiteSpace(parameters.SelectionText))
            {
                model.AddItem((CefMenuCommand)26514, (string)resources["Chromium.ContextMenu.Translate"]);
                model.AddItem((CefMenuCommand)26502, (string)resources["Chromium.ContextMenu.SearchGoogleFor"] + $" \"{parameters.SelectionText}\"");
                model.AddItem((CefMenuCommand)26503, (string)resources["Chromium.ContextMenu.OrSearchWithBing"]);
                model.AddItem(CefMenuCommand.Find, (string)resources["Chromium.ContextMenu.Search"] + "???" + (string)resources["Chromium.ContextMenu.OnPage"]);
                model.AddItem((CefMenuCommand)26507, (string)resources["Chromium.ContextMenu.ClearSearchResult"]);
                model.AddSeparator();
                model.AddItem(CefMenuCommand.Copy, (string)resources["Chromium.ContextMenu.Copy"]);
                model.AddSeparator();
            } 

            //Nếu không trong 1 khu vực edit text nào đó
            if (!parameters.IsEditable && string.IsNullOrWhiteSpace(parameters.SelectionText))
            {
                model.AddItem(CefMenuCommand.Back, (string)resources["Chromium.ContextMenu.Back"]);
                model.SetEnabled(CefMenuCommand.Back, browser.CanGoBack);

                model.AddItem(CefMenuCommand.Forward, (string)resources["Chromium.ContextMenu.Forward"]);
                model.SetEnabled(CefMenuCommand.Forward, browser.CanGoForward);

                model.AddItem(CefMenuCommand.Reload, (string)resources["Chromium.ContextMenu.Reload"]);
                model.AddSeparator();
            }

            //Nếu trong 1 khu vực edit text nào đó
            if (parameters.IsEditable)
            {
                model.AddItem(CefMenuCommand.Cut, (string)resources["Chromium.ContextMenu.Cut"]);
                model.SetEnabled(CefMenuCommand.Cut, !string.IsNullOrWhiteSpace(parameters.SelectionText));
                //remove command Copy và separator nếu nó được add từ bên trên 
                model.RemoveAt(model.GetIndexOf(CefMenuCommand.Copy) - 1);
                model.Remove(CefMenuCommand.Copy);
                model.AddItem(CefMenuCommand.Copy, (string)resources["Chromium.ContextMenu.Copy"]);
                model.SetEnabled(CefMenuCommand.Copy, !string.IsNullOrWhiteSpace(parameters.SelectionText));
                model.AddItem(CefMenuCommand.Paste, (string)resources["Chromium.ContextMenu.Paste"]);
                model.SetEnabled(CefMenuCommand.Paste, Clipboard.ContainsText());
                model.AddSeparator();
            } 

            model.AddItem(CefMenuCommand.SelectAll, (string)resources["Chromium.ContextMenu.SelectAll"]);

            if (!parameters.IsEditable && string.IsNullOrWhiteSpace(parameters.SelectionText))
            {
                model.AddSeparator();
                model.AddItem(CefMenuCommand.Print, (string)resources["Chromium.ContextMenu.Print"]);
                model.AddItem((CefMenuCommand)26504, (string)resources["Chromium.ContextMenu.ViewSource"]);
                model.AddItem((CefMenuCommand)26505, (string)resources["Chromium.ContextMenu.Inspect"]);
                model.AddItem((CefMenuCommand)26513, "Me Debugger");
                model.AddSeparator();
                model.AddItem((CefMenuCommand)26512, (string)resources["Chromium.ContextMenu.Screenshot"]);
            }
        }

        protected override bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            if (commandId == (CefMenuCommand)26501)
            {
                OpenLinkInNewTab?.Invoke(chromiumWebBrowser, new OpenNewTabParams(parameters.LinkUrl));
                return true;
            }

            if (commandId == (CefMenuCommand)26502)
            {
                SearchGoogle?.Invoke(chromiumWebBrowser, parameters.SelectionText);
                return true;
            }

            if (commandId == (CefMenuCommand)26503)
            {
                //Format the Url with the search query using the 'Bing' service
                string searchAddress = "https://www.bing.com/search?q=" + parameters.SelectionText;
                //And open new popup using the previously formatted Url
                frame.ExecuteJavaScriptAsync($"window.open('{searchAddress}', '_blank')");
                //Notify if the context contextMenu click is handled
                return true;
            }

            if (commandId == (CefMenuCommand)26504)
            {
                OpenLinkInNewTab?.Invoke(chromiumWebBrowser, new OpenNewTabParams("view-source:" + parameters.PageUrl));
                return true;
            }

            if (commandId == (CefMenuCommand)26505)
            {
                if (chromiumWebBrowser is ChromiumWebBrowser chromium)
                    chromium.Dispatcher.BeginInvoke(() => browser.GetHost().ShowDevTools());
                return true;
            }

            //thằng này sẽ return false (https://github.com/cefsharp/CefSharp/blob/master/CefSharp/Handler/ContextMenuHandler.cs)
            return base.OnContextMenuCommand(chromiumWebBrowser, browser, frame, parameters, commandId, eventFlags);
        }

        protected override bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            var webBrowser = (ChromiumWebBrowser)chromiumWebBrowser;

            //IMenuModel is only valid in the context of this method, so need to read the values before invoking on the UI thread
            var menuItemModels = GetMenuItemModels(model);

            //Do thằng lìn IContextMenuParams parameters này toàn bị disposed
            ContextMenuParamsWrapper paramsWrapper =
                new ContextMenuParamsWrapper(parameters.DictionarySuggestions,
                                             parameters.XCoord,
                                             parameters.YCoord,
                                             parameters.SelectionText,
                                             parameters.MisspelledWord,
                                             parameters.LinkUrl,
                                             parameters.PageUrl,
                                             parameters.SourceUrl);


            webBrowser.RunAsyncInUI(() =>
            {
                var contextMenu = new ContextMenu
                {
                    IsOpen = true,
                    Placement = PlacementMode.Mouse
                };

                RoutedEventHandler handler = null;

                handler = (s, e) =>
                {
                    contextMenu.Closed -= handler;

                    //If the callback has been disposed then it's already been executed
                    //so don't call Cancel
                    if (!callback.IsDisposed)
                    {
                        callback.Cancel();
                    }
                };

                contextMenu.Closed += handler;

                AddMenuItemFromMenuItemModel(contextMenu, menuItemModels, browser, paramsWrapper);

                webBrowser.ContextMenu = contextMenu;
            });

            //Mở ContextMenu đẹp hơn (của Wpf)
            return true;
        }

        //Tạo các MenuItem cho 1 MenuItem hoặc ContextMenu từ 1 danh sách các MenuItemModel
        private void AddMenuItemFromMenuItemModel(ItemsControl itemsControl, IList<MenuItemModel> menuItemModels, IBrowser browser, ContextMenuParamsWrapper paramsWrapper)
        {
            foreach (var item in menuItemModels)
            {
                if (item.IsSeperator)
                {
                    itemsControl.Items.Add(new Separator());

                    continue;
                }

                if (item.CommandId == CefMenuCommand.NotFound)
                {
                    continue;
                }

                var menuItem = new MenuItem
                {
                    Header = item.Label?.Replace("&", "_"),
                    IsEnabled = item.IsEnabled,
                    IsChecked = item.IsChecked.GetValueOrDefault(),
                    IsCheckable = item.IsChecked.HasValue,
                    Command = new RelayCommand(obj =>
                    {
                        ExecuteCommand(browser, item.CommandId, paramsWrapper);
                    }),
                };

                Brush foreground = (Brush)Application.Current.Resources["Primary.Dark"];
                if (item.CommandId == (CefMenuCommand)26501)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.Tab, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26508)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.TabUnselected, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26506)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.LinkVariantPlus, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26503)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.MicrosoftBing, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26507)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.SearchExpand, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Back)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ArrowLeft, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Forward)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ArrowRight, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Reload)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.Reload, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Cut)
                {
                    menuItem.InputGestureText = "Ctrl+X";
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ContentCut, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Copy)
                {
                    menuItem.InputGestureText = "Ctrl+C";
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ContentCopy, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Paste)
                {
                    menuItem.InputGestureText = "Ctrl+V";
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ContentPaste, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.SelectAll)
                {
                    menuItem.InputGestureText = "Ctrl+A";
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.SelectAll, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Print)
                { 
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.Printer, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26504)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.LanguageHtml5, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26505)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.DeveloperBoard, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26509)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.FileImageOutline, Width = 20, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26510)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.ImageMultipleOutline, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26511)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.FileImageAddOutline, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26512)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.MonitorScreenshot, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26513)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.Security, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26514)
                {
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.GoogleTranslate, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == (CefMenuCommand)26502)
                {
                    StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(new TextBlock { Text = (string)Application.Current.Resources["Chromium.ContextMenu.SearchGoogleFor"] + " " });
                    stackPanel.Children.Add(new TextBlock { Text = "\"", Foreground = (Brush)Application.Current.Resources["DeepOrange.Dark"] });
                    stackPanel.Children.Add(new TextBlock { Text = $"{paramsWrapper.SelectionText}", Foreground = (Brush)Application.Current.Resources["DeepPurple.Dark"], MaxWidth = 600, TextTrimming = TextTrimming.CharacterEllipsis });
                    stackPanel.Children.Add(new TextBlock { Text = "\"", Foreground = (Brush)Application.Current.Resources["DeepOrange.Dark"] });
                    menuItem.Header = stackPanel;
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.DinosaurPixel, Width = 18, Foreground = foreground };
                }
                if (item.CommandId == CefMenuCommand.Find)
                {
                    StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(new TextBlock { Text = (string)Application.Current.Resources["Chromium.ContextMenu.Search"] + " " });
                    stackPanel.Children.Add(new TextBlock { Text = "\"", Foreground = (Brush)Application.Current.Resources["DeepOrange.Dark"] });
                    stackPanel.Children.Add(new TextBlock { Text = $"{paramsWrapper.SelectionText}", Foreground = (Brush)Application.Current.Resources["DeepPurple.Dark"], MaxWidth = 200, TextTrimming = TextTrimming.CharacterEllipsis });
                    stackPanel.Children.Add(new TextBlock { Text = "\"", Foreground = (Brush)Application.Current.Resources["DeepOrange.Dark"] });
                    stackPanel.Children.Add(new TextBlock { Text = " " + (string)Application.Current.Resources["Chromium.ContextMenu.OnPage"] });
                    menuItem.Header = stackPanel;
                    menuItem.Icon = new PackIcon { Kind = PackIconKind.Search, Width = 20, Foreground = foreground };
                }

                //TODO: Make this recursive and remove duplicate code
                if (item.SubModel != null && item.SubModel.Count > 0)
                {
                    AddMenuItemFromMenuItemModel(menuItem, item.SubModel, browser, paramsWrapper);
                }

                itemsControl.Items.Add(menuItem);
            }
        }

        public event EventHandler<OpenNewTabParams>? OpenLinkInNewTab;
        public event EventHandler<string>? SearchGoogle;
        public event EventHandler? ScreenshotRequest;
        public event EventHandler<string>? SaveImageAs;
        public event EventHandler? DebuggerRequest;
        public event EventHandler<string>? FindOnPage;
        public event EventHandler<string>? Translate;

        protected virtual void ExecuteCommand(IBrowser browser, CefMenuCommand commandId, ContextMenuParamsWrapper paramsWrapper)
        {
            // If the user chose a replacement word for a misspelling, replace it here.
            if (commandId >= CefMenuCommand.SpellCheckSuggestion0 &&
                commandId <= CefMenuCommand.SpellCheckSuggestion4)
            {
                int sugestionIndex = (int)commandId - (int)CefMenuCommand.SpellCheckSuggestion0;
                if (sugestionIndex < paramsWrapper.DictionarySuggestions.Count)
                {
                    var suggestion = paramsWrapper.DictionarySuggestions[sugestionIndex];
                    browser.ReplaceMisspelling(suggestion);
                }

                return;
            }

            switch (commandId)
            {
                // Navigation.
                case CefMenuCommand.Back:
                    {
                        browser.GoBack();
                        break;
                    }
                case CefMenuCommand.Forward:
                    {
                        browser.GoForward();
                        break;
                    }
                case CefMenuCommand.Reload:
                    {
                        browser.Reload();
                        break;
                    }
                case CefMenuCommand.ReloadNoCache:
                    {
                        browser.Reload(ignoreCache: true);
                        break;
                    }
                case CefMenuCommand.StopLoad:
                    {
                        browser.StopLoad();
                        break;
                    }

                //Editing
                case CefMenuCommand.Undo:
                    {
                        browser.FocusedFrame.Undo();
                        break;
                    }
                case CefMenuCommand.Redo:
                    {
                        browser.FocusedFrame.Redo();
                        break;
                    }
                case CefMenuCommand.Cut:
                    {
                        browser.FocusedFrame.Cut();
                        break;
                    }
                case CefMenuCommand.Copy:
                    {
                        browser.FocusedFrame.Copy();
                        break;
                    }
                case CefMenuCommand.Paste:
                    {
                        browser.FocusedFrame.Paste();
                        break;
                    }
                case CefMenuCommand.Delete:
                    {
                        browser.FocusedFrame.Delete();
                        break;
                    }
                case CefMenuCommand.SelectAll:
                    {
                        browser.FocusedFrame.SelectAll();
                        break;
                    }

                // Miscellaneous.
                case CefMenuCommand.Print:
                    {
                        browser.GetHost().Print();
                        break;
                    }
                case CefMenuCommand.ViewSource:
                    {
                        browser.FocusedFrame.ViewSource();
                        break;
                    }
                case CefMenuCommand.Find:
                    {
                        browser.GetHost().Find(paramsWrapper.SelectionText, false, false, false);
                        FindOnPage?.Invoke(browser, paramsWrapper.SelectionText);
                        break;
                    }

                // Spell checking.
                case CefMenuCommand.AddToDictionary:
                    {
                        browser.GetHost().AddWordToDictionary(paramsWrapper.MisspelledWord);
                        break;
                    }

                case (CefMenuCommand)CefMenuCommandShowDevToolsId:
                    {
                        browser.GetHost().ShowDevTools(inspectElementAtX: paramsWrapper.XCoord, inspectElementAtY: paramsWrapper.YCoord);
                        break;
                    }
                case (CefMenuCommand)CefMenuCommandCloseDevToolsId:
                    {
                        browser.GetHost().CloseDevTools();
                        break;
                    }

                //Me custom command
                case (CefMenuCommand)26501:
                    {
                        OpenLinkInNewTab?.Invoke(browser, new OpenNewTabParams(paramsWrapper.LinkUrl));
                        break;
                    }

                case (CefMenuCommand)26502:
                    {
                        SearchGoogle?.Invoke(browser, paramsWrapper.SelectionText);
                        break;
                    }

                case (CefMenuCommand)26503:
                    {
                        //Format the Url with the search query using the 'Bing' service
                        string searchAddress = "https://www.bing.com/search?q=" + paramsWrapper.SelectionText;
                        //And open new popup using the previously formatted Url
                        browser.FocusedFrame.ExecuteJavaScriptAsync($"window.open('{searchAddress}', '_blank')");
                        break;
                    }

                case (CefMenuCommand)26504:
                    {
                        OpenLinkInNewTab?.Invoke(browser, new OpenNewTabParams("view-source:" + paramsWrapper.PageUrl));
                        break;
                    }

                case (CefMenuCommand)26505:
                    {
                        browser.GetHost().ShowDevTools();
                        break;
                    }
                case (CefMenuCommand)26506:
                    {
                        Clipboard.SetText(paramsWrapper.LinkUrl);
                        break;
                    }
                case (CefMenuCommand)26507:
                    {
                        browser.GetHost().StopFinding(true);
                        break;
                    }

                case (CefMenuCommand)26508:
                    {
                        OpenLinkInNewTab?.Invoke(browser, new OpenNewTabParams(paramsWrapper.LinkUrl, false));
                        break;
                    }

                case (CefMenuCommand)26509:
                    {
                        OpenLinkInNewTab?.Invoke(browser, new OpenNewTabParams(paramsWrapper.SourceUrl));
                        break;
                    }

                case (CefMenuCommand)26510:
                    {
                        Clipboard.SetText(paramsWrapper.SourceUrl);
                        break;
                    }
                case (CefMenuCommand)26511:
                    {
                        SaveImageAs?.Invoke(browser, paramsWrapper.SourceUrl);
                        break;
                    }
                case (CefMenuCommand)26512:
                    {
                        ScreenshotRequest?.Invoke(browser, EventArgs.Empty);
                        break;
                    }
                case (CefMenuCommand)26513:
                    {
                        DebuggerRequest?.Invoke(browser, EventArgs.Empty);
                        break;
                    }
                case (CefMenuCommand)26514:
                    {
                        Translate?.Invoke(browser, paramsWrapper.SelectionText);
                        break;
                    }
            }
        }

        //Lấy danh sách các item con trong 1 context menu của CEF
        private static IList<MenuItemModel> GetMenuItemModels(IMenuModel model)
        {
            var menuItemModels = new List<MenuItemModel>();

            for (var i = 0; i < model.Count; i++)
            {
                var type = model.GetTypeAt(i);
                bool? isChecked = null;

                if (type == MenuItemType.Check)
                {
                    isChecked = model.IsCheckedAt(i);
                }

                var sub = model.GetSubMenuAt(i);

                var subModel = sub == null ? null : GetMenuItemModels(sub);

                var menuItemModel = new MenuItemModel
                {
                    Label = model.GetLabelAt(i),
                    CommandId = model.GetCommandIdAt(i),
                    IsEnabled = model.IsEnabledAt(i),
                    Type = type,
                    IsSeperator = type == MenuItemType.Separator,
                    IsChecked = isChecked,
                    SubModel = subModel
                };

                menuItemModels.Add(menuItemModel);
            }

            return menuItemModels;
        }

        //Đóng gói thông tin để chuyển sang MenuItem của Wpf
        //Mỗi IMenuModel đại diện cho 1 context menu của CEF
        //Mỗi MenuItemModel này tương ứng với 1 item con của nó
        internal class MenuItemModel
        {
            internal string? Label { get; set; }
            internal CefMenuCommand CommandId { get; set; }
            internal bool IsEnabled { get; set; }
            internal bool IsSeperator { get; set; }
            internal bool? IsChecked { get; set; }
            internal MenuItemType Type { get; set; }

            internal IList<MenuItemModel>? SubModel { get; set; }
        }

        protected override void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            //Sau khi tắt contextMenu, gán cho nó = null để GC dọn đi
            base.OnContextMenuDismissed(chromiumWebBrowser, browser, frame);
        }

        //mở context menu xấu xấu của CEF
        //protected override bool RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        //{
        //    
        //    return false;
        //}
    }
}
