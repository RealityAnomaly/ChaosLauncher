using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SourceLauncher.Controls
{
    /// <inheritdoc cref="TreeView"/>
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser
    {
        private readonly IDictionary<string, BitmapSource> _fileIconCache
            = new Dictionary<string, BitmapSource>();

        public event FileOpenedDelegate FileOpened;
        public delegate void FileOpenedDelegate(string filePath);

        public FileBrowser()
        {
            InitializeComponent();
        }

        public void BuildTree(string basePath)
        {
            Items.Clear();

            var item = BuildTreeItem(basePath);
            Items.Add(item);
        }

        private FileItem BuildTreeItem(string path)
        {
            var dict = new FileItem {Name = Path.GetFileName(path), Path = path, Icon = @"/Images/common/folder.png", IsDirectory = true};

            // Add all the files in this directory
            foreach (var filePath in Directory.GetFiles(path))
                dict.Children.Add(new FileItem { Name = Path.GetFileName(filePath), Path = filePath, Icon = ResolveExtensionIcon(filePath) });

            // Search the directories under this directory
            foreach (var dirPath in Directory.GetDirectories(path))
                dict.Children.Add(BuildTreeItem(dirPath));

            return dict;
        }

        private dynamic ResolveExtensionIcon(string path)
        {
            return ResolveExtensionIconNative(path);
        }

        private BitmapSource ResolveExtensionIconNative(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == null)
                return null;

            if (_fileIconCache.ContainsKey(ext))
                return _fileIconCache[ext];

            using (var ico = Icon.ExtractAssociatedIcon(path))
            {
                if (ico == null)
                    return null;

                // Decode the image
                var image = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // Save in cache so we aren't fetching for every single file
                _fileIconCache[ext] = image;
                return image;
            }
        }

        private void FileBrowser_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = GetSelectedItem((FrameworkElement) e.OriginalSource);
            if (!(item.DataContext is FileItem file))
                return;

            if (!file.IsDirectory)
                FileOpened?.Invoke(file.Path);
        }

        private TreeViewItem GetSelectedItem(UIElement sender)
        {
            var point = sender.TranslatePoint(new System.Windows.Point(0, 0), this);
            var test = InputHitTest(point) as DependencyObject;
            while (test != null && !(test is TreeViewItem))
                test = VisualTreeHelper.GetParent(test);
            return test as TreeViewItem;
        }
    }
}
