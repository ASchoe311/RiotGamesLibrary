using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace RiotGamesLibrary
{

    public class RiotInstallController : InstallController
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private CancellationTokenSource watcherToken;
        private RiotGamesLibrary _plugin;
        public RiotInstallController(Game game, RiotGamesLibrary plugin) : base(game)
        {
            Name = $"Install {game.Name}";
            _plugin = plugin;
        }

        public override void Install(InstallActionArgs args)
        {
            logger.Info("Opening riot client for installation");
            Playnite.SDK.API.Instance.Dialogs.ShowMessage("Opening Riot Client so you can install the game");
            Process.Start(RiotClient.ClientExecPath);
            StartInstallWatcher();
        }

        public async void StartInstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            await Task.Run(async () =>
            {
                while (true)
                {
                    if (watcherToken.IsCancellationRequested) return;
                    if (Game.GameId == "rg-legendsofruneterra")
                    {
                        if (File.Exists(Path.Combine(RiotClient.LORInstallPath, "LoR.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotClient.LORInstallPath
                            };
                            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                            _plugin.UpdateSettings();
                            return;
                        }
                    }
                    if (Game.GameId == "rg-leagueoflegends")
                    {
                        if (File.Exists(Path.Combine(RiotClient.LeagueInstallPath, "LeagueClient.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotClient.LeagueInstallPath
                            };
                            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                            _plugin.UpdateSettings();
                            return;
                        }
                    }
                    if (Game.GameId == "rg-valorant")
                    {
                        if (File.Exists(Path.Combine(RiotClient.ValorantInstallPath, "VALORANT.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotClient.ValorantInstallPath
                            };
                            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                            _plugin.UpdateSettings();
                            return;
                        }
                    }
                    await Task.Delay(10000);
                }
            });
        }

    }

    public class RiotUninstallController : UninstallController
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private List<MessageBoxOption> leagueBoxOptions = new List<MessageBoxOption>
            {
                new MessageBoxOption("Live", true, false),
                new MessageBoxOption("PBE", false, false),
                new MessageBoxOption("Cancel", false, true)
            };
        private List<MessageBoxOption> otherBoxOptions = new List<MessageBoxOption>
            {
                new MessageBoxOption("Uninstall", true, false),
                new MessageBoxOption("Cancel", false, true)
            };

        private RiotGamesLibrary _plugin;

        private string GetUninstallStringFromGameID()
        {
            if (Game.GameId == "rg-leagueoflegends")
            {
                return "--uninstall-product=league_of_legends";
            }
            if (Game.GameId == "rg-valorant")
            {
                return "--uninstall-product=valorant";
            }
            if (Game.GameId == "rg-legendsofruneterra")
            {
                return "--uninstall-product=bacon";
            }

            return string.Empty;
        }

        public RiotUninstallController(Game game, RiotGamesLibrary plugin) : base(game)
        {
            Name = $"Uninstall {game.Name}";
            _plugin = plugin;
        }

        private CancellationTokenSource watcherToken;

        public override void Dispose()
        {
            logger.Debug($"Is watcher null? {watcherToken == null}");
            watcherToken?.Cancel();
        }

        public override void Uninstall(UninstallActionArgs args)
        {
            Process proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
            if (proc != null) { proc.Kill(); }
            proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
            if (proc != null) { proc.Kill(); }
            Dispose();
            StartUninstallWatcher();
            var playniteApi = Playnite.SDK.API.Instance;
            MessageBoxOption selected;
            if (Game.GameId == "rg-leagueoflegends")
            {
                selected = playniteApi.Dialogs.ShowMessage(
                    $"Which version of {Game.Name} would you like to uninstall?",
                    $"Uninstalling {Game.Name}",
                    MessageBoxImage.Exclamation,
                    leagueBoxOptions
                );
            }
            else
            {
                selected = playniteApi.Dialogs.ShowMessage(
                    $"Confirm uninstall of {Game.Name}?",
                    $"Uninstalling {Game.Name}",
                    MessageBoxImage.Exclamation,
                    otherBoxOptions
                );
            }
            if (selected.IsCancel)
            {
                Dispose();
                return;
            }
            else if (selected.IsDefault)
            {
                Process.Start(RiotClient.ClientExecPath, (GetUninstallStringFromGameID() + " --uninstall-patchline=live"));
            }
            else
            {
                Process.Start(RiotClient.ClientExecPath, (GetUninstallStringFromGameID() + " --uninstall-patchline=PBE"));

            }
        }
        public async void StartUninstallWatcher()
        {
            watcherToken = new CancellationTokenSource();
            while (true)
            {
                if (watcherToken.IsCancellationRequested)
                {
                    logger.Debug("Trying to cancel uninstall");
                    Game.IsUninstalling = false;
                    return;
                }

                if (Game.GameId == "rg-leagueoflegends")
                {
                    if (!RiotClient.LeagueInstalled)
                    {
                        logger.Debug("Uninstall finished");
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        _plugin.UpdateSettings();
                        return;
                    }
                    
                }
                if (Game.GameId == "rg-legendsofruneterra")
                {
                    if (!RiotClient.LORInstalled)
                    {
                        logger.Debug("Uninstall finished");
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        _plugin.UpdateSettings();
                        return;
                    }

                }
                if (Game.GameId == "rg-valorant")
                {
                    if (!RiotClient.ValorantInstalled)
                    {
                        logger.Debug("Uninstall finished");
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        _plugin.UpdateSettings();
                        return;
                    }

                }

                await Task.Delay(2000);
            }
        }
    }
}
