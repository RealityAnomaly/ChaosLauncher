using System;

namespace SourceLauncher.Models
{
    public class Output
    {
        private readonly Type _type;
        public Output(Type type = null)
        {
            _type = type;
        }

        public override string ToString()
        {
            return _type == null ? "(Default Output)" : _type.ToString();
        }
    }
}
