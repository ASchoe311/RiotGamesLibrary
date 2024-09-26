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
            Name = $"{ResourceProvider.GetString("LOCInstallGame")} {game.Name}";
            _plugin = plugin;
        }

        public override void Install(InstallActionArgs args)
        {
            logger.Info("Opening riot client for installation");
            Playnite.SDK.API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCRiotGamesOpeningClient"));
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
                        if (File.Exists(Path.Combine(RiotGame.InstallPath("rg-legendsofruneterra"), "LoR.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotGame.InstallPath("rg-legendsofruneterra")
                            };
                            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                            _plugin.UpdateSettings();
                            return;
                        }
                    }
                    if (Game.GameId == "rg-leagueoflegends")
                    {
                        if (File.Exists(Path.Combine(RiotGame.InstallPath("rg-leagueoflegends"), "LeagueClient.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotGame.InstallPath("rg-leagueoflegends")
                            };
                            InvokeOnInstalled(new GameInstalledEventArgs(installInfo));
                            _plugin.UpdateSettings();
                            return;
                        }
                    }
                    if (Game.GameId == "rg-valorant")
                    {
                        if (File.Exists(Path.Combine(RiotGame.InstallPath("rg-valorant"), "VALORANT.exe")))
                        {
                            var installInfo = new GameInstallationData()
                            {
                                InstallDirectory = RiotGame.InstallPath("rg-valorant")
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
                new MessageBoxOption(ResourceProvider.GetString("LOCUninstallGame"), true, false),
                new MessageBoxOption(ResourceProvider.GetString("LOCCancelLabel"), false, true)
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
            Name = $"{ResourceProvider.GetString("LOCUninstallGame")} {game.Name}";
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
            Dispose();
            StartUninstallWatcher();
            var playniteApi = Playnite.SDK.API.Instance;
            MessageBoxOption selected;
            //Unused code, for now
            //if (Game.GameId == "rg-leagueoflegends")
            //{
            //    selected = playniteApi.Dialogs.ShowMessage(
            //        $"Which version of {Game.Name} would you like to uninstall?",
            //        $"Uninstalling {Game.Name}",
            //        MessageBoxImage.Exclamation,
            //        leagueBoxOptions
            //    );
            //}
            //else
            //{
            //    selected = playniteApi.Dialogs.ShowMessage(
            //        $"Confirm uninstall of {Game.Name}?",
            //        $"Uninstalling {Game.Name}",
            //        MessageBoxImage.Exclamation,
            //        otherBoxOptions
            //    );
            //}
            selected = playniteApi.Dialogs.ShowMessage(
                    $"{ResourceProvider.GetString("LOCRiotGamesConfirmUninstall")} {Game.Name}?",
                    $"{ResourceProvider.GetString("LOCUninstalling")} {Game.Name}",
                    MessageBoxImage.Exclamation,
                    otherBoxOptions
                );
            if (selected.IsCancel)
            {
                Dispose();
                return;
            }
            else if (selected.IsDefault)
            {
                logger.Info("Stopping Riot Client Services prior to game uninstall");
                Process proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
                proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
                Process.Start(RiotClient.ClientExecPath, (GetUninstallStringFromGameID() + " --uninstall-patchline=live"));
            }
            //Unused code, for now
            else
            {
                Process proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
                proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
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
                    if (!RiotGame.IsInstalled("rg-leagueoflegends"))
                    {
                        logger.Debug("Uninstall finished");
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        _plugin.UpdateSettings();
                        return;
                    }
                    
                }
                if (Game.GameId == "rg-legendsofruneterra")
                {
                    if (!RiotGame.IsInstalled("rg-legendsofruneterra"))
                    {
                        logger.Debug("Uninstall finished");
                        InvokeOnUninstalled(new GameUninstalledEventArgs());
                        _plugin.UpdateSettings();
                        return;
                    }

                }
                if (Game.GameId == "rg-valorant")
                {
                    if (!RiotGame.IsInstalled("rg-valorant"))
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
