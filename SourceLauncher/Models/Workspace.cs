using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class Workspace
    {
        // State parameters
        [JsonIgnore]
        public bool UnsavedChanges { get; set; }
        [JsonIgnore]
        public bool UnappliedChanges { get; set; }
        [JsonIgnore]
        public string LoadedPath;

        // hack?
        [JsonIgnore]
        public bool PerforceNotEnabled => !PerforceEnabled;

        // Saved parameters
        public string Name { get; set; } = "Unnamed Workspace";
        public Guid Identifier { get; private set; } = Guid.NewGuid();

        // Path configuration
        public string Folder { get; set; }

        // Source configuration
        public SourceGame SourceGame { get; set; }
        public string LaunchParameters { get; set; }

        // Perforce

        public bool PerforceEnabled { get; set; } = false;
        public string PerforceServer { get; set; } = string.Empty;
        public string PerforceUser { get; set; } = string.Empty;
        public string PerforceClient { get; set; } = string.Empty;
        public string PerforceCharset { get; set; } = string.Empty;

        /// <summary>
        /// Opens a workspace from a folder path.
        /// If the workspace does not exist, this will virtually create it.
        /// </summary>
        public static Workspace Open(string path)
        {
            Workspace workspace = null;

            //if (!Directory.Exists(path))
            //return null;

            if (!File.Exists(path))
            {
                workspace = new Workspace();
                File.WriteAllText(path, JsonConvert.SerializeObject(workspace));
            }
            else
            {
                workspace = JsonConvert.DeserializeObject<Workspace>(File.ReadAllText(path));
            }

            workspace.LoadedPath = path;
            workspace.Folder = new FileInfo(workspace.LoadedPath).Directory?.FullName;
            return workspace;
        }

        public void Save()
        {
            File.WriteAllText(LoadedPath, JsonConvert.SerializeObject(this));
            UnsavedChanges = false;
        }
    }
}
