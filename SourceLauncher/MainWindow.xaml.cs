using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SourceLauncher.Services;
using SourceLauncher.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SourceLauncher.Models.Ipc;
using ZetaIpc.Runtime.Server;

namespace SourceLauncher
{
    /// <inheritdoc cref="Window" />
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

            // Init IPC server
            var ipcServer = new IpcServer();
            ipcServer.Start(App.IpcPort);

            // Register the services to the container
            services.AddSingleton(loggerFactory);
            services.AddSingleton(ipcServer);
            services.AddSingleton(typeof(SteamService));
            services.AddSingleton(typeof(PerforceService));
            _serviceProvider = services.BuildServiceProvider();

            // Prepare services for use by the main window
            _steamService = _serviceProvider.GetService<SteamService>();
            _perforceService = _serviceProvider.GetService<PerforceService>();
            var ipcServerTmp = _serviceProvider.GetService<IpcServer>();

            // Register event handlers
            ipcServerTmp.ReceivedRequest += IpcServer_ReceivedRequest;
            FileSelector.FileOpened += FileSelector_FileOpened;

            if (_perforceService.P4Exists)
            {
                P4V.IsEnabled = true;
                P4Merge.IsEnabled = true;
                P4Admin.IsEnabled = true;
            }
            
            // Handle command line arguments
            HandleArguments(Environment.GetCommandLineArgs().Skip(1));
        }

        private void FileSelector_FileOpened(string filePath)
        {
            // Check if we can handle this internally,
            // but if not, handle with the OS's default handler
            if (!HandleFileOpen(filePath))
                Process.Start(filePath);
        }

        /// <summary>
        /// Handles opening of files in the workspace internally.
        /// </summary>
        private bool HandleFileOpen(string path)
        {
            var success = true;

            var ext = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(ext?.Trim()))
                return false;
            ext = ext.Substring(1);

            switch (ext)
            {
                case "vmf":
                case "vmm":
                    // testing only
                    RunTool("hammer.exe", path);
                    break;
                default:
                    // We couldn't handle the open internally
                    success = false;
                    break;
            }

            return success;
        }

        /// <summary>
        /// Handles arguments recieved from
        /// the command line for this application.
        /// </summary>
        private void HandleArguments(IEnumerable<string> args)
        {
            var i = 0;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "/test":
                        MessageBox.Show("ipc argument test");
                        break;
                    default:
                        // if this is the first argument, 
                        // treat it as file open instead of a switch
                        if (i == 0)
                        {
                            if (!HandleFileOpen(arg))
                                MessageBox.Show("Chaos Launcher is unable to handle this file type.", "Chaos Launcher",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                            
                        break;
                }

                i++;
            }
        }
        
        /// <summary>
        /// Handles requests from the IPC server,
        /// for things like opening in an existing process.
        /// </summary>
        private void IpcServer_ReceivedRequest(object sender, ReceivedRequestEventArgs e)
        {
            // Handle our IPC message
            var message = JsonConvert.DeserializeObject<IpcMessage>(e.Request);
            switch (message.Name)
            {
                // We have arguments from a starting process instance
                case "ProcStartArgs":
                    var args = message.Payload as JArray;
                    var strArgs = args?.Select(i => (string) i).ToArray();

                    // Bring our window to focus
                    //Dispatcher.Invoke(() => { Activate(); });
                    HandleArguments(strArgs);
                    break;
                default:
                    throw new InvalidOperationException("Invalid IPC message type!");
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

        private void Preferences_OnClick(object sender, RoutedEventArgs e)
        {
            Window preferencesWindow = new SettingsWindow(_serviceProvider);
            preferencesWindow.ShowDialog();
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
