using SourceLauncher.Controls;
using SourceLauncher.Models;
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
using static SourceLauncher.Models.Parameter;

namespace SourceLauncher.Windows
{
    /// <summary>
    /// Interaction logic for ToolControlWindow.xaml
    /// </summary>
    public partial class ToolControlWindow : Window
    {
        public Tool Tool { get; private set; }
        private ChaosShell shell;
        private Canvas canvas;
        private Parameter selectedParam;
        private ObservableCollection<Parameter> availableParameters = new ObservableCollection<Parameter>();
        public ToolControlWindow(ChaosShell shell, Canvas canvas, Tool tool, int tabIndex = 0)
        {
            this.shell = shell;
            this.canvas = canvas;
            this.Tool = tool;
            InitializeComponent();

            tabControl.TabIndex = tabIndex;
            availableList.ItemsSource = availableParameters;

            staticButton.Click += delegate { selectedParam.ParamMode = ParameterMode.Content; };
            connectionButton.Click += delegate { selectedParam.ParamMode = ParameterMode.Reference; };
            variableButton.Click += delegate { selectedParam.ParamMode = ParameterMode.Variable; };
            switchButton.Click += delegate { selectedParam.ParamMode = ParameterMode.Switch; };

            RefreshTool();
        }

        private void SetLinkDestination(Guid guid)
        {

        }

        private void RefreshTool()
        {
            Title = String.Format("Tool Properties - {0}", Tool.ToString());
            mainGrid.DataContext = Tool;

            configuredList.SelectedItems.Clear();
            configuredList.ItemsSource = null;

            availableList.SelectionChanged += AvailableList_SelectionChanged;
            configuredList.SelectionChanged += ConfiguredList_SelectionChanged;

            if (Tool.GetType() == typeof(CmdletTool))
            {
                cmdletBtn.IsChecked = true;
                CmdletTool ctool = Tool as CmdletTool;

                if (ctool.Metadata != null)
                {
                    UpdateAvailable(ctool.Metadata.Parameters);
                } else
                {
                    Task.Run(() =>
                    {
                        IDictionary<string, ParameterMetadata> info = shell.GetParameters(ctool.Name);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            UpdateAvailable(info);
                        }));
                    });
                }
            }
            else if (Tool.GetType() == typeof(ScriptTool))
            {
                scriptBtn.IsChecked = true;
            }
            else if (Tool.GetType() == typeof(ExternalTool))
            {
                externalBtn.IsChecked = true;
            }
        }

        private void UpdateAvailable(IDictionary<string, ParameterMetadata> info)
        {
            availableParameters.Clear();

            foreach (KeyValuePair<string, ParameterMetadata> m in info)
            {
                if (m.Key != null)
                    availableParameters.Add(new Parameter(m.Value));
            }

            foreach (Parameter par in Tool.Parameters)
            {
                availableParameters.Remove(par);
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
            if (availableList.SelectedItems.Count > 0)
            {
                addBtn.IsEnabled = true;
            }
            else
            {
                addBtn.IsEnabled = false;
            }
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

            foreach (RadioButton btn2 in buttons)
            {
                if (btn == btn2)
                {
                    btn2.IsChecked = true;
                }
                else
                {
                    btn2.IsChecked = false;
                }
            }
        }

        private void SetButtonMode(ParameterMode mode)
        {
            if (!(mode == ParameterMode.Switch))
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
            selectedParam = param;

            paramGroup.IsEnabled = true;
            shellGroup.IsEnabled = true;

            SetButtonMode(param.ParamMode);

            if (selectedParam.Content.Count > 0)
                staticTextBox.Text = selectedParam.ContentToString();

            if (selectedParam.Reference != null)
                connectionTextBox.Text = selectedParam.ReferenceToString();

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
                            param.Help = shell.GetParameterHelp(Tool.Name, param.ToString());

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
            selectedParam = null;

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
                availableParameters.Remove(itm);
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
                availableParameters.Add(itm);
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
            foreach (UIElement element in canvas.Children)
            {
                if (!(element is ToolControl toolControl) || toolControl.Tool.Identifier == Tool.Identifier)
                    continue;

                tools.Add(toolControl.Tool);
            }

            ToolConnectionWindow connectionWindow = new ToolConnectionWindow(tools, Tool);
            connectionWindow.ShowDialog();

            selectedParam.Reference = connectionWindow.Reference;
            connectionTextBox.Text = selectedParam.ReferenceToString();
        }

        private void StaticTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            selectedParam?.SetContent(staticTextBox.Text);
        }

        private void SetTool_Click(object sender, RoutedEventArgs e)
        {
            if (Tool.GetType() == typeof(CmdletTool))
                SetToolCmdlet();
            else if (Tool.GetType() == typeof(ScriptTool))
                SetToolScript();
            else if (Tool.GetType() == typeof(ExternalTool))
                SetToolExternal();
        }

        private void SetToolCmdlet()
        {
            MessageBoxResult result = MessageBox.Show("Changing the cmdlet will remove all parameters and references. Are you sure you want to do this?", "Tool Configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            CmdletTool newTool = CmdletTool.PickTool(shell);
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
