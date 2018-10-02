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
        public bool UnsavedChanges { get; set; } = false;
        [JsonIgnore]
        public bool UnappliedChanges { get; set; } = false;
        [JsonIgnore]
        public string LoadedPath;

        // hack?
        [JsonIgnore]
        public bool PerforceNotEnabled { get { return !PerforceEnabled; } }

        // Saved parameters
        public string Name { get; set; } = "Unnamed Workspace";
        public Guid Identifier { get; private set; } = Guid.NewGuid();

        // Path configuration
        public string Folder { get; set; }

        // Perforce

        public bool PerforceEnabled { get; set; } = false;
        public string PerforceServer { get; set; } = String.Empty;
        public string PerforceUser { get; set; } = String.Empty;
        public string PerforceClient { get; set; } = String.Empty;
        public string PerforceCharset { get; set; } = String.Empty;

        /// <summary>
        /// Opens a workspace from a folder path.
        /// If the workspace does not exist, this will virtually create it.
        /// </summary>
        public static Workspace Open(string path)
        {
            Workspace workspace = null;

            //if (!Directory.Exists(path))
            //return null;

            if (File.Exists(path))
            {
                workspace = JsonConvert.DeserializeObject<Workspace>(File.ReadAllText(path));
            }
            else
            {
                workspace = new Workspace();
                File.WriteAllText(path, JsonConvert.SerializeObject(workspace));
            }

            workspace.LoadedPath = path;
            workspace.Folder = new FileInfo(workspace.LoadedPath).Directory.FullName;
            return workspace;
        }

        public void Save()
        {
            File.WriteAllText(LoadedPath, JsonConvert.SerializeObject(this));
            UnsavedChanges = false;
        }
    }
}
