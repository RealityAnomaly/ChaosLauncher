using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Services
{
    class SteamService : BaseService
    {
        private VProperty vdf;
        private IList<string> libraryPaths = new List<string>();

        public SteamService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            ReloadAllSteam();
        }

        private void ReloadAllSteam()
        {
            libraryPaths.Clear();
            LoadSteamConfig();
            LoadLibraryPaths();
        }

        private void LoadSteamConfig()
        {
            logger.LogInformation("Loading Steam system configuration.");
            RegistryKey steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (steamKey == null)
            {
                logger.LogError("Steam could not be detected from the registry.");
                return;
            }

            string installDir = (string) steamKey.GetValue("SteamPath");
            logger.LogInformation(String.Format("Found Steam at {0}", installDir));

            libraryPaths.Add(installDir);

            try
            {
                vdf = VdfConvert.Deserialize(File.ReadAllText(String.Format("{0}/config/config.vdf", installDir)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Encountered an exception while reading the VDF. Steam configuration will not be loaded.");
                return;
            }

            logger.LogInformation("Steam configuration loaded successfully.");
        }

        private void LoadLibraryPaths()
        {
            if (vdf == null)
            {
                logger.LogError("VDF is not loaded. Cannot load library paths.");
                return;
            }

            foreach (VProperty key in vdf.Value["Software"]["Valve"]["Steam"])
            {
                if (!key.Key.Contains("BaseInstallFolder_"))
                    continue;

                logger.LogInformation(String.Format("Found additional Steam library at {0}", key.Value.ToString()));
                libraryPaths.Add(key.Value.ToString());
            }
        }
    }
}
