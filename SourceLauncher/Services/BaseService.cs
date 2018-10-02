using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SourceLauncher.Services
{
    class BaseService
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly ILogger logger;
        public BaseService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());
        }
    }
}
