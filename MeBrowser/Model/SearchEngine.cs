using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.Model
{
    public enum SearchEngine
    {
        [Description("https://www.google.com/search?q=")]
        Google,
        [Description("https://www.bing.com/search?q=")]
        Bing
    }
}
