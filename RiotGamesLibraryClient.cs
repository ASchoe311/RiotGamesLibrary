using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace RiotGamesLibrary
{
    public class RiotGamesLibraryClient : LibraryClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public override string Icon => RiotClient.Icon;
        public override bool IsInstalled => RiotClient.IsInstalled;

        public override void Open()
        {
            Process.Start(RiotClient.ClientExecPath);
        }

        public override void Shutdown()
        {
            var proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
            var proc2 = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
            if (proc == null && proc2 == null)
            {
                logger.Info("Playnite attempted to auto-close Riot Client, but it was already shut down");
                return;
            }
            logger.Info("Auto-closing Riot Client");
            if (proc != null) { proc.Kill(); }
            if (proc2 != null) { proc2.Kill(); }
        }
    }
}