using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Controls
{
    public class FileItem : TreeItem
    {
        public string Path { get; set; }
        public dynamic Icon { get; set; }
        public bool IsDirectory { get; set; } = false;
    }
}
