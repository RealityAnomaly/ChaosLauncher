using Newtonsoft.Json;
using SourceLauncher.Controls;
using SourceLauncher.Windows;
using System.Management.Automation;

namespace SourceLauncher.Models
{
    public class CmdletTool : Tool
    {
        [JsonIgnore]
        public readonly CommandInfo Metadata;
        [JsonIgnore]
        public bool ReadOnly;

        protected CmdletTool(CommandInfo meta) : base(meta.Name)
        {
            Metadata = meta;
        }

        public static CmdletTool PickTool(ChaosShell shell)
        {
            var pickerWindow = new CmdletPickerWindow(shell);
            pickerWindow.ShowDialog();

            if (pickerWindow.cmdletList.SelectedItem == null)
                return null;

            var item = (CommandInfo)pickerWindow.cmdletList.SelectedItem;
            var newTool = new CmdletTool(item);

            return newTool;
        }
    }
}
