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
//Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Classes\riotclient\DefaultIcon
// Computer\HKEY_USERS\S-1-5-21-4227508906-1149539934-2714898774-1001\Control Panel\NotifyIconSettings\2047204458577611852