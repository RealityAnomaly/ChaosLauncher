using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SourceLauncher.Windows
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for ToolConnectionWindow.xaml
    /// </summary>
    public partial class ToolConnectionWindow
    {
        private Tool _currentTool;

        private readonly ObservableCollection<Output> _outputList = new ObservableCollection<Output>();

        public OutputReference Reference { get; private set; }
        public ToolConnectionWindow(IEnumerable<Tool> toolControls, Tool currentTool, OutputReference reference = null)
        {
            InitializeComponent();
            _currentTool = currentTool;

            Reference = reference;
            entityList.ItemsSource = toolControls;
            _outputList.Add(new Output());
            entityOutputs.ItemsSource = _outputList;
        }

        private void EntityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _outputList.Clear();

            if (entityList.SelectedItem == null)
            {
                linkButton.IsEnabled = false;
                entityOutputs.IsEnabled = false;

                return;
            }

            linkButton.IsEnabled = true;
            entityOutputs.IsEnabled = true;

            var ou = new Output();
            _outputList.Add(ou);
            entityOutputs.SelectedItem = ou;
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(entityList.SelectedItem is Tool srcTool)) throw new ArgumentNullException(nameof(srcTool));
            Reference = new OutputReference(srcTool.Identifier, entityOutputs.SelectedItem as Output);

            Close();
        }
    }
}
