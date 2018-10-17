using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Windows.Controls;

namespace SourceLauncher.Controls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for ChaosShell.xaml
    /// </summary>
    public partial class ChaosShell :  IDisposable
    {
        private readonly object _threadLock = new object();
        private PowerShell _powerShell;
        //private TextPointer inputStartPos;
        private int _offset;

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

        /// <summary>
        /// Clears the shell, appends the copyright message and updates the location.
        /// </summary>
        private void Clear()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                internalShell.Document.Blocks.Clear();
            }));
            
            AppendScroll("HELIOS ChaosShell\rCopyright (C) Joseph Marsden. All rights reserved.\r");
            UpdateLocation();
        }

        /// <summary>
        /// Returns a list of commands from a PowerShell module.
        /// </summary>
        /// <param name="module">String containing the name of the module.</param>
        /// <returns>IDictionary containing the command and command info.</returns>
        public IDictionary<string, CommandInfo> GetCommands(string module)
        {
            var par = InvokeScript($"(Get-Module -Name {module}).ExportedCommands");

            if (par.Count <= 0 || par[0] == null)
                return null;

            var meta = par[0].BaseObject as IDictionary<string, CommandInfo>;
            return meta;
        }

        /// <summary>
        /// Returns information from a single PowerShell command.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>CommandInfo with information for the command.</returns>
        public CommandInfo GetCommand(string command)
        {
            Collection<PSObject> par = InvokeScript($"Get-Command -Name {command}");
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].BaseObject as CommandInfo;
        }

        /// <summary>
        /// Returns a list of parameters a single command accepts.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>IDictionary with a key containing the parameter name and a value containing the metadata.</returns>
        public IDictionary<string, ParameterMetadata> GetParameters(string command)
        {
            var commandMeta = GetCommand(command);
            if (command == null)
                return null;

            var meta = commandMeta.Parameters as IDictionary<string, ParameterMetadata>;
            return meta;
        }

        /// <summary>
        /// Returns help for a single PowerShell command.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>Preformatted string containing the help for the command.</returns>
        public string GetCommandHelp(string command)
        {
            var par = InvokeScript($"Get-Help -Name {command} | Out-String");
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].ToString();
        }

        /// <summary>
        /// Returns help for a single parameter of a PowerShell command.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <param name="parameter">String containing the name of the parameter.</param>
        /// <returns>Preformatted string containing the help for the parameter.</returns>
        public string GetParameterHelp(string command, string parameter)
        {
            var par = InvokeScript($"Get-Help -Name {command} -Parameter {parameter} | Out-String");
            return par.Count <= 0 ? null : par[0].ToString();
        }

        /// <summary>
        /// Appends text to the console, scrolls to the end of the console,
        /// and moves the input starting position to the very end.
        /// </summary>
        /// <param name="text">Text to append.</param>
        private void AppendScroll(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                internalShell.AppendText(text);
                internalShell.ScrollToEnd();
            }));
            //inputStartPos = internalShell.Document.ContentEnd.GetPositionAtOffset(0);
        }

        /// <summary>
        /// Updates the location of the current working directory.
        /// </summary>
        private void UpdateLocation()
        {
            _offset = 0;

            var locations = InvokeInternal("Get-Location");
            if (locations[0].BaseObject is PathInfo path) AppendScroll($"\rCS {path.Path}> ");
        }

        /// <summary>
        /// Invokes a script internally, creating a new runspace.
        /// </summary>
        /// <param name="script">Script to invoke.</param>
        /// <returns>A Collection containing objects the command returned.</returns>
        private Collection<PSObject> InvokeInternal(string script)
        {
            Collection<PSObject> result;

            if (_powerShell == null)
            {
                _powerShell = PowerShell.Create();
            }

            _powerShell.Streams.Debug.DataAdded += Debug_DataAdded;
            _powerShell.Streams.Error.DataAdded += Error_DataAdded;
            _powerShell.Streams.Progress.DataAdded += Progress_DataAdded;
            _powerShell.Streams.Verbose.DataAdded += Verbose_DataAdded;
            _powerShell.Streams.Warning.DataAdded += Warning_DataAdded;

            _powerShell.AddScript(script);

            try
            {
                result = _powerShell.Invoke();
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

        

        public Collection<PSObject> InvokeScript(string script)
        {
            Collection<PSObject> output;

            // lock to prevent two commands from being executed at once in the same runspace
            lock (_threadLock)
            {
                IsExecuting = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar.IsIndeterminate = true;
                }));

                AppendScroll(script);
                output = InvokeInternal(script);
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
            _powerShell.Dispose();
        }

        private void InternalShell_KeyDown(object sender, KeyEventArgs e)
        {
            var isReadOnly = _offset <= 0;

            // Supress backspace since the area is read-only.
            if (isReadOnly && e.Key == Key.Back)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                _offset--;
                return;
            }

            // Ignore offsets for selection keys.
            if (e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Up ||
                e.Key == Key.Down ||
                (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))) return;

            // Execute, if the return key is pressed
            if (e.Key == Key.Return)
            {
                var input = new TextRange(internalShell.Document.ContentEnd.GetPositionAtOffset(-_offset), internalShell.Document.ContentEnd.GetPositionAtOffset(0)).Text
                    .Replace("\r", string.Empty).Replace("\r\n", string.Empty);
                AppendScroll("\r");

                InvokeInternal(input);

                UpdateLocation();
                return;
            }

            _offset++;
        }

        public void SetReadOnly(bool readOnly)
        {
            internalShell.IsReadOnly = readOnly;
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var newRecord = ((PSDataCollection<WarningRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var newRecord = ((PSDataCollection<VerboseRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var newRecord = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.ErrorDetails);
        }

        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            var newRecord = ((PSDataCollection<DebugRecord>)sender)[e.Index];
            AppendScroll("\n" + newRecord.Message);
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            var newRecord = ((PSDataCollection<ProgressRecord>)sender)[e.Index];
        }
    }
}
