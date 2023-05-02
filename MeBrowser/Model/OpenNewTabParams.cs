using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.Model
{
    public class OpenNewTabParams
    {
        public bool IsNewTabSelected { get; set; } = true;
        public string LinkUrl { get; set; } = "about:blank";
        public OpenNewTabParams()
        {
            
        }
        public OpenNewTabParams(string linkUrl, bool isSelectedNewTab = true)
        {
            LinkUrl = linkUrl;
            IsNewTabSelected = isSelectedNewTab;
        }
    }
}
