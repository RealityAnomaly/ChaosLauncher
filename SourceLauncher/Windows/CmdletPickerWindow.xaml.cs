using SourceLauncher.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SourceLauncher.Windows
{
    /// <summary>
    /// Interaction logic for CmdletPicker.xaml
    /// </summary>
    public partial class CmdletPickerWindow : Window
    {
        private ChaosShell shell;
        private ObservableCollection<CommandInfo> commands = new ObservableCollection<CommandInfo>();
        public CmdletPickerWindow(ChaosShell shell)
        {
            this.shell = shell;
            InitializeComponent();

            setBtn.Click += delegate { Close(); };

            Task.Run(() =>
            {
                IDictionary<string, CommandInfo> commandsTemp = shell.GetCommands("SourceRun");
                if (commandsTemp == null)
                    return;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (KeyValuePair<string, CommandInfo> pair in commandsTemp)
                        commands.Add(pair.Value);

                    cmdletList.ItemsSource = commands;
                }));
            });
        }

        private void CmdletList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdletList.SelectedItem != null)
            {
                setBtn.IsEnabled = true;

                // Retrieve help for the command from ChaosShell
                CommandInfo meta = (CommandInfo)cmdletList.SelectedItem;
                Task.Run(() =>
                {
                    string help = shell.GetCommandHelp(meta.Name);

                    if (!(String.IsNullOrWhiteSpace(help)))
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            cmdletHelp.Text = help;
                        }));
                    } 
                });
            }
            else
            {
                setBtn.IsEnabled = false;
                cmdletHelp.Text = String.Empty;
            }
        }
    }
}
