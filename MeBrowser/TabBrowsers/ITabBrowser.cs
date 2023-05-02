using MeBrowser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace MeBrowser.TabBrowsers
{
    public interface ITabBrowser
    {
        TabNotify TabMain { get; }
        ICommand OpenNewTab { get; }
        ICommand CloseThisTab { get; }
        ICommand OpenSetupTab { get; }
        //ObservableCollection<HistoryEntry> History { get; }
    }
}
