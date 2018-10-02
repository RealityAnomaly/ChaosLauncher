using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Collections;
using System.IO;
using System.Threading;

namespace SourceLauncher.Controls
{
    /// <summary>
    /// Interaction logic for ChaosShell.xaml
    /// </summary>
    public partial class ChaosShell : UserControl, IDisposable
    {
        private readonly object threadLock = new object();

        private PowerShell powerShell;
        //private TextPointer inputStartPos;
        private int offset;

        public bool IsExecuting { get; private set; }
        public ChaosShell()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                Clear();
                InvokeScript("Import-Module SourceRun");
            });
        }

        public void Clear()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                internalShell.Document.Blocks.Clear();
            }));
            
            AppendScroll("HELIOS ChaosShell\rCopyright (C) Joseph Marsden. All rights reserved.\r");
            UpdateLocation();
        }

        public IDictionary<string, CommandInfo> GetCommands(string module)
        {
            Collection<PSObject> par = InvokeScript(String.Format("(Get-Module -Name {0}).ExportedCommands", module));

            if (par.Count <= 0 || par[0] == null)
                return null;

            IDictionary<string, CommandInfo> meta = par[0].BaseObject as IDictionary<string, CommandInfo>;
            return meta;
        }

        public CommandInfo GetCommand(string command)
        {
            Collection<PSObject> par = InvokeScript(String.Format("Get-Command -Name {0}", command));
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].BaseObject as CommandInfo;
        }

        public IDictionary<string, ParameterMetadata> GetParameters(string command)
        {
            CommandInfo commandMeta = GetCommand(command);
            if (command == null)
                return null;

            IDictionary<string, ParameterMetadata> meta = commandMeta.Parameters as IDictionary<string, ParameterMetadata>;
            return meta;
        }

        public string GetCommandHelp(string command)
        {
            Collection<PSObject> par = InvokeScript(String.Format("Get-Help -Name {0} | Out-String", command));
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].ToString();
        }

        public string GetParameterHelp(string command, string parameter)
        {
            Collection<PSObject> par = InvokeScript(String.Format("Get-Help -Name {0} -Parameter {1} | Out-String", command, parameter));
            if (par.Count <= 0)
                return null;

            return par[0].ToString();
        }

        public void AppendScroll(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                internalShell.AppendText(text);
                internalShell.ScrollToEnd();
            }));
            //inputStartPos = internalShell.Document.ContentEnd.GetPositionAtOffset(0);
        }

        public void UpdateLocation()
        {
            offset = 0;

            Collection<PSObject> locations = InvokeInternal("Get-Location");
            PathInfo path = locations[0].BaseObject as PathInfo;
            AppendScroll(String.Format("\rCS {0}> ", path.Path));
        }

        private Collection<PSObject> InvokeInternal(string Script)
        {
            Collection<PSObject> result = null;

            if (powerShell == null)
            {
                powerShell = PowerShell.Create();
            }

            powerShell.Streams.Debug.DataAdded += Debug_DataAdded;
            powerShell.Streams.Error.DataAdded += Error_DataAdded;
            powerShell.Streams.Progress.DataAdded += Progress_DataAdded;
            powerShell.Streams.Verbose.DataAdded += Verbose_DataAdded;
            powerShell.Streams.Warning.DataAdded += Warning_DataAdded;

            powerShell.AddScript(Script);

            try
            {
                result = powerShell.Invoke();
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(e.Message, "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                }));

                return null;
            }

            return result;
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            WarningRecord newRecord = ((PSDataCollection<WarningRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            VerboseRecord newRecord = ((PSDataCollection<VerboseRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            ErrorRecord newRecord = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.ErrorDetails);
        }

        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            DebugRecord newRecord = ((PSDataCollection<DebugRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            ProgressRecord newRecord = ((PSDataCollection<ProgressRecord>)sender)[e.Index];
        }

        public Collection<PSObject> InvokeScript(string Script)
        {
            Collection<PSObject> output;

            // lock to prevent two commands from being executed at once in the same runspace
            lock (threadLock)
            {
                IsExecuting = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar.IsIndeterminate = true;
                }));

                AppendScroll(Script);
                output = InvokeInternal(Script);
                UpdateLocation();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar.IsIndeterminate = false;
                }));

                IsExecuting = false;
            }

            if (output != null)
            {
                return output;
            }

            return null;
        }

        public void Dispose()
        {
            powerShell.Dispose();
        }

        private void InternalShell_KeyDown(object sender, KeyEventArgs e)
        {
            bool isReadOnly = offset <= 0;

            // Supress backspace since the area is read-only.
            if (isReadOnly && e.Key == Key.Back)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                offset--;
                return;
            }

            // Ignore offsets for selection keys.
            if (!(e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Up ||
                e.Key == Key.Down ||
                (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))))
            {
                // Execute, if the return key is pressed
                if (e.Key == Key.Return)
                {
                    string input = new TextRange(internalShell.Document.ContentEnd.GetPositionAtOffset(-offset), internalShell.Document.ContentEnd.GetPositionAtOffset(0)).Text
                        .Replace("\r", String.Empty).Replace("\r\n", String.Empty);
                    AppendScroll("\r");

                    InvokeInternal(input);

                    UpdateLocation();
                    return;
                }

                offset++;
                return;
            }
        }

        public void SetReadOnly(bool readOnly)
        {
            internalShell.IsReadOnly = readOnly;
        }
    }
}
