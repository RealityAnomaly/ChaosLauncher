using SourceLauncher.Controls;
using SourceLauncher.Models;
using SourceLauncher.Services;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace SourceLauncher.Windows
{
    /// <summary>
    /// Interaction logic for WorkspaceSettingsWindow.xaml
    /// </summary>
    public partial class WorkspaceSettingsWindow
    {
        private ChaosShell _chaosShell;
        private readonly Workspace _workspace;
        private readonly PerforceService _perforce;
        private readonly SteamService _steam;
        public WorkspaceSettingsWindow(IServiceProvider serviceProvider, ChaosShell chaosShell, Workspace workspace)
        {
            _chaosShell = chaosShell;
            _workspace = workspace;
            workspace.UnappliedChanges = false;

            _perforce = serviceProvider.GetService<PerforceService>();
            _steam = serviceProvider.GetService<SteamService>();

            InitializeComponent();
            MainGrid.DataContext = workspace;
            BranchSelector.ItemsSource = _steam.SourceGames;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_workspace.UnsavedChanges)
            {
                _workspace.Save();
                _workspace.UnappliedChanges = true;
            }

            Close();
        }

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            _workspace.Save();
            _workspace.UnappliedChanges = true;

            ApplyBtn.IsEnabled = false;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Setting_Modified(object sender, RoutedEventArgs e)
        {
            _workspace.UnsavedChanges = true;
            _workspace.UnappliedChanges = true;

            ApplyBtn.IsEnabled = true;
        }

        private void TestPerforce_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var repo = PerforceService.Connect(_workspace.PerforceServer, _workspace.PerforceUser, _workspace.PerforceClient);
                var info = repo.GetServerMetaData(null);

                MessageBox.Show("Connected to Perforce successfully.\nServer version: " + info.Version.Major + "\nServer IP: " + info.Address.Uri, "Perforce", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection to the Perforce server failed: " + ex.Message, "Perforce", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
