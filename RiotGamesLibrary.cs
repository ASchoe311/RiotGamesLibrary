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

        internal static readonly string AssemblyPath = Path.GetDirectoryName(typeof(RiotGamesLibrary).Assembly.Location);

        private static readonly string iconPath = Path.Combine(AssemblyPath, "icon.png");

        public override Guid Id { get; } = Guid.Parse("91d13c6f-63d3-42ed-a100-6f811a8387ea");

        // Change to something more appropriate
        public override string Name => "Riot Games";

        public override string LibraryIcon { get; } = iconPath;



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
            UpdateSettings();
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
                    Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC (Windows)") },
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
                    Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC (Windows)") },
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
                    Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC (Windows)") },
                    IsInstalled = RiotClient.LORInstalled,
                    InstallDirectory = RiotClient.LORInstallPath,
                    Icon = new MetadataFile(RiotClient.LORIcon)
                }
            };
            if (settings.Settings.MakeLeagueCompAppAction && settings.Settings.LeagueCompanionExe != string.Empty)
            {
                gameList[0].GameActions.Add(new GameAction()
                {
                    Name = "Open Companion App",
                    Type = GameActionType.File,
                    Path = settings.Settings.LeagueCompanionExe,
                    Arguments = settings.Settings.LeagueCompanionExeArgs,
                    WorkingDir = Path.GetDirectoryName(settings.Settings.LeagueCompanionExe),
                    TrackingMode = TrackingMode.Default,
                    IsPlayAction = false
                });
            }
            if (settings.Settings.MakeValorantCompAppAction && settings.Settings.ValorantCompanionExe != string.Empty)
            {
                gameList[0].GameActions.Add(new GameAction()
                {
                    Name = "Open Companion App",
                    Type = GameActionType.File,
                    Path = settings.Settings.ValorantCompanionExe,
                    Arguments = settings.Settings.ValorantCompanionExeArgs,
                    WorkingDir = Path.GetDirectoryName(settings.Settings.ValorantCompanionExe),
                    TrackingMode = TrackingMode.Default,
                    IsPlayAction = false
                });
            }
            return gameList;
        }

        public void UpdateCompanionActions()
        {
            PlayniteApi.Database.Games.BeginBufferUpdate();
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (game.PluginId != Id)
                {
                    continue;
                }
                if (game.GameId == "rg-leagueoflegends")
                {
                    if (settings.Settings.LeagueCompanionExe != string.Empty)
                    {
                        if (game.GameActions.Count == 2)
                        {
                            var action = game.GameActions[1];
                            action.Path = settings.Settings.LeagueCompanionExe;
                            action.Arguments = settings.Settings.LeagueCompanionExeArgs;
                            action.WorkingDir = Path.GetDirectoryName(settings.Settings.LeagueCompanionExe);
                        }
                        else if (settings.Settings.MakeLeagueCompAppAction)
                        {
                            var action = new GameAction()
                            {
                                Name = "Open Companion App",
                                Type = GameActionType.File,
                                Path = settings.Settings.LeagueCompanionExe,
                                Arguments = settings.Settings.LeagueCompanionExeArgs,
                                WorkingDir = Path.GetDirectoryName(settings.Settings.LeagueCompanionExe),
                                TrackingMode = TrackingMode.Default,
                                IsPlayAction = false
                            };
                            game.GameActions.Add(action);
                        }
                    }
                    if (settings.Settings.LeagueCompanionExe == string.Empty || !settings.Settings.MakeLeagueCompAppAction)
                    {
                        if (game.GameActions.Count == 2)
                        {
                            game.GameActions.Remove(game.GameActions.ElementAt(1));
                        }
                    }
                }
                if (game.GameId == "rg-valorant")
                {
                    if (settings.Settings.ValorantCompanionExe != string.Empty)
                    {
                        if (game.GameActions.Count == 2)
                        {
                            var action = game.GameActions[1];
                            action.Path = settings.Settings.ValorantCompanionExe;
                            action.Arguments = settings.Settings.ValorantCompanionExeArgs;
                            action.WorkingDir = Path.GetDirectoryName(settings.Settings.ValorantCompanionExe);
                        }
                        else if (settings.Settings.MakeValorantCompAppAction)
                        {
                            var action = new GameAction()
                            {
                                Name = "Open Companion App",
                                Type = GameActionType.File,
                                Path = settings.Settings.ValorantCompanionExe,
                                Arguments = settings.Settings.ValorantCompanionExeArgs,
                                WorkingDir = Path.GetDirectoryName(settings.Settings.ValorantCompanionExe),
                                TrackingMode = TrackingMode.Default,
                                IsPlayAction = false
                            };
                            game.GameActions.Add(action);
                        }
                    }
                    if (settings.Settings.ValorantCompanionExe == string.Empty || !settings.Settings.MakeValorantCompAppAction)
                    {
                        if (game.GameActions.Count == 2)
                        {
                            game.GameActions.Remove(game.GameActions.ElementAt(1));
                        }
                    }
                }
                PlayniteApi.Database.Games.Update(game);
            }
            PlayniteApi.Database.Games.EndBufferUpdate();
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
                    if (settings.Settings.LeagueCompanionExeArgs != string.Empty)
                    {
                        Process.Start(settings.Settings.LeagueCompanionExe, settings.Settings.LeagueCompanionExeArgs);
                    }
                    else
                    {
                        Process.Start(settings.Settings.LeagueCompanionExe);
                    }
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                if (settings.Settings.ValorantCompanionExe != string.Empty)
                {
                    logger.Debug($"Trying to run companion at {settings.Settings.ValorantCompanionExe}");
                    if (settings.Settings.ValorantCompanionExeArgs != string.Empty)
                    {
                        Process.Start(settings.Settings.ValorantCompanionExe, settings.Settings.ValorantCompanionExeArgs);
                    }
                    else
                    {
                        Process.Start(settings.Settings.ValorantCompanionExe);
                    }
                }
            }
            base.OnGameStarted(args);
        }

        private static List<string> overwolfProcs = new List<string>()
        {
            "Overwolf",
            "OverwolfBrowser",
            "OverwolfHelper64",
            "OverwolfHelper"
        };

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
                    if (Path.GetFileNameWithoutExtension(settings.Settings.LeagueCompanionExe) == "OverwolfLauncher")
                    {
                        logger.Debug("Stopping overwolf processes");
                        foreach (string owProc in overwolfProcs)
                        {
                            var procs = Process.GetProcessesByName(owProc);
                            foreach (var proc in procs)
                            {
                                if (proc != null) { proc.Kill(); }
                            }
                        }
                    }
                    else
                    {
                        logger.Debug($"Trying to stop companion by name {Path.GetFileNameWithoutExtension(settings.Settings.LeagueCompanionExe)}");
                        Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(settings.Settings.LeagueCompanionExe));
                        foreach (Process proc in procs)
                        {
                            proc.Kill();
                        }

                    }
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                if (settings.Settings.CloseCompanionWithValorant && settings.Settings.ValorantCompanionExe != string.Empty)
                {

                    if (Path.GetFileNameWithoutExtension(settings.Settings.ValorantCompanionExe) == "OverwolfLauncher")
                    {
                        logger.Debug("Stopping overwolf processes");
                        foreach (string owProc in overwolfProcs)
                        {
                            var procs = Process.GetProcessesByName(owProc);
                            foreach (var proc in procs)
                            {
                                if (proc != null) { proc.Kill(); }
                            }
                        }
                    }
                    else
                    {
                        logger.Debug($"Trying to stop companion by name {Path.GetFileNameWithoutExtension(settings.Settings.ValorantCompanionExe)}");
                        Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(settings.Settings.ValorantCompanionExe));
                        foreach (Process proc in procs)
                        {
                            proc.Kill();
                        }
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

        public void UpdateSettings()
        {
            settings.Settings.LeaguePath = RiotClient.LeagueInstalled ? RiotClient.LeagueInstallPath: "Not Installed";
            settings.Settings.ValorantPath = RiotClient.ValorantInstalled ? RiotClient.ValorantInstallPath : "Not Installed";
            settings.Settings.LORPath = RiotClient.LORInstalled ? RiotClient.LORInstallPath : "Not Installed";
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new RiotInstallController(args.Game, this);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new RiotUninstallController(args.Game, this);
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