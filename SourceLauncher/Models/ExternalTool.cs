using System.IO;

namespace SourceLauncher.Models
{
    internal class ExternalTool : Tool
    {
        private ExternalTool(string name) : base(name)
        {

        }

        public override string GetShortName()
        {
            return Path.GetFileName(Name);
        }

        public static ExternalTool PickTool()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filter = @"Executable Files|*.exe;*.com;*.scr|Batch Scripts|*.bat;*.cmd|Python Scripts|*.py";
                dialog.Title = @"Select the executable to use as an external tool.";
                var dr = dialog.ShowDialog();

                if (dr != System.Windows.Forms.DialogResult.OK)
                    return null;

                var tool = new ExternalTool(dialog.FileName);
                return tool;
            }
        }
    }
}
