using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.Model
{
    public class DownloadArgs
    {
        public DownloadItem Item { get; set; }
        public IDownloadItemCallback Callback { get; set; }
    }
}
