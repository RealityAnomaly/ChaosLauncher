using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SourceLauncher.Models
{
    public class SourceGame : IEquatable<SourceGame>
    {
        public string AppId;
        public string AppExecutable;
        public string ProductName;
        public string ContentDir;

        public bool Equals(SourceGame other)
        {
            return other != null && AppId.Equals(other.AppId);
        }

        public override string ToString()
        {
            return $"{ProductName} ({AppId})";
        }
    }
}
