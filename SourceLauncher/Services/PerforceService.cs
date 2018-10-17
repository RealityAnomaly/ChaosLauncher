using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Perforce.P4;

namespace SourceLauncher.Services
{
    internal class PerforceService : BaseService
    {
        public PerforceService(IServiceProvider s) : base(s) { InitializeP4(); }
        public bool P4Exists;

        private const string P4Path = @"C:\Program Files\Perforce";

        private void InitializeP4()
        {
            Logger.LogInformation("The Perforce subsystem is initializing...");
            LocateP4();
        }

        public static Repository Connect(string uri, string user, string client)
        {
            var server = new Server(new ServerAddress(uri));
            var rep = new Repository(server);
            var con = rep.Connection;

            con.UserName = user;
            con.Client = new Client
            {
                Name = client
            };

            var options = new Options
            {
                ["ProgramName"] = "Chaos Launcher"
            };

            con.Connect(options);
            //var cred = con.Login(null, null, null);

            return rep;
        }

        private void LocateP4()
        {
            if (!Directory.Exists(P4Path))
            {
                Logger.LogError("Perforce does not seem to be installed. Running P4V will not be available.");
                return;
            }

            P4Exists = true;
        }
        public void RunP4Tool(string tool)
        {
            if (!P4Exists)
                return;

            Process.Start($@"{P4Path}\{tool}.exe");
        }
    }
}
