using SourceLauncher.Models;
using SourceLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PoshCode;

namespace SourceLauncher.Controls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for ToolControl.xaml
    /// </summary>
    public partial class ToolControl
    {
        private readonly IList<OutputReference> _outputs = new List<OutputReference>();
        private readonly IList<Line> _connectionLines = new List<Line>();
        public Tool Tool { get; private set; }

        private readonly PoshConsole _shellHost;

        public ToolControl(PoshConsole shellHost, Tool tool)
        {
            _shellHost = shellHost;
            Tool = tool;

            InitializeComponent();
            mainGrid.DataContext = Tool;

            AutoSetColor();
        }

        private void AutoSetColor()
        {
            if (Tool.GetType() == typeof(CmdletTool))
            {
                SetImage("cs_icon_lg.png");
                typeImage.Visibility = Visibility.Visible;

                // Dark red gradient
                gradientCol1.Color = Color.FromRgb(153, 30, 30);
                gradientCol2.Color = Color.FromRgb(64, 28, 28);
            }
            else if (Tool.GetType() == typeof(ScriptTool))
            {
                SetImage("ps_icon_lg.png");
                typeImage.Visibility = Visibility.Visible;

                // Blue gradient (powershell)
                gradientCol1.Color = Color.FromRgb(60, 119, 224);
                gradientCol2.Color = Color.FromRgb(1, 36, 86);
            }
            else if (Tool is ExternalTool)
            {
                typeImage.Visibility = Visibility.Hidden;

                // Purple gradient
                gradientCol1.Color = Color.FromRgb(115, 47, 172);
                gradientCol2.Color = Color.FromRgb(73, 29, 89);
            }
        }

        public void SetSelected(bool selected)
        {
            if (!selected)
            {
                AutoSetColor();
                return;
            }

            gradientCol1.Color = Color.FromRgb(171, 187, 214);
            gradientCol2.Color = Color.FromRgb(73, 77, 87);
        }

        private void SetImage(string source)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri("pack://application:,,,/SourceLauncher;component/Images/" + source);
            img.EndInit();

            typeImage.Source = img;
        }

        private void AddConnection(OutputReference connection)
        {
            if (connection.SourceToolId == Tool.Identifier)
                throw new Exception("Tool cannot be referenced by itself.");

            _outputs.Add(connection);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(@"Are you sure you want to delete this action widget? Deleting this widget will remove all its connections and parameters.", "Workspace", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Delete();
        }

        private void Delete()
        {
            var canvas = Parent as Canvas;
            canvas?.Children.Remove(this);
        }

        private void SetParametersBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void SetInputsBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings(1);
        }

        private void SetOutputsBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings(2);
        }

        private void OpenSettings(int tabIndex = 0)
        {
            var canvas = Parent as Canvas;

            var toolControlWindow = new ToolControlWindow(_shellHost, canvas, Tool, tabIndex);
            toolControlWindow.ShowDialog();

            Tool = toolControlWindow.Tool;
            mainGrid.DataContext = Tool;
            UpdateConnections();
        }

        public void UpdateConnections()
        {
            if (!(Parent is Canvas canvas)) throw new InvalidOperationException();

            foreach (var line in _connectionLines)
            {
                canvas.Children.Remove(line);
            }
            
            _connectionLines.Clear();

            IList<ToolControl> linkedControls = new List<ToolControl>();
            foreach(var param in Tool.Parameters.Where(p => p.Reference != null))
            {
                foreach(UIElement obj in canvas.Children)
                {
                    // Ensure the control isn't null and it's not self
                    if (!(obj is ToolControl control) || control == this)
                        continue;

                    if (control.Tool.Identifier == param.Reference.SourceToolId)
                        linkedControls.Add(control);
                }
            }

            if (linkedControls.Count <= 0)
                return;

            var localPoint = TransformToAncestor(canvas).Transform(new Point(0, 0));

            foreach (var control in linkedControls)
            {
                var remotePoint = control.TransformToAncestor(canvas).Transform(new Point(0, 0));

                var line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = localPoint.X ,
                    Y1 = localPoint.Y + (Height / 2),

                    X2 = remotePoint.X + control.Width,
                    Y2 = remotePoint.Y + (control.Height / 2),

                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    StrokeThickness = 2
                };

                _connectionLines.Add(line);
                canvas.Children.Add(line);
            }
        }

        public override string ToString()
        {
            return Tool.Name;
        }
    }
}
