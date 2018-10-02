using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    class ExternalTool : Tool
    {
        public ExternalTool(string name) : base(name)
        {

        }

        public override string GetShortName()
        {
            return Path.GetFileName(Name);
        }

        public static ExternalTool PickTool()
        {
            using (System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filter = "Executable Files|*.exe;*.com;*.scr|Batch Scripts|*.bat;*.cmd|Python Scripts|*.py";
                dialog.Title = "Select the executable to use as an external tool.";
                System.Windows.Forms.DialogResult dr = dialog.ShowDialog();

                if (dr != System.Windows.Forms.DialogResult.OK)
                    return null;

                ExternalTool tool = new ExternalTool(dialog.FileName);
                return tool;
            }
        }
    }
}
