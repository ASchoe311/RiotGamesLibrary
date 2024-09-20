using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RiotGamesLibrary
{

    public class RiotGame
    {
        private struct GameName
        {
            public string registryName;
            public string exeName;
        }

        private static Dictionary<string, GameName> idToInstall = new Dictionary<string, GameName>()
        {
            { "rg-leagueoflegends", new GameName(){registryName = "league_of_legends", exeName = "LeagueClient.exe"} },
            { "rg-valorant", new GameName(){registryName = "valorant", exeName = "VALORANT.exe"} },
            { "rg-legendsofruneterra", new GameName(){registryName = "bacon", exeName = "LoR.exe"} }
        };

        public static Dictionary<string, string> Icons = new Dictionary<string, string>()
        {
            { "rg-leagueoflegends",  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\leagueicon.png") },
            { "rg-valorant",  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\valoranticon.png") },
            { "rg-legendsofruneterra",  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\loricon.png") }
        };

        public static string InstallPath(string gameId)
        {
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game {idToInstall[gameId].registryName}.live"))
            {
                if (key != null && File.Exists(Path.Combine(key?.GetValue("InstallLocation").ToString(), idToInstall[gameId].exeName)))
                {
                    return Path.GetFullPath(key.GetValue("InstallLocation").ToString());
                }
                return string.Empty;
            }
        }

        public static bool IsInstalled(string gameId)
        {
            if (string.IsNullOrEmpty(InstallPath(gameId)) || !Directory.Exists(InstallPath(gameId)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }

    public class RiotClient
    {

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "RiotClientServices.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(@"riotclient\shell\open\command"))
                {
                    if (key?.GetValue("").ToString().Contains("RiotClientServices.exe") == true)
                    {
                        return Path.GetDirectoryName(key.GetValue("").ToString().Split('\"')[1]);
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\riotclient\shell\open\command"))
                {
                    if (key?.GetValue("").ToString().Contains("RiotClientServices.exe") == true)
                    {
                        return Path.GetDirectoryName(key.GetValue("").ToString().Split('\"')[1]);
                    }
                }
                return string.Empty;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(InstallationPath) || !Directory.Exists(InstallationPath))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\rioticon.png");
    }
}
