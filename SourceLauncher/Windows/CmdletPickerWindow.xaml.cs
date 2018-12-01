using SourceLauncher.Controls;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PoshCode;
using SourceLauncher.Utilities;

namespace SourceLauncher.Windows
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for CmdletPicker.xaml
    /// </summary>
    public partial class CmdletPickerWindow
    {
        private readonly PoshConsole _shellHost;
        private readonly ObservableCollection<CommandInfo> _commands = new ObservableCollection<CommandInfo>();
        public CmdletPickerWindow(PoshConsole shellHost)
        {
            _shellHost = shellHost;
            InitializeComponent();

            setBtn.Click += delegate { Close(); };

            Task.Run(() =>
            {
                var commandsTemp = _shellHost.GetCommands("SourceRun");
                if (commandsTemp == null)
                    return;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var pair in commandsTemp)
                        _commands.Add(pair.Value);

                    cmdletList.ItemsSource = _commands;
                }));
            });
        }

        private void CmdletList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdletList.SelectedItem != null)
            {
                setBtn.IsEnabled = true;

                // Retrieve help for the command from ChaosShellRaw
                var meta = (CommandInfo)cmdletList.SelectedItem;
                Task.Run(() =>
                {
                    var help = _shellHost.GetCommandHelp(meta.Name);

                    if (!(string.IsNullOrWhiteSpace(help)))
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
                cmdletHelp.Text = string.Empty;
            }
        }
    }
}
