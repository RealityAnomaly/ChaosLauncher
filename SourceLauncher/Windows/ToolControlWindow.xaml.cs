using SourceLauncher.Controls;
using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PoshCode;
using SourceLauncher.Utilities;
using static SourceLauncher.Models.Parameter;

namespace SourceLauncher.Windows
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for ToolControlWindow.xaml
    /// </summary>
    public partial class ToolControlWindow
    {
        private readonly PoshConsole _shellHost;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<Parameter> _availableParameters = new ObservableCollection<Parameter>();
        private Parameter _selectedParam;
        public Tool Tool { get; private set; }
        public ToolControlWindow(PoshConsole shellHost, Canvas canvas, Tool tool, int tabIndex = 0)
        {
            _shellHost = shellHost;
            _canvas = canvas;
            Tool = tool;
            InitializeComponent();

            tabControl.TabIndex = tabIndex;
            availableList.ItemsSource = _availableParameters;

            staticButton.Click += delegate { _selectedParam.ParamMode = ParameterMode.Content; };
            connectionButton.Click += delegate { _selectedParam.ParamMode = ParameterMode.Reference; };
            variableButton.Click += delegate { _selectedParam.ParamMode = ParameterMode.Variable; };
            switchButton.Click += delegate { _selectedParam.ParamMode = ParameterMode.Switch; };

            RefreshTool();
        }

        private void RefreshTool()
        {
            Title = $"Tool Properties - {Tool}";
            mainGrid.DataContext = Tool;

            configuredList.SelectedItems.Clear();
            configuredList.ItemsSource = null;

            availableList.SelectionChanged += AvailableList_SelectionChanged;
            configuredList.SelectionChanged += ConfiguredList_SelectionChanged;

            if (Tool.GetType() == typeof(CmdletTool))
            {
                cmdletBtn.IsChecked = true;
                if (!(Tool is CmdletTool ctool))
                    throw new Exception("Could not cast Tool to CmdletTool.");

                if (ctool.ReadOnly)
                {
                    SetTool.IsEnabled = false;
                    Nickname.IsEnabled = false;
                }


                if (ctool.Metadata != null)
                {
                    UpdateAvailable(ctool.Metadata.Parameters);
                }
                else
                {
                    Task.Run(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var info = _shellHost.GetParameters(ctool.Name);
                            UpdateAvailable(info);
                        }));
                    });
                }
            }
            else if (Tool.GetType() == typeof(ScriptTool))
            {
                scriptBtn.IsChecked = true;
            }
            else if (Tool is ExternalTool)
            {
                externalBtn.IsChecked = true;
            }
        }

        private void UpdateAvailable(IDictionary<string, ParameterMetadata> info)
        {
            _availableParameters.Clear();

            foreach (KeyValuePair<string, ParameterMetadata> m in info)
            {
                if (m.Key != null)
                    _availableParameters.Add(new Parameter(m.Value));
            }

            foreach (Parameter par in Tool.Parameters)
            {
                _availableParameters.Remove(par);
            }

            configuredList.ItemsSource = Tool.Parameters;
        }

        private void ConfiguredList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (configuredList.SelectedItems.Count == 1)
            {
                SelectParam(configuredList.SelectedItems[0] as Parameter);
                removeBtn.IsEnabled = true;
            }
            else if (configuredList.SelectedItems.Count > 1)
            {
                DeselectParam();
                removeBtn.IsEnabled = true;
            }
            else
            {
                DeselectParam();
                removeBtn.IsEnabled = false;
            }
        }

        private void AvailableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            addBtn.IsEnabled = availableList.SelectedItems.Count > 0;
        }

        private void SelectRadioButton(RadioButton btn)
        {
            IList<RadioButton> buttons = new List<RadioButton>()
            {
                staticButton,
                connectionButton,
                variableButton,
                switchButton,
            };

            foreach (var btn2 in buttons)
            {
                btn2.IsChecked = btn == btn2;
            }
        }

        private void SetButtonMode(ParameterMode mode)
        {
            if (mode != ParameterMode.Switch)
            {
                // If not a switch, force the switch button to be disabled, and make sure others are enabled
                staticButton.IsEnabled = true;
                connectionButton.IsEnabled = true;
                variableButton.IsEnabled = true;
                switchButton.IsEnabled = false;
            }

            switch (mode)
            {
                case ParameterMode.Content:
                    SelectRadioButton(staticButton);
                    break;
                case ParameterMode.Reference:
                    SelectRadioButton(connectionButton);
                    break;
                case ParameterMode.Variable:
                    SelectRadioButton(variableButton);
                    break;
                case ParameterMode.Switch:
                    // If the param is a switch, force radio button to switch only
                    staticButton.IsEnabled = false;
                    connectionButton.IsEnabled = false;
                    variableButton.IsEnabled = false;
                    switchButton.IsEnabled = true;

                    SelectRadioButton(switchButton);
                    break;
            }
        }

        private void SelectParam(Parameter param)
        {
            _selectedParam = param;

            paramGroup.IsEnabled = true;
            shellGroup.IsEnabled = true;

            SetButtonMode(param.ParamMode);

            if (_selectedParam.Content.Count > 0)
                staticTextBox.Text = _selectedParam.ContentToString();

            if (_selectedParam.Reference != null)
                connectionTextBox.Text = _selectedParam.ReferenceToString();

            if (param.Help != null)
            {
                helpBlock.Text = param.Help;
            }
            else
            {
                if (Tool.GetType() == typeof(CmdletTool))
                {
                    Task.Run(() =>
                    {
                        lock (Tool)
                        {
                            param.Help = _shellHost.GetParameterHelp(Tool.Name, param.ToString());

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                helpBlock.Text = param.Help;

                                if (param.Help == null)
                                    param.Help = "No help is available for this command.";
                            }));
                        }
                    });
                }
            }
        }
        private void DeselectParam()
        {
            _selectedParam = null;

            // Clear all buttons and fields, and lock groups
            SelectRadioButton(null);

            paramGroup.IsEnabled = false;
            shellGroup.IsEnabled = false;

            staticTextBox.Text = String.Empty;
            connectionTextBox.Text = String.Empty;
            variableTextBox.Text = String.Empty;

            helpBlock.Text = String.Empty;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            IList<Parameter> toBeRemoved = new List<Parameter>();

            foreach (Parameter itm in availableList.SelectedItems)
            {
                toBeRemoved.Add(itm);
                Tool.Parameters.Add(itm);
            }

            foreach (Parameter itm in toBeRemoved)
            {
                _availableParameters.Remove(itm);
            }

            availableList.SelectedItems.Clear();
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            DeselectParam();

            IList<Parameter> toBeRemoved = new List<Parameter>();

            foreach (Parameter itm in configuredList.SelectedItems)
            {
                toBeRemoved.Add(itm);
                _availableParameters.Add(itm);
            }

            foreach (Parameter itm in toBeRemoved)
            {
                Tool.Parameters.Remove(itm);
            }

            configuredList.SelectedItems.Clear();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            IList<Tool> tools = new List<Tool>();
            foreach (UIElement element in _canvas.Children)
            {
                if (!(element is ToolControl toolControl) || toolControl.Tool.Identifier == Tool.Identifier)
                    continue;

                tools.Add(toolControl.Tool);
            }

            var connectionWindow = new ToolConnectionWindow(tools, Tool);
            connectionWindow.ShowDialog();

            if (connectionWindow.Reference == null)
                return;

            _selectedParam.Reference = connectionWindow.Reference;
            connectionTextBox.Text = _selectedParam.ReferenceToString();
        }

        private void StaticTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _selectedParam?.SetContent(staticTextBox.Text);
        }

        private void SetTool_Click(object sender, RoutedEventArgs e)
        {
            switch (Tool)
            {
                case CmdletTool _:
                    SetToolCmdlet();
                    break;
                case ScriptTool _:
                    SetToolScript();
                    break;
                case ExternalTool _:
                    SetToolExternal();
                    break;
            }
        }

        private void SetToolCmdlet()
        {
            var result = MessageBox.Show("Changing the cmdlet will remove all parameters and references. Are you sure you want to do this?", "Tool Configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var newTool = CmdletTool.PickTool(_shellHost);
            if (newTool == null)
                return;

            if (Tool != null && Tool.ToString() != Tool.Nickname)
                newTool.Nickname = Tool.Nickname;

            Tool = newTool;
            RefreshTool();
        }

        private void SetToolScript()
        {

        }

        private void SetToolExternal()
        {
            ExternalTool newTool = ExternalTool.PickTool();
            if (newTool == null)
                return;

            Tool.Name = newTool.Name;
            Tool.Nickname = newTool.Nickname;

            RefreshTool();
        }
    }
}
