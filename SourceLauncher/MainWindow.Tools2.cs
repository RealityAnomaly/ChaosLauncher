using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SourceLauncher
{
    partial class MainWindow
    {
        private void RunEngine2Tool(string toolName)
        {

        }

        private void OpenHammer2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("hammer");
        private void OpenWorkshopManager2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("workshopmanager");
        private void OpenWorkshopAdmin2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("workshopadmin");

        private void OpenAssetBrowser2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("assetbrowser");
        private void OpenModelEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("model_editor");
        private void OpenMaterialEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("met");
        private void OpenParticleEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("pet");
        private void OpenModelDocEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("modeldoc_editor");
        private void OpenSurfacePropertyEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("surfacepropertyeditor");
        private void OpenShaderGraphEditor2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("shadergraph_editor");

        private void OpenFacePoser2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("faceposer");

        private void OpenVConsole2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("vconsole");
        private void OpenSnooper2_OnClick(object sender, RoutedEventArgs e) => RunEngine2Tool("snooper");
    }
}
