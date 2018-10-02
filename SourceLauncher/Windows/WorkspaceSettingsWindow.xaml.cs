using SourceLauncher.Controls;
using SourceLauncher.Models;
using SourceLauncher.Services;
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
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using Perforce.P4;

namespace SourceLauncher.Windows
{
    /// <summary>
    /// Interaction logic for WorkspaceSettingsWindow.xaml
    /// </summary>
    public partial class WorkspaceSettingsWindow : Window
    {
        private IServiceProvider serviceProvider;
        private ChaosShell chaosShell;
        private Workspace workspace;

        private PerforceService perforce;
        public WorkspaceSettingsWindow(IServiceProvider serviceProvider, ChaosShell chaosShell, Workspace workspace)
        {
            this.serviceProvider = serviceProvider;
            this.chaosShell = chaosShell;
            this.workspace = workspace;
            workspace.UnappliedChanges = false;

            InitializeComponent();
            mainGrid.DataContext = workspace;

            perforce = serviceProvider.GetService<PerforceService>();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (workspace.UnsavedChanges)
            {
                workspace.Save();
                workspace.UnappliedChanges = true;
            }

            Close();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            workspace.Save();
            workspace.UnappliedChanges = true;

            applyBtn.IsEnabled = false;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Setting_Modified(object sender, RoutedEventArgs e)
        {
            workspace.UnsavedChanges = true;
            workspace.UnappliedChanges = true;

            applyBtn.IsEnabled = true;
        }

        private void TestPerforce_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Repository repo = perforce.Connect(workspace.PerforceServer, workspace.PerforceUser, workspace.PerforceClient);
                ServerMetaData info = repo.GetServerMetaData(null);

                MessageBox.Show("Connected to Perforce successfully.\nServer version: " + info.Version.Major + "\nServer IP: " + info.Address.Uri, "Perforce", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection to the Perforce server failed: " + ex.Message, "Perforce", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
