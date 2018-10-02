using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using SourceLauncher.Models;
using SourceLauncher.Services;
using SourceLauncher.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SourceLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private IServiceProvider serviceProvider;
        private SteamService steamService;
        private PerforceService perforceService;
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
            serviceProvider = services.BuildServiceProvider();

            // Prepare services for use by the main window
            steamService = serviceProvider.GetService<SteamService>();
            perforceService = serviceProvider.GetService<PerforceService>();

            if (perforceService.p4Exists)
            {
                p4v.IsEnabled = true;
                p4merge.IsEnabled = true;
                p4admin.IsEnabled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (currentWorkspace != null && !CloseWorkspace())
                e.Cancel = true;
        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            SetUnsaved();
        }

        private void P4open_Click(object sender, RoutedEventArgs e)
        {
            perforceService.RunP4Tool("p4v");
        }

        private void P4merge_Click(object sender, RoutedEventArgs e)
        {
            perforceService.RunP4Tool("p4merge");
        }

        private void P4admin_Click(object sender, RoutedEventArgs e)
        {
            perforceService.RunP4Tool("p4admin");
        }

        private void LoadWorkspace_Click(object sender, RoutedEventArgs e)
        {
            using(System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filter = "Workspace Files (*.json)|*.json";
                dialog.FileName = "workspace.json";
                dialog.Title = "Select your workspace.json file.";
                System.Windows.Forms.DialogResult dr = dialog.ShowDialog();

                if (dr != System.Windows.Forms.DialogResult.OK)
                    return;

                OpenWorkspace(dialog.FileName);
            }
        }

        private void NewWorkspace_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog())
            {
                dialog.Filter = "Workspace Files (*.json)|*.json";
                dialog.FileName = "workspace.json";
                dialog.Title = "Save this in your workspace folder.";
                System.Windows.Forms.DialogResult dr = dialog.ShowDialog();

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
            Window workspaceSettingsWindow = new WorkspaceSettingsWindow(serviceProvider, chaosShell, currentWorkspace);
            workspaceSettingsWindow.ShowDialog();

            if (currentWorkspace.UnappliedChanges)
                OpenWorkspace(currentWorkspace);
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
