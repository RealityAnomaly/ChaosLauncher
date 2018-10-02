using Newtonsoft.Json;
using SourceLauncher.Controls;
using SourceLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class CmdletTool : Tool
    {
        [JsonIgnore]
        public readonly CommandInfo Metadata;
        public CmdletTool(CommandInfo meta) : base(meta.Name)
        {
            Metadata = meta;
        }

        public static CmdletTool PickTool(ChaosShell shell)
        {
            CmdletPickerWindow pickerWindow = new CmdletPickerWindow(shell);
            pickerWindow.ShowDialog();

            if (pickerWindow.cmdletList.SelectedItem == null)
                return null;

            CommandInfo item = (CommandInfo)pickerWindow.cmdletList.SelectedItem;
            CmdletTool newTool = new CmdletTool(item);

            return newTool;
        }
    }
}
