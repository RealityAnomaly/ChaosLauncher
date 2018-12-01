using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SourceLauncher.Models.Steam
{
    internal class SteamCache
    {
        public readonly IList<string> LibraryPaths = new List<string>();
        public readonly IList<SourceGame> SourceGames = new List<SourceGame>();
        public const string FileName = "steamcache.json";

        [JsonIgnore]
        public bool UpdateRequired;
    }
}
