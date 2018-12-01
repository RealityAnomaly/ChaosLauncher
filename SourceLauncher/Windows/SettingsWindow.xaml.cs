using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SourceLauncher.Services;

namespace SourceLauncher.Windows
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SteamService _steamService;

        public SettingsWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _steamService = serviceProvider.GetService<SteamService>();

            AppsCachedNumber.Content = $"({_steamService.Cache.SourceGames.Count} apps cached)";
        }

        private void InvalidateCache_Click(object sender, RoutedEventArgs e)
        {
            _steamService.InvalidateSteamCache();
        }
    }
}
