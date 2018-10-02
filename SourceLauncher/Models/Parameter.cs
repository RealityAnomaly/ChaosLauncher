using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class Parameter
    {
        public enum ParameterMode
        {
            Content,
            Reference,
            Variable,
            Switch
        }

        /// <summary>
        /// Contains content for the parameter.
        /// If this is null, the parameter will be a switch.
        /// </summary>
        public IList<string> Content { get; private set; } = new List<string>();

        private readonly string Name;
        [JsonIgnore]
        public string Help;
        public readonly Type ParamType;
        public ParameterMode ParamMode = ParameterMode.Content;
        public OutputReference Reference;

        public Parameter(string name)
        {
            Name = name;
        }

        public Parameter(ParameterMetadata meta)
        {
            Name = meta.Name;

            if (meta.SwitchParameter)
                ParamMode = ParameterMode.Switch;

            ParamType = meta.ParameterType;
        }

        public void SetContent(string content)
        {
            SetContent(content.Split(';'));
        }

        public void SetContent(IList<string> content)
        {
            Content = content;
        }

        public void AddContent(string content)
        {
            Content.Add(content);
        }

        public override string ToString()
        {
            return Name;
        }

        public string ContentToString()
        {
            return String.Join(";", Content);
        }

        public string ReferenceToString()
        {
            return String.Format("ref:{0}:{1}", Reference.SourceToolId, Reference.SourceOutput.ToString());
        }
    }
}
