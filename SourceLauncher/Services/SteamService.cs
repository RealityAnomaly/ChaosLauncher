using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using SourceLauncher.Models;
using SourceLauncher.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Services
{
    internal class SteamService : BaseService
    {
        private VProperty _configVdf;
        private readonly IList<string> _libraryPaths = new List<string>();
        public readonly IList<SourceGame> SourceGames = new List<SourceGame>();

        public SteamService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ReloadAllSteam();
        }

        private void ReloadAllSteam()
        {
            _libraryPaths.Clear();

            // Retrieve Steam configuration to find Steam config files and default dir
            LoadSteamConfig();
            // Load library paths from Steam config files
            LoadLibraryPaths();
            // Scan libraries for Source games
            LoadSourceGames();
        }

        private void LoadSteamConfig()
        {
            Logger.LogInformation("Loading Steam system configuration.");
            var steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (steamKey == null)
            {
                Logger.LogError("Steam could not be detected from the registry.");
                return;
            }

            var installDir = (string) steamKey.GetValue("SteamPath");
            Logger.LogInformation($"Found Steam at {installDir}");

            _libraryPaths.Add(Path.Combine(installDir, @"steamapps\common"));

            try
            {
                _configVdf = VdfConvert.Deserialize(File.ReadAllText($@"{installDir}\config\config.vdf"));
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Encountered an exception while reading the VDF. Steam configuration will not be loaded.");
                return;
            }

            Logger.LogInformation("Steam configuration loaded successfully.");
        }

        private void LoadLibraryPaths()
        {
            if (_configVdf == null)
            {
                Logger.LogError("VDF is not loaded. Cannot load library paths.");
                return;
            }

            foreach (var vToken in _configVdf.Value["Software"]["Valve"]["Steam"])
            {
                var key = (VProperty) vToken;
                if (!key.Key.Contains("BaseInstallFolder_"))
                    continue;

                Logger.LogInformation($"Found additional Steam library at {key.Value}");
                _libraryPaths.Add(Path.Combine(key.Value.ToString(), @"steamapps\common"));
            }
        }

        private void LoadSourceGames()
        {
            foreach (var path in _libraryPaths)
            {
                IList<string> steamApps = Directory.GetDirectories(path);
                foreach (var steamApp in steamApps)
                {
                    var game = ScanSteamApp(steamApp);
                    if (game == null) continue;

                    Logger.LogInformation($"Found Source game {game.ProductName} with appID {game.AppId}.");
                    SourceGames.Add(game);
                }
            }
        }

        private static SourceGame ScanSteamApp(string steamApp)
        {
            IList<string> contentDirs = Directory.GetDirectories(steamApp);
            return (from contentDir in contentDirs from contentFile in Directory.GetFiles(contentDir) where Path.GetFileName(contentFile) == ("steam.inf")
                select new SourceGame() {ProductName = Path.GetFileName(steamApp), ContentDir = contentDir, AppId = SteamParser.ParseSteamInf(File.ReadAllLines(contentFile))["appID"]}).FirstOrDefault();
        }
    }
}
