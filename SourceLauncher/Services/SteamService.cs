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
using Newtonsoft.Json;
using SourceLauncher.Models.Steam;

namespace SourceLauncher.Services
{
    internal class SteamService : BaseService
    {
        private VProperty _configVdf;
        public SteamCache Cache;

        public SteamService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ReloadAllSteam();
        }

        private void ReloadAllSteam()
        {
            // Load the Steam cache file
            LoadSteamCache();
            // Retrieve Steam configuration to find Steam config files and default dir
            LoadSteamConfig();

            // Rebuild the cache if out of date
            if (Cache.UpdateRequired)
                RebuildSteamCache();
        }

        private void LoadSteamCache()
        {
            try
            {
                Cache = JsonConvert.DeserializeObject<SteamCache>(
                    File.ReadAllText(SteamCache.FileName));

                Logger.LogInformation("Steam Cache loaded.");
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Could not load the Steam Cache: {e}. Creating a new one.");
                Cache = new SteamCache
                {
                    UpdateRequired = true
                };
            }
        }

        private void SaveSteamCache()
        {
            try
            {
                File.WriteAllText(SteamCache.FileName, JsonConvert.SerializeObject(Cache));
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Could not save the Steam Cache: {e}.");
            }
        }

        public void InvalidateSteamCache()
        {
            File.Delete(SteamCache.FileName);
        }

        private void RebuildSteamCache()
        {
            Logger.LogInformation("Rebuilding Steam cache.");

            // Load library paths from Steam config files
            LoadLibraryPaths();
            // Scan libraries for Source games
            LoadSourceGames();

            // Save the cache.
            SaveSteamCache();
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

            Cache.LibraryPaths.Add(Path.Combine(installDir, @"steamapps\common"));

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
                Cache.LibraryPaths.Add(Path.Combine(key.Value.ToString(), @"steamapps\common"));
            }
        }

        private void LoadSourceGames()
        {
            foreach (var path in Cache.LibraryPaths)
            {
                IList<string> steamApps = Directory.GetDirectories(path);
                foreach (var steamApp in steamApps)
                {
                    var game = ScanSteamApp(steamApp);
                    if (game == null) continue;

                    Logger.LogInformation($"Found Source game {game.ProductName} with appID {game.AppId}.");
                    Cache.SourceGames.Add(game);
                }
            }
        }

        /// <summary>
        /// Scans a Steam App to check if it's a Source game.
        /// </summary>
        private static SourceGame ScanSteamApp(string steamApp, string contentDir = null)
        {
            if (contentDir == null)
                contentDir = steamApp;

            foreach (var file in Directory.GetFiles(contentDir))
            {
                if (Path.GetFileName(file) != "steam.inf") continue;

                // different steam.inf format for Source2
                var appIdName = "appID";
                var isSource2 = IsSource2(contentDir);
                if (isSource2)
                    appIdName = "AppID";

                var game = new SourceGame
                {
                    ProductName = Path.GetFileName(steamApp),
                    ContentDir = contentDir,
                    AppId = SteamParser.ParseSteamInf(File.ReadAllLines(file))[appIdName],
                    IsSource2 = isSource2
                };

                return game;
            }

            return Directory.GetDirectories(contentDir)
                .Select(subFolder => ScanSteamApp(steamApp, subFolder))
                .FirstOrDefault(app => app != null);
        }

        /// <summary>
        /// Scans a Source content directory to see if its engine is Source 2.
        /// </summary>
        private static bool IsSource2(string contentDir)
        {
            var gameDir = Directory.GetParent(contentDir).FullName;
            var binDir = Path.Combine(gameDir, "bin", "win64");

            // no win64 folder so we're definitely not source2
            if (!Directory.Exists(binDir))
                return false;

            // check for the presence of vconsole2.exe in the bin dir. This is a bit of a hack.
            return Directory.GetFiles(binDir)
                .Select(file => file.Contains("vconsole2.exe"))
                .Any(b => b);
        }
    }
}
