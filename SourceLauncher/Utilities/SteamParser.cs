using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Utilities
{
    /// <summary>
    /// This class handles parsing of various Steam and Valve configuration files.
    /// </summary>
    internal class SteamParser
    {
        /// <summary>
        /// Parses a steam.inf file into key value pairs.
        /// </summary>
        public static IDictionary<string, string> ParseSteamInf(IEnumerable<string> lines)
        {
            return lines.Select(line => line.Split('=')).ToDictionary(spl => spl[0], spl => spl[1]);
        }
    }
}
