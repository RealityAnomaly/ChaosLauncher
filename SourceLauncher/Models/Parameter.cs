using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Management.Automation;

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

        private readonly string _name;
        private readonly Type _paramType;
        public ParameterMode ParamMode = ParameterMode.Content;
        public OutputReference Reference;

        [JsonIgnore]
        public string Help;

        public Parameter(string name)
        {
            _name = name;
        }

        public Parameter(ParameterMetadata meta)
        {
            _name = meta.Name;

            if (meta.SwitchParameter)
                ParamMode = ParameterMode.Switch;

            _paramType = meta.ParameterType;
        }

        public void SetContent(string content)
        {
            SetContent(content.Split(';'));
        }

        private void SetContent(IList<string> content)
        {
            Content = content;
        }

        public void AddContent(string content)
        {
            Content.Add(content);
        }

        public override string ToString()
        {
            return _name;
        }

        public string ContentToString()
        {
            return string.Join(";", Content);
        }

        public string ReferenceToString()
        {
            return $"ref:{Reference.SourceToolId}:{Reference.SourceOutput}";
        }
    }
}
