using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MeBrowser.Model
{
    public class ColorContainer
    {
        public string Name { get; set; }
        public Brush Brush { get; set; }
        public override string ToString()
        {
            return base.ToString() + $" {Name}";
        }
    }
}
