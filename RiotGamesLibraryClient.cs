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
        public override string Icon => RiotClient.Icon;
        public override bool IsInstalled => RiotClient.IsInstalled;

        public bool LeagueInstalled = false;
        public bool ValorantInstalled = false;

        public override void Open()
        {
            Process.Start(RiotClient.ClientExecPath);
        }

        public override void Shutdown()
        {
            var proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
            if (proc != null) { proc.Kill(); }
            proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
            if (proc != null) { proc.Kill(); }
        }
    }
}