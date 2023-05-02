using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeBrowser.Model
{
    public class HistoryEntry : BindableBase
    {
        public bool IsMarked { get => GetProperty<bool>(); set => SetProperty(value); }
        public DateTime Time { get; set; }
        public string Url { get; set; }
    }
}
