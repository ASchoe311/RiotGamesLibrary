﻿using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Management;
using System.Drawing;
using System.Drawing.Imaging;

namespace RiotGamesLibrary
{
    public enum GamesEnums
    {
        League,
        LeaguePBE,
        Valorant,
        LoR
    }

    public class RiotGamesLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private RiotGamesLibrarySettingsViewModel settings { get; set; }

        internal static readonly string AssemblyPath = Path.GetDirectoryName(typeof(RiotGamesLibrary).Assembly.Location);

        private static readonly string iconPath = Path.Combine(AssemblyPath, "icon.png");

        public override Guid Id { get; } = Guid.Parse("91d13c6f-63d3-42ed-a100-6f811a8387ea");
        private int vNum = 4;

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
                HasSettings = true,
                CanShutdownClient = true
            };
            UpdateSettings();
        }

        private static Dictionary<string, Tuple<string, string>> rgGames = new Dictionary<string, Tuple<string, string>>()
        {
            { "rg-leagueoflegends", Tuple.Create("League of Legends", "--launch-product=league_of_legends --launch-patchline=live") },
            { "rg-valorant", Tuple.Create("Valorant", "--launch-product=valorant --launch-patchline=live") },
            { "rg-legendsofruneterra", Tuple.Create("Legends of Runeterra", "--launch-product=bacon --launch-patchline=live") }
        };

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            foreach (var game in rgGames.Keys)
            {
                // pass icon as bytes so it doesn't get deleted on game removal
                byte[] iconData = null;
                using (var ms = new MemoryStream())
                {
                    Bitmap bmp = new Icon(RiotGame.Icons[game], new System.Drawing.Size(256, 256)).ToBitmap();
                    bmp.Save(ms, ImageFormat.Png);
                    iconData = ms.ToArray();
                }
                yield return new GameMetadata()
                {
                    Name = rgGames[game].Item1,
                    GameId = game,
                    Source = new MetadataNameProperty("Riot Games"),
                    Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC (Windows)") },
                    IsInstalled = RiotGame.IsInstalled(game),
                    InstallDirectory = RiotGame.InstallPath(game),
                    Icon = new MetadataFile($"{game}_icon.png", iconData)
                };
            }
        }

        //REMOVE AFTER A FEW UPDATES
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            base.OnApplicationStarted(args);
            if (settings.Settings.VersionNum != vNum)
            {
                logger.Info("Detected first run of new plugin version, ensuring sources are properly set, clearing outdated game actions, and updating icons");
                
                // Ensure source is properly set
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

                // Update game sources, actions, and icons
                foreach (var game in PlayniteApi.Database.Games)
                {
                    if (game.PluginId != Id)
                    {
                        continue;
                    }

                    Bitmap bmp = new Icon(RiotGame.Icons[game.GameId], new System.Drawing.Size(256, 256)).ToBitmap();
                    string savePath = Path.Combine(GetPluginUserDataPath(), $"{game.GameId}_icon.png");
                    bmp.Save(savePath, ImageFormat.Png);

                    if (game.SourceId == null || game.SourceId != rgSource)
                    {
                        game.SourceId = rgSource;
                    }

                    game.Icon = savePath;
                    
                    if (game.GameActions != null)
                    {
                        List<GameAction> removals = new List<GameAction>();
                        for (int i = 0; i < game.GameActions.Count; i++)
                        {
                            if (game.GameActions[i].IsPlayAction)
                            {
                                removals.Add(game.GameActions[i]);
                            }
                            else if (game.GameActions[i].Name != null && game.GameActions[i].Name != string.Empty && game.GameActions[i].Name.ToLower().Contains("companion"))
                            {
                                removals.Add(game.GameActions[i]);
                            }
                        }
                        foreach (var r in removals)
                        {
                            game.GameActions.Remove(r);
                        }
                    }
                    PlayniteApi.Database.Games.Update(game);
                }
                settings.Settings.VersionNum = vNum;
                SavePluginSettings(settings.Settings);
            }
        }

        /// <summary>
        /// Creates a game action for the given companion app
        /// </summary>
        /// <param name="comp">A <c>CompanionApp</c> <see cref="CompanionApp"/></param>
        /// <returns>A new game action to launch the companion app</returns>
        private GameAction GenAction (CompanionApp comp)
        {
            return new GameAction()
            {
                Name = $"Open {comp.ExeName}",
                Type = GameActionType.File,
                Path = comp.ExePath,
                Arguments = comp.LaunchArgs,
                WorkingDir = Path.GetDirectoryName(comp.ExePath),
                TrackingMode = TrackingMode.Default,
                IsPlayAction = false
            };
        }

        /// <summary>
        /// Creates or removes game actions for each companion app based on settings
        /// </summary>
        public void UpdateCompanionActions()
        {
            PlayniteApi.Database.Games.BeginBufferUpdate();
            foreach (var game in PlayniteApi.Database.Games)
            {
                if (game.PluginId != Id || game.GameId == "rg-legendsofruneterra")
                {
                    continue;
                }

                ObservableCollection<CompanionApp> companionsList = (game.GameId == "rg-leagueoflegends") ? settings.Settings.LeagueCompanions : settings.Settings.ValorantCompanions;
                string gameName = (game.GameId == "rg-leagueoflegends") ? "League of Legends" : "Valorant";
                List<GameAction> removals = new List<GameAction>();
                foreach (var comp in companionsList)
                {
                    bool actionExists = false;
                    if (game.GameActions == null)
                    {
                        if (comp.GenerateAction)
                        {
                            logger.Info($"Generating game action for {comp.ExeName} for {gameName}");
                            game.GameActions = new ObservableCollection<GameAction>();
                            game.GameActions.Add(GenAction(comp));
                        }
                        continue;
                    }
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
                                logger.Info($"Removing game action for {comp.ExeName} from {gameName}");
                                removals.Add(game.GameActions[i]);
                            }
                        }
                    }
                    if (!actionExists && comp.GenerateAction)
                    {
                        logger.Info($"Generating game action for {comp.ExeName} for  {gameName}");
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
                foreach (var r in removals)
                {
                    game.GameActions.Remove(r);
                }                
                PlayniteApi.Database.Games.Update(game);
            }
            PlayniteApi.Database.Games.EndBufferUpdate();
        }

        /// <summary>
        /// Launch relevant companion apps on game start
        /// </summary>
        /// <param name="args"><see cref="OnGameStartedEventArgs"/></param>
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            //logger.Debug($"launching game with id {args.Game.Id}");
            base.OnGameStarted(args);
            if (args.Game.PluginId != Id || args.Game.GameId == "rg-legendsofruneterra")
            {
                return;
            }
            ObservableCollection<CompanionApp> companionsList = (args.Game.GameId == "rg-leagueoflegends") ? settings.Settings.LeagueCompanions : settings.Settings.ValorantCompanions;
            string gameName = (args.Game.GameId == "rg-leagueoflegends") ? "League of Legends" : "Valorant";
            foreach (var comp in companionsList)
            {
                if (comp.CompanionEnabled)
                {
                    logger.Info($"Starting {gameName} companion app: {comp.ExeName}");
                    Process.Start(comp.ExePath, comp.LaunchArgs);
                }
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            base.OnGameStopped(args);
            if (args.Game.PluginId != Id)
            {
                return;
            }
            OnGameStoppedAsync(args);
        }

        /// <summary>
        /// Asynchronously close companion apps and client
        /// </summary>
        /// <param name="args"><c>OnGameStoppedEventArgs</c></param>
        private async void OnGameStoppedAsync(OnGameStoppedEventArgs args)
        {
            await Task.Run(() => OGS(args));
        }

        /// <summary>
        /// Close companion apps and client
        /// </summary>
        /// <param name="args"><c>OnGameStoppedEventArgs</c></param>
        private void OGS(OnGameStoppedEventArgs args)
        {
            if (args.Game.GameId != "rg-legendsofruneterra")
            {
                ObservableCollection<CompanionApp> companionsList = (args.Game.GameId == "rg-leagueoflegends") ? settings.Settings.LeagueCompanions : settings.Settings.ValorantCompanions;
                string gameName = (args.Game.GameId == "rg-leagueoflegends") ? "League of Legends" : "Valorant";
                foreach (var comp in companionsList)
                {
                    if (comp.CloseWithGame)
                    {
                        logger.Info($"Trying to stop {gameName} companion app: {comp.ExeName}");
                        var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
                        using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                        using (var results = searcher.Get())
                        {
                            var query = from p in Process.GetProcesses()
                                        join mo in results.Cast<ManagementObject>()
                                        on p.Id equals (int)(uint)mo["ProcessId"]
                                        select new
                                        {
                                            Process = p,
                                            Path = (string)mo["ExecutablePath"],
                                            CommandLine = (string)mo["CommandLine"],
                                        };
                            foreach (var item in query)
                            {
                                if (item.Path != null && item.Path.Contains(Path.GetDirectoryName(comp.ExePath)))
                                {
                                    item.Process.Kill();
                                }
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
            settings.Settings.RiotClientPath = RiotClient.IsInstalled ? RiotClient.InstallationPath : ResourceProvider.GetString("LOCRiotGamesNotInstalled");
            settings.Settings.LeaguePath = RiotGame.IsInstalled("rg-leagueoflegends") ? RiotGame.InstallPath("rg-leagueoflegends") : ResourceProvider.GetString("LOCRiotGamesNotInstalled");
            settings.Settings.ValorantPath = RiotGame.IsInstalled("rg-valorant") ? RiotGame.InstallPath("rg-valorant") : ResourceProvider.GetString("LOCRiotGamesNotInstalled");
            settings.Settings.LORPath = RiotGame.IsInstalled("rg-legendsofruneterra") ? RiotGame.InstallPath("rg-legendsofruneterra") : ResourceProvider.GetString("LOCRiotGamesNotInstalled");
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

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                yield break;
            }

            logger.Info($"Initializing play controller for {args.Game.GameId} with tracking path {RiotGame.InstallPath(args.Game.GameId)} ");
            Dictionary<string, string> idToExe = new Dictionary<string, string>()
            {
                { "rg-leagueoflegends", "LeagueClient.exe" },
                { "rg-valorant", "VALORANT.exe" },
                { "rg-legendsofruneterra", "LoR.exe" },
            };
            logger.Info($"Executable {idToExe[args.Game.GameId]} for {args.Game.GameId} is found in the tracking path? {File.Exists(Path.Combine(RiotGame.InstallPath(args.Game.GameId), idToExe[args.Game.GameId]))}");

            var gameInfo = rgGames[args.Game.GameId];
            AutomaticPlayController playController = new AutomaticPlayController(args.Game);
            playController.Name = "Play";
            playController.Path = RiotClient.ClientExecPath;
            playController.Arguments = gameInfo.Item2;
            playController.WorkingDir = RiotClient.InstallationPath;
            playController.TrackingMode = TrackingMode.Directory;
            playController.TrackingPath = RiotGame.InstallPath(args.Game.GameId);

            yield return playController;
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