using System.Collections.Generic;
using System.Management.Automation;
using PoshCode;

namespace SourceLauncher.Utilities
{
    internal static class ChaosShellExtensions
    {
        /// <summary>
        /// Returns a list of commands from a PowerShell module.
        /// </summary>
        /// <param name="shellHost">Shell host.</param>
        /// <param name="module">String containing the name of the module.</param>
        /// <returns>IDictionary containing the command and command info.</returns>
        public static IDictionary<string, CommandInfo> GetCommands(this PoshConsole shellHost, string module)
        {
            var par = shellHost.Invoke($"(Get-Module -Name {module}).ExportedCommands");

            if (par.Count <= 0 || par[0] == null)
                return null;

            var meta = par[0].BaseObject as IDictionary<string, CommandInfo>;
            return meta;
        }

        /// <summary>
        /// Returns information from a single PowerShell command.
        /// </summary>
        /// <param name="shellHost">Shell host.</param>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>CommandInfo with information for the command.</returns>
        public static CommandInfo GetCommand(this PoshConsole shellHost, string command)
        {
            var par = shellHost.Invoke($"Get-Command -Name {command}");
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].BaseObject as CommandInfo;
        }

        /// <summary>
        /// Returns a list of parameters a single command accepts.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>IDictionary with a key containing the parameter name and a value containing the metadata.</returns>
        public static IDictionary<string, ParameterMetadata> GetParameters(this PoshConsole shellHost, string command)
        {
            var commandMeta = shellHost.GetCommand(command);
            if (command == null)
                return null;

            var meta = commandMeta.Parameters as IDictionary<string, ParameterMetadata>;
            return meta;
        }

        /// <summary>
        /// Returns help for a single PowerShell command.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <returns>Preformatted string containing the help for the command.</returns>
        public static string GetCommandHelp(this PoshConsole shellHost, string command)
        {
            var par = shellHost.Invoke($"Get-Help -Name {command} | Out-String");
            if (par.Count <= 0 || par[0] == null)
                return null;

            return par[0].ToString();
        }

        /// <summary>
        /// Returns help for a single parameter of a PowerShell command.
        /// </summary>
        /// <param name="command">String containing the name of the command.</param>
        /// <param name="parameter">String containing the name of the parameter.</param>
        /// <returns>Preformatted string containing the help for the parameter.</returns>
        public static string GetParameterHelp(this PoshConsole shellHost, string command, string parameter)
        {
            var par = shellHost.Invoke($"Get-Help -Name {command} -Parameter {parameter} | Out-String");
            return par.Count <= 0 ? null : par[0].ToString();
        }
    }
}
