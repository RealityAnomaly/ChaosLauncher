using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SourceLauncher.Models
{
    public class DragInfo
    {
        public Point ElementPoint { get; set; }
        public UIElement Element { get; set; }

        public DragInfo(Point point, UIElement element)
        {
            ElementPoint = point;
            Element = element;
        }
    }
}
