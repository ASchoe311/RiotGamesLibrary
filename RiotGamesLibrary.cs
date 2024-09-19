﻿using Playnite.SDK;
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
                    Source = new MetadataNameProperty("Riot Games"),
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
                    Source = new MetadataNameProperty("Riot Games"),
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
                    Source = new MetadataNameProperty("Riot Games"),
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
            return gameList;
        }

        //REMOVE AFTER A FEW UPDATES
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            base.OnApplicationStarted(args);
            if (settings.Settings.FirstStart)
            {
                logger.Info("Detected first run of new plugin version, ensuring sources are properly set and clearing outdated companion actions");
                Guid rgSource = Guid.NewGuid();
                bool srcFound = false;
                foreach (var source in PlayniteApi.Database.Sources)
                {
                    //logger.Debug($"Source with id {source.Id} is {source.Name}");
                    if (source.Name == "Riot Games")
                    {
                        rgSource = source.Id;
                        srcFound = true;
                    }

                }
                if (!srcFound)
                {
                    GameSource rg = new GameSource("Riot Games");
                    rgSource = rg.Id;
                    PlayniteApi.Database.Sources.Add(rg);
                    PlayniteApi.Database.Sources.Update(rg);
                }
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (game.PluginId != Id)
                    {
                        continue;
                    }
                    game.SourceId = rgSource;
                    if (game.GameId != "rg-legendsofruneterra")
                    {
                        for (int i = 0; i < game.GameActions.Count; i++)
                        {
                            if (game.GameActions[i].Name != null && game.GameActions[i].Name != string.Empty && game.GameActions[i].Name.ToLower().Contains("companion"))
                            {
                                game.GameActions.Remove(game.GameActions[i]);
                            }
                        }
                    }
                    PlayniteApi.Database.Games.Update(game);
                }
                settings.Settings.FirstStart = false;
                SavePluginSettings(settings.Settings);
            }
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
                    foreach (var comp in settings.Settings.LeagueCompanions)
                    {
                        bool actionExists = false;
                        for (int i = 0; i < game.GameActions.Count; i++)
                        {
                            if (game.GameActions[i].Name == $"Open {comp.ExeName}")
                            {
                                actionExists = true;
                                if (comp.GenerateAction)
                                {
                                    game.GameActions[i].Path = comp.ExePath;
                                    game.GameActions[i].Arguments = comp.LaunchArgs;
                                    game.GameActions[i].WorkingDir = Path.GetDirectoryName(comp.ExePath);
                                }
                                else
                                {
                                    logger.Info($"Removing game action for {comp.ExeName} from League of Legends");
                                    game.GameActions.Remove(game.GameActions[i]);
                                }
                            }
                        }
                        if (!actionExists && comp.GenerateAction)
                        {
                            logger.Info($"Generating game action for {comp.ExeName} for League of Legends");
                            game.GameActions.Add(new GameAction()
                            {
                                Name = $"Open {comp.ExeName}",
                                Type = GameActionType.File,
                                Path = comp.ExePath,
                                Arguments = comp.LaunchArgs,
                                WorkingDir = Path.GetDirectoryName(comp.ExePath),
                                TrackingMode = TrackingMode.Default,
                                IsPlayAction = false
                            });
                        }
                    }
                }
                if (game.GameId == "rg-valorant")
                {
                    foreach (var comp in settings.Settings.ValorantCompanions)
                    {
                        bool actionExists = false;
                        for (int i = 0; i < game.GameActions.Count; i++)
                        {
                            if (game.GameActions[i].Name == $"Open {comp.ExeName}")
                            {
                                actionExists = true;
                                if (comp.GenerateAction)
                                {
                                    game.GameActions[i].Path = comp.ExePath;
                                    game.GameActions[i].Arguments = comp.LaunchArgs;
                                    game.GameActions[i].WorkingDir = Path.GetDirectoryName(comp.ExePath);
                                }
                                else
                                {
                                    logger.Info($"Removing game action for {comp.ExeName} from Valorant");
                                    game.GameActions.Remove(game.GameActions[i]);
                                }
                            }
                        }
                        if (!actionExists && comp.GenerateAction)
                        {
                            logger.Info($"Generating game action for {comp.ExeName} for Valorant");
                            game.GameActions.Add(new GameAction()
                            {
                                Name = $"Open {comp.ExeName}",
                                Type = GameActionType.File,
                                Path = comp.ExePath,
                                Arguments = comp.LaunchArgs,
                                WorkingDir = Path.GetDirectoryName(comp.ExePath),
                                TrackingMode = TrackingMode.Default,
                                IsPlayAction = false
                            });
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
            base.OnGameStarted(args);
            if (args.Game.PluginId != Id)
            {
                return;
            }
            if (args.Game.GameId == "rg-leagueoflegends")
            {
                foreach (var comp in settings.Settings.LeagueCompanions)
                {
                    if (comp.CompanionEnabled) 
                    {
                        logger.Info($"Starting League of Legends companion app: {comp.ExeName}");
                        Process.Start(comp.ExePath, comp.LaunchArgs);
                    }
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                foreach (var comp in settings.Settings.ValorantCompanions)
                {
                    if (comp.CompanionEnabled)
                    {
                        logger.Info($"Starting Valorant companion app: {comp.ExeName}");
                        Process.Start(comp.ExePath, comp.LaunchArgs); 
                    }
                }
            }
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
            base.OnGameStopped(args);
            if (args.Game.PluginId != Id)
            {
                return;
            }
            if (args.Game.GameId == "rg-leagueoflegends")
            {
                foreach (var comp in settings.Settings.LeagueCompanions)
                {
                    if (comp.CompanionEnabled && comp.CloseWithGame)
                    {
                        logger.Info($"Trying to stop League of Legends companion app: {comp.ExeName}");
                        if (Path.GetFileNameWithoutExtension(comp.ExePath) == "OverwolfLauncher")
                        {
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
                            Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(comp.ExePath));
                            foreach (Process proc in procs)
                            {
                                proc.Kill();
                            }
                        }
                    }
                }
            }
            if (args.Game.GameId == "rg-valorant")
            {
                foreach (var comp in settings.Settings.ValorantCompanions)
                {
                    if (comp.CompanionEnabled && comp.CloseWithGame)
                    {
                        logger.Info($"Trying to stop Valorant companion app: {comp.ExeName}");
                        if (Path.GetFileNameWithoutExtension(comp.ExePath) == "OverwolfLauncher")
                        {
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
                            Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(comp.ExePath));
                            foreach (Process proc in procs)
                            {
                                proc.Kill();
                            }
                        }
                    }
                }
            }
            if (settings.Settings.CloseRiotClient)
            {
                logger.Info($"Trying to stop Riot Client");
                Process proc = Process.GetProcessesByName("Riot Client").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
                proc = Process.GetProcessesByName("RiotClientServices").FirstOrDefault();
                if (proc != null) { proc.Kill(); }
            }
        }

        public void UpdateSettings()
        {
            settings.Settings.RiotClientPath = RiotClient.IsInstalled ? RiotClient.InstallationPath : "Not Installed";
            settings.Settings.LeaguePath = RiotClient.LeagueInstalled ? RiotClient.LeagueInstallPath : "Not Installed";
            settings.Settings.ValorantPath = RiotClient.ValorantInstalled ? RiotClient.ValorantInstallPath : "Not Installed";
            settings.Settings.LORPath = RiotClient.LORInstalled ? RiotClient.LORInstallPath : "Not Installed";
            SavePluginSettings(settings.Settings);
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