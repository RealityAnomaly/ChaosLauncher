using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Ribbon;

namespace SourceLauncher
{
    public partial class MainWindow : RibbonWindow
    {
        private Workspace currentWorkspace;
        private void OpenWorkspace(string fileName)
        {
            Workspace workspace = Workspace.Open(fileName);

            if (workspace == null)
            {
                MessageBox.Show("Failed to load the workspace. Please see the log for more information.", "Workspace", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenWorkspace(workspace);
        }

        private void OpenWorkspace(Workspace workspace)
        {
            if (chaosShell.IsExecuting)
            {
                MessageBox.Show((string)Application.Current.FindResource("chaosShellExecuting"), "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (currentWorkspace != null && workspace.Identifier != currentWorkspace.Identifier)
                CloseWorkspace();

            currentWorkspace = workspace;

            Task.Run(() =>
            {
                chaosShell.InvokeScript(String.Format("Set-VWorkspace -Path \"{0}\"", new FileInfo(currentWorkspace.LoadedPath).Directory.FullName));
            });

            workspaceTab.Visibility = Visibility.Visible;
            saveWorkspaceBtn.IsEnabled = true;
            closeWorkspaceBtn.IsEnabled = true;

            if (workspace.PerforceEnabled)
            {
                p4group.Visibility = Visibility.Visible;
            }
            else
            {
                p4group.Visibility = Visibility.Hidden;
            }

            Title = String.Format("Chaos Launcher - {0}", currentWorkspace.Name);
        }

        private bool CloseWorkspace()
        {
            if (chaosShell.IsExecuting)
            {
                MessageBox.Show((string)Application.Current.FindResource("chaosShellExecuting"), "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (currentWorkspace.UnsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("Save changes to the workspace?", "Workspace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                switch(result)
                {
                    case MessageBoxResult.Yes:
                        SaveWorkspace();
                        break;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }

            currentWorkspace = null;

            Task.Run(() =>
            {
                chaosShell.InvokeScript("Set-VWorkspace -Clear");
            });
            
            workspaceTab.Visibility = Visibility.Hidden;
            saveWorkspaceBtn.IsEnabled = false;
            closeWorkspaceBtn.IsEnabled = false;
            Title = "Chaos Launcher";

            return true;
        }

        private void SaveWorkspace()
        {
            currentWorkspace.Save();
            Title = Title.TrimEnd('*');
        }

        private void SetUnsaved()
        {
            currentWorkspace.UnsavedChanges = true;
            Title = Title + "*";
        }
    }
}
