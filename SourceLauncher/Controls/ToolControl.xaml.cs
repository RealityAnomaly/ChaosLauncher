using SourceLauncher.Models;
using SourceLauncher.Windows;
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
using System.Windows.Shapes;

namespace SourceLauncher.Controls
{
    /// <summary>
    /// Interaction logic for ToolControl.xaml
    /// </summary>
    public partial class ToolControl : UserControl
    {
        private IList<OutputReference> Outputs = new List<OutputReference>();
        private IList<Line> ConnectionLines = new List<Line>();
        public Tool Tool { get; private set; }

        private ChaosShell chaosShell;

        private enum ConnectionType
        {
            Input,
            Output
        }

        public ToolControl(ChaosShell chaosShell, Tool tool)
        {
            this.chaosShell = chaosShell;
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
            else if (Tool.GetType() == typeof(ExternalTool))
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
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri("pack://application:,,,/SourceLauncher;component/Images/" + source);
            img.EndInit();

            typeImage.Source = img;
        }

        private void AddConnection(OutputReference connection)
        {
            if (connection.SourceToolId == Tool.Identifier)
                throw new Exception("Tool cannot be referenced by itself.");

            Outputs.Add(connection);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this action widget?\nDeleting this widget will remove all its connections and parameters.", "Workspace", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Delete();
        }

        private void Delete()
        {
            Canvas canvas = Parent as Canvas;
            canvas.Children.Remove(this);
        }

        private void SetParametersBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings(0);
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
            Canvas canvas = Parent as Canvas;

            ToolControlWindow toolControlWindow = new ToolControlWindow(chaosShell, canvas, Tool, tabIndex);
            toolControlWindow.ShowDialog();

            Tool = toolControlWindow.Tool;
            mainGrid.DataContext = Tool;
            UpdateConnections();
        }

        public void UpdateConnections()
        {
            Canvas canvas = Parent as Canvas;

            foreach (Line line in ConnectionLines)
            {
                canvas.Children.Remove(line);
            }

            ConnectionLines.Clear();

            IList<ToolControl> linkedControls = new List<ToolControl>();
            foreach(Parameter param in Tool.Parameters.Where(p => p.Reference != null))
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

            Point localPoint = TransformToAncestor(canvas).Transform(new Point(0, 0));

            foreach (ToolControl control in linkedControls)
            {
                Point remotePoint = control.TransformToAncestor(canvas).Transform(new Point(0, 0));

                Line line = new Line
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

                ConnectionLines.Add(line);
                canvas.Children.Add(line);
            }
        }

        public override string ToString()
        {
            return Tool.Name;
        }
    }
}
