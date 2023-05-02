using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MeBrowser.Model
{
    public class TabNavigationEntry : BindableBase
    {
        public int Id { get; set; }
        public string Title { get => GetProperty<string>(); set => SetProperty(value); }
        public string Url { get => GetProperty<string>(); set => SetProperty(value); }
        public string FaviconUrl { get => GetProperty<string>(); set => SetProperty(value); }
    }
}
