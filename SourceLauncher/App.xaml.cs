using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SourceLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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
