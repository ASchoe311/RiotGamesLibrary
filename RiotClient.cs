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
        public static string LeagueInstallPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game league_of_legends.live"))
                {
                    if (key?.GetValue("InstallLocation").ToString().ToLower().Contains("league of legends") == true)
                    {
                        return Path.GetFullPath(key.GetValue("InstallLocation").ToString());
                    }
                    return string.Empty;
                }
            }
        }

        public static string ValorantInstallPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game valorant.live"))
                {
                    if (key?.GetValue("InstallLocation").ToString().ToLower().Contains("valorant") == true)
                    {
                        return Path.GetFullPath(key.GetValue("InstallLocation").ToString());
                    }
                    return string.Empty;
                }
            }
        }

        public static string LORInstallPath
        {
            get
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Riot Game bacon.live"))
                {
                    if (key?.GetValue("InstallLocation").ToString().ToLower().Contains("lor") == true)
                    {
                        return Path.GetFullPath(key.GetValue("InstallLocation").ToString());
                    }
                    return string.Empty;
                }
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


        public static bool LeagueInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(LeagueInstallPath) || !Directory.Exists(LeagueInstallPath))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool ValorantInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(ValorantInstallPath) || !Directory.Exists(ValorantInstallPath))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool LORInstalled
        {
            get
            {
                if (string.IsNullOrEmpty(LORInstallPath) || !Directory.Exists(LORInstallPath))
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
        public static string LeagueIcon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\leagueicon.png");
        public static string ValorantIcon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\valoranticon.png");
        public static string LORIcon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\loricon.png");
    }
}
