using SourceLauncher.Controls;
using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace SourceLauncher.Windows
{
    /// <summary>
    /// Interaction logic for ToolConnectionWindow.xaml
    /// </summary>
    public partial class ToolConnectionWindow : Window
    {
        private IEnumerable<Tool> tools;
        private Tool currentTool;

        private ObservableCollection<Output> outputList = new ObservableCollection<Output>();

        public OutputReference Reference { get; private set; }
        public ToolConnectionWindow(IEnumerable<Tool> toolControls, Tool currentTool, OutputReference reference = null)
        {
            InitializeComponent();
            this.tools = toolControls;
            this.currentTool = currentTool;

            Reference = reference;

            entityList.ItemsSource = toolControls;

            outputList.Add(new Output(null));
            entityOutputs.ItemsSource = outputList;
        }

        private void EntityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            outputList.Clear();

            if (entityList.SelectedItem == null)
            {
                linkButton.IsEnabled = false;
                entityOutputs.IsEnabled = false;

                return;
            }

            linkButton.IsEnabled = true;
            entityOutputs.IsEnabled = true;

            Output ou = new Output(null);
            outputList.Add(ou);
            entityOutputs.SelectedItem = ou;
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            Tool srcTool = entityList.SelectedItem as Tool;
            Reference = new OutputReference(srcTool.Identifier, entityOutputs.SelectedItem as Output);

            Close();
        }
    }
}
