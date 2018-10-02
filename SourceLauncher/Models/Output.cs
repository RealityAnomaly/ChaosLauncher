using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceLauncher.Models
{
    public class Output
    {
        private readonly Type type;
        public Output(Type type = null)
        {
            this.type = type;
        }

        public override string ToString()
        {
            if (type == null)
                return "(Default Output)";

            return type.ToString();
        }
    }
}
