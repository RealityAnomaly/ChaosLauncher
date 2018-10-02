using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Perforce.P4;

namespace SourceLauncher.Services
{
    class PerforceService : BaseService
    {
        public PerforceService(IServiceProvider s) : base(s) { InitializeP4(); }
        public bool p4Exists = false;

        private static readonly string p4Path = @"C:\Program Files\Perforce";
        private void InitializeP4()
        {
            logger.LogInformation("The Perforce subsystem is initializing...");
            LocateP4();
        }

        public Repository Connect(string uri, string user, string client)
        {
            Server server = new Server(new ServerAddress(uri));
            Repository rep = new Repository(server);
            Connection con = rep.Connection;

            con.UserName = user;
            con.Client = new Client
            {
                Name = client
            };

            Options options = new Options
            {
                ["ProgramName"] = "Chaos Launcher"
            };

            con.Connect(options);
            Credential cred = con.Login(null, null, null);

            return rep;
        }

        private void LocateP4()
        {
            if (!Directory.Exists(p4Path))
            {
                logger.LogError("Perforce does not seem to be installed. Running P4V will not be available.");
                return;
            }

            p4Exists = true;
        }
        public void RunP4Tool(string tool)
        {
            if (!p4Exists)
                return;

            Process.Start(String.Format(@"{0}\{1}.exe", p4Path, tool));
        }
    }
}
