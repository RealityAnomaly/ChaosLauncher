using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SourceLauncher.Services;
using SourceLauncher.Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;

namespace SourceLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PerforceService _perforceService;
        private readonly SteamService _steamService;
        public MainWindow()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection();
            ILoggerFactory loggerFactory = new LoggerFactory();
            //loggerFactory.AddEventLog();
            loggerFactory.AddDebug();

            // Register the services to the container
            services.AddSingleton(loggerFactory);
            services.AddSingleton(typeof(SteamService));
            services.AddSingleton(typeof(PerforceService));
            _serviceProvider = services.BuildServiceProvider();

            // Prepare services for use by the main window
            _steamService = _serviceProvider.GetService<SteamService>();
            _perforceService = _serviceProvider.GetService<PerforceService>();

            if (_perforceService.P4Exists)
            {
                P4V.IsEnabled = true;
                P4Merge.IsEnabled = true;
                P4Admin.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_currentWorkspace != null && !CloseWorkspace())
                e.Cancel = true;
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            SetUnsaved();
        }

        private void P4open_Click(object sender, RoutedEventArgs e)
        {
            _perforceService.RunP4Tool("p4v");
        }

        private void P4merge_Click(object sender, RoutedEventArgs e)
        {
            _perforceService.RunP4Tool("p4merge");
        }

        private void P4admin_Click(object sender, RoutedEventArgs e)
        {
            _perforceService.RunP4Tool("p4admin");
        }

        private void LoadWorkspace_Click(object sender, RoutedEventArgs e)
        {
            using(var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filter = @"Workspace Files (*.json)|*.json";
                dialog.FileName = "workspace.json";
                dialog.Title = @"Select your workspace.json file.";
                var dr = dialog.ShowDialog();

                if (dr != System.Windows.Forms.DialogResult.OK)
                    return;

                OpenWorkspace(dialog.FileName);
            }
        }

        private void NewWorkspace_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = @"Workspace Files (*.json)|*.json";
                dialog.FileName = "workspace.json";
                dialog.Title = @"Save this in your workspace folder.";
                var dr = dialog.ShowDialog();

                if (dr != System.Windows.Forms.DialogResult.OK)
                    return;

                OpenWorkspace(dialog.FileName);
            }
        }

        private void CloseWorkspaceBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseWorkspace();
        }

        private void SaveWorkspaceBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveWorkspace();
        }

        private void ConfigureWorkspaceBtn_Click(object sender, RoutedEventArgs e)
        {
            Window workspaceSettingsWindow = new WorkspaceSettingsWindow(_serviceProvider, ChaosShell, _currentWorkspace);
            workspaceSettingsWindow.ShowDialog();

            if (_currentWorkspace.UnappliedChanges)
                OpenWorkspace(_currentWorkspace);
        }

        private void NewChaosShell_Click(object sender, RoutedEventArgs e)
        {
            Window chaosShellWindow = new ChaosShellWindow();
            chaosShellWindow.Show();
        }

        private void AddCmdletWidget_Click(object sender, RoutedEventArgs e)
        {
            NewCmdletWidget();
        }

        private void AddScriptWidget_Click(object sender, RoutedEventArgs e)
        {
            NewScriptWidget();
        }

        private void AddExternalWidget_Click(object sender, RoutedEventArgs e)
        {
            NewExternalWidget();
        }

        private void RibbonWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                StopTargeting();
        }
    }
}
