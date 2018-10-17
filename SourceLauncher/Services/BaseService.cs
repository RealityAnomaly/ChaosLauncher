using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SourceLauncher.Services
{
    internal class BaseService
    {
        private readonly IServiceProvider _serviceProvider;
        protected readonly ILogger Logger;

        internal BaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());
        }
    }
}
