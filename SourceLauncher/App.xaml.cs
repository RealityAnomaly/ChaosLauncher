using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using SourceLauncher.Models.Ipc;
using ZetaIpc.Runtime.Client;

namespace SourceLauncher
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int IpcPort = 44720;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Send input to our main process, if running
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                // If we're running more than 1 process, bail, and send input to the main process
                var client = new IpcClient();
                client.Initialize(IpcPort);
                client.Send(JsonConvert.SerializeObject(new IpcMessage {Name = "ProcStartArgs", Payload = e.Args}));

                // Shutdown the process
                Shutdown(0);
                return;
            }

            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(ChaosShellException))
            {
                MessageBox.Show(e.Exception.Message, "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
#if DEBUG
                e.Handled = false;
#else
                MessageBox.Show(e.Exception.Message, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
#endif
            }
        }
    }
}
