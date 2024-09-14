using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RiotGamesLibrary
{
    public enum GamesEnums
    {
        League = 0,
        LeaguePBE = 1,
        Valorant = 2,
        LoR = 3
    }

    public class RiotGamesLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private RiotGamesLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("91d13c6f-63d3-42ed-a100-6f811a8387ea");

        // Change to something more appropriate
        public override string Name => "Riot Games";

        public override string LibraryIcon => RiotClient.Icon;



        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new RiotGamesLibraryClient();

        public RiotGamesLibrary(IPlayniteAPI api) : base(api)
        {
            settings = new RiotGamesLibrarySettingsViewModel(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
            settings.Settings.RiotClientPath = RiotClient.InstallationPath;
            settings.Settings.LeaguePath = RiotClient.LeagueInstallPath;
            settings.Settings.ValorantPath = RiotClient.ValorantInstallPath;
            settings.Settings.LORPath = RiotClient.LORInstallPath;
        }

        private string GetGameArgs(Enum game)
        {
            switch (game)
            {
                case GamesEnums.League:
                    return "--launch-product=league_of_legends --launch-patchline=live";
                case GamesEnums.LeaguePBE:
                    return "--launch-product=league_of_legends --launch-patchline=pbe";
                case GamesEnums.Valorant:
                    return "--launch-product=valorant --launch-patchline=live";
                case GamesEnums.LoR:
                    return "--launch-product=bacon --launch-patchline=live";
                default:
                    return string.Empty;
            }
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            // Return list of user's games.
            List<GameMetadata> gameList = new List<GameMetadata>()
            {
                new GameMetadata()
                {
                    Name = "League of Legends",
                    GameId = "rg-leagueoflegends",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = RiotClient.ClientExecPath,
                            Arguments = GetGameArgs(GamesEnums.League),
                            WorkingDir = RiotClient.InstallationPath,
                            TrackingMode = TrackingMode.Directory,
                            TrackingPath = RiotClient.LeagueInstallPath,
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = RiotClient.LeagueInstalled,
                    InstallDirectory = RiotClient.LeagueInstallPath,
                    Icon = new MetadataFile(RiotClient.LeagueIcon)
                },
                new GameMetadata()
                {
                    Name = "Valorant",
                    GameId = "rg-valorant",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = RiotClient.ClientExecPath,
                            Arguments = GetGameArgs(GamesEnums.Valorant),
                            WorkingDir = RiotClient.InstallationPath,
                            TrackingMode = TrackingMode.Directory,
                            TrackingPath = RiotClient.ValorantInstallPath,
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = RiotClient.ValorantInstalled,
                    InstallDirectory = RiotClient.ValorantInstallPath,
                    Icon = new MetadataFile(RiotClient.ValorantIcon)
                },
                new GameMetadata()
                {
                    Name = "Legends of Runeterra",
                    GameId = "rg-legendsofruneterra",
                    GameActions = new List<GameAction>
                    {
                        new GameAction()
                        {
                            Type = GameActionType.File,
                            Path = RiotClient.ClientExecPath,
                            Arguments = GetGameArgs(GamesEnums.LoR),
                            WorkingDir = RiotClient.InstallationPath,
                            TrackingMode = TrackingMode.Directory,
                            TrackingPath = RiotClient.LORInstallPath,
                            IsPlayAction = true
                        }
                    },
                    IsInstalled = RiotClient.LORInstalled,
                    InstallDirectory = RiotClient.LORInstallPath,
                    Icon = new MetadataFile(RiotClient.LORIcon)
                }
            };
            //if (settings.Settings.LeaguePBE)
            //{
            //    gameList.Add(new GameMetadata()
            //    {
            //        Name = "League of Legends PBE",
            //        GameId = "rg-leagueoflegendspbe",
            //        GameActions = new List<GameAction>
            //        {
            //            new GameAction()
            //            {
            //                Type = GameActionType.File,
            //                Path = RiotClient.ClientExecPath,
            //                Arguments = GetGameArgs(GamesEnums.LeaguePBE),
            //                WorkingDir = RiotClient.InstallationPath,
            //                TrackingMode = TrackingMode.Directory,
            //                TrackingPath = RiotClient.LeagueInstallPath,
            //                IsPlayAction = true
            //            }
            //        },
            //        IsInstalled = RiotClient.LeagueInstalled,
            //        InstallDirectory = RiotClient.LeagueInstallPath,
            //        Icon = new MetadataFile(RiotClient.LeagueIcon)
            //    });
            //}
            return gameList;
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            //logger.Debug($"launching game with id {args.Game.Id}");
            if (args.Game.PluginId != Id)
            {
                return;
            }
            if (args.Game.GameId == "rg-leagueoflegends")
            {
                if (settings.Settings.LeagueCompanionExe != string.Empty)
                {
                    logger.Debug($"Trying to run companion at {settings.Settings.LeagueCompanionExe}");
                    Process.Start(settings.Settings.LeagueCompanionExe);
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                if (settings.Settings.ValorantCompanionExe != string.Empty)
                {
                    logger.Debug($"Trying to run companion at {settings.Settings.ValorantCompanionExe}");
                    Process.Start(settings.Settings.ValorantCompanionExe);
                }
            }
            base.OnGameStarted(args);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return;
            }
            if (args.Game.GameId == "rg-leagueoflegends")
            {
                if (settings.Settings.CloseCompanionWithLeague && settings.Settings.LeagueCompanionExe != string.Empty)
                {
                    logger.Debug($"Trying to stop companion by name {Path.GetFileNameWithoutExtension(settings.Settings.LeagueCompanionExe)}");
                    Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(settings.Settings.LeagueCompanionExe));
                    foreach (Process proc in procs)
                    {
                        proc.Kill();
                    }
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                if (settings.Settings.CloseCompanionWithValorant && settings.Settings.ValorantCompanionExe != string.Empty)
                {
                    logger.Debug($"Trying to stop companion by name {Path.GetFileNameWithoutExtension(settings.Settings.ValorantCompanionExe)}");
                    Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(settings.Settings.ValorantCompanionExe));
                    foreach (Process proc in procs)
                    {
                        proc.Kill();
                    }
                }
            }
            if (settings.Settings.CloseRiotClient)
            {
                logger.Debug($"Trying to stop Riot Client");
                Process proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
                proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
            }
            base.OnGameStopped(args);
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new RiotInstallController(args.Game);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new RiotUninstallController(args.Game);
        }
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new RiotGamesLibrarySettingsView();
        }
    }
}