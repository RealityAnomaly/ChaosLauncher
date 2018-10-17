using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SourceLauncher
{
    partial class MainWindow
    {
        /// <summary>
        /// Runs the Source Engine with the specified parameter set.
        /// </summary>
        private void RunEngine(ICollection<string> parameters = null)
        {
            if (parameters == null)
                parameters = new List<string>();

            if (!_currentWorkspace.PerforceEnabled)
                parameters.Add("-nop4");

            // TODO: this should be customisable!
            parameters.Add("-windowed");
            parameters.Add("-width 1280");
            parameters.Add("-height 720");

            if (_currentWorkspace.LaunchParameters != null)
            {
                var optionalParameters = _currentWorkspace.LaunchParameters.Split('\n');
                parameters = parameters.Concat(optionalParameters).ToList();
            }

            var realParameters = string.Join(" ", parameters);

            Process.Start($"steam://rungameid/{_currentWorkspace.SourceGame.AppId}//{realParameters}");
            //RunProcessWithSpecialEnv(_currentWorkspace.SourceGame.AppExecutable);
        }

        /// <summary>
        /// Runs the specified engine tool.
        /// </summary>
        private void RunEngineTool(string tool)
        {
            ICollection<string> parameters = new List<string> { "-tools", $"-game \"{_currentWorkspace.SourceGame.ContentDir}\"", $"+toolload {tool}", "+disconnect" };
            //ICollection<string> parameters = new List<string> { "-tools", $"-game \"{_currentWorkspace.SourceGame.ContentDir}\"", $"+toolload {tool}" };
            if (tool == "hammer_dll")
                parameters.Add("-foundrymode");
            RunEngine(parameters);
        }

        /// <summary>
        /// Runs the app-specific workshop publishing tool.
        /// </summary>
        private void RunPublisher()
        {
            if (_currentWorkspace.SourceGame.AppId.Equals("362890"))
            {
                RunTool("blackmesa_publish");
            }
            else if (_currentWorkspace.SourceGame.AppId.Equals("620"))
            {
                RunTool("p2map_publish");
            }
            else
            {
                MessageBox.Show("This engine branch does not support the publishing tool.", "Source SDK", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Runs a conventional executable tool.
        /// </summary>
        private void RunTool(string tool)
        {
            try
            {
                RunProcessWithSpecialEnv($@"{_currentWorkspace.SourceGame.ContentDir}\..\bin\{tool}.exe");
            }
            catch
            {
                MessageBox.Show("This engine branch does not support this tool and/or it is missing from the bin directory.", "Source SDK", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RunProcessWithSpecialEnv(string fileName)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            };

            info.EnvironmentVariables["VProject"] = _currentWorkspace.SourceGame.ContentDir;

            // Set Perforce env variables
            if (_currentWorkspace.PerforceEnabled)
            {
                info.EnvironmentVariables["P4PORT"] = _currentWorkspace.PerforceServer;
                info.EnvironmentVariables["P4USER"] = _currentWorkspace.PerforceUser;
                info.EnvironmentVariables["P4CLIENT"] = _currentWorkspace.PerforceClient;
                info.EnvironmentVariables["P4CHARSET"] = _currentWorkspace.PerforceCharset;
            }

            var process = new Process
            {
                StartInfo = info
            };

            process.Start();
        }

        private void LaunchSource_OnClick(object sender, RoutedEventArgs e) => RunEngine();

        private void OpenHammer_OnClick(object sender, RoutedEventArgs e) => RunTool("hammer");
        private void OpenHlmv_OnClick(object sender, RoutedEventArgs e) => RunTool("hlmv");
        private void OpenModelBrowser_OnClick(object sender, RoutedEventArgs e) => RunTool("modelbrowser");
        private void OpenQCEyes_OnClick(object sender, RoutedEventArgs e) => RunTool("QC_Eyes");
        private void OpenFacePoser_OnClick(object sender, RoutedEventArgs e) => RunTool("hlfaceposer");
        private void PublishMap_OnClick(object sender, RoutedEventArgs e) => RunPublisher();
        private void OpenSceneManager_OnClick(object sender, RoutedEventArgs e) => RunTool("scenemanager");
        private void OpenSceneViewer_OnClick(object sender, RoutedEventArgs e) => RunTool("sceneviewer");
        private void OpenElementViewer_OnClick(object sender, RoutedEventArgs e) => RunTool("elementviewer");
        private void OpenFoundry_OnClick(object sender, RoutedEventArgs e) => RunEngineTool("hammer_dll");
        private void OpenVMTEditor_OnClick(object sender, RoutedEventArgs e) => RunEngineTool("vmt");
        private void OpenParticleEditor_OnClick(object sender, RoutedEventArgs e) => RunEngineTool("pet");
        private void OpenCommentaryEditor_OnClick(object sender, RoutedEventArgs e) => RunEngineTool("commedit");

        private void OpenWorkingDir_OnClick(object sender, RoutedEventArgs e) =>
            Process.Start("explorer.exe", $"/open, {_currentWorkspace.Folder}");

        private void SetLaunchParams_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
