using SourceLauncher.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class OutputReference
    {
        public readonly Guid SourceToolId;
        public readonly Output SourceOutput;

        /// <summary>
        /// References an output of a tool on the workspace.
        /// </summary>
        /// <param name="sourceTool">The tool to be referenced.</param>
        /// <param name="sourceOutput">The output. If null is specified, the default output will be used.</param>
        public OutputReference(Guid sourceToolId, Output sourceOutput)
        {
            SourceToolId = sourceToolId;
            SourceOutput = sourceOutput;
        }
    }
}
