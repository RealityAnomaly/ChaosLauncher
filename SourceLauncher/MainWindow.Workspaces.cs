using SourceLauncher.Models;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SourceLauncher
{
    public partial class MainWindow
    {
        private Workspace _currentWorkspace;
        private void OpenWorkspace(string fileName)
        {
            var workspace = Workspace.Open(fileName);

            if (workspace == null)
            {
                MessageBox.Show("Failed to load the workspace. Please see the log for more information.", "Workspace", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenWorkspace(workspace);
        }

        private void OpenWorkspace(Workspace workspace)
        {
            if (ChaosShell.IsExecuting)
            {
                MessageBox.Show((string)Application.Current.FindResource("chaosShellExecuting"), "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_currentWorkspace != null && workspace.Identifier != _currentWorkspace.Identifier)
                CloseWorkspace();

            _currentWorkspace = workspace;

            Task.Run(() =>
            {
                ChaosShell.InvokeScript(
                    $"Set-VWorkspace -Path \"{new FileInfo(_currentWorkspace.LoadedPath).Directory?.FullName}\"");

                // No longer required since we've embedded this into process start
                /**
                if (_currentWorkspace.SourceGame != null)
                    ChaosShell.InvokeScript(
                        $"Set-VProject -Path \"{_currentWorkspace.SourceGame.ContentDir}\"");*/
            });

            WorkspaceTab.Visibility = Visibility.Visible;
            SaveWorkspaceBtn.IsEnabled = true;
            CloseWorkspaceBtn.IsEnabled = true;
            Title = $"Chaos Launcher - {_currentWorkspace.Name}";

            // Show tools relating to the Source game,
            // only if the user has actually defined one in Workspace Options.
            if (workspace.SourceGame != null)
            {
                EngineBranch.IsEnabled = true;
                ToolsTab.Visibility = Visibility.Visible;
            }

            // Show Perforce version control if P4 is enabled on the workspace.
            P4Group.Visibility = workspace.PerforceEnabled ? Visibility.Visible : Visibility.Hidden;
        }

        private bool CloseWorkspace()
        {
            if (ChaosShell.IsExecuting)
            {
                MessageBox.Show((string)Application.Current.FindResource("chaosShellExecuting"), "ChaosShell", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_currentWorkspace.UnsavedChanges)
            {
                var result = MessageBox.Show("Save changes to the workspace?", "Workspace", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (result != MessageBoxResult.Cancel)
                {
                    if (result == MessageBoxResult.Yes)
                        SaveWorkspace();
                }
                else
                {
                    return false;
                }
            }

            _currentWorkspace = null;

            Task.Run(() =>
            {
                ChaosShell.InvokeScript("Set-VWorkspace -Clear");

                // No longer required since we've embedded this into process start
                //ChaosShell.InvokeScript("Set-VProject -Clear");
            });
            
            WorkspaceTab.Visibility = Visibility.Hidden;
            ToolsTab.Visibility = Visibility.Hidden;
            SaveWorkspaceBtn.IsEnabled = false;
            CloseWorkspaceBtn.IsEnabled = false;
            EngineBranch.IsEnabled = false;

            Title = "Chaos Launcher";

            return true;
        }

        private void SaveWorkspace()
        {
            _currentWorkspace.Save();
            Title = Title.TrimEnd('*');
        }

        private void SetUnsaved()
        {
            _currentWorkspace.UnsavedChanges = true;
            Title = Title + "*";
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
