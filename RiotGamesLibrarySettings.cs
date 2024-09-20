using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;
using Shell32;

namespace RiotGamesLibrary
{

    public class CompanionApp
    {
        public string ExePath { get; set; }
        public string LaunchArgs { get; set; }
        public bool CloseWithGame { get; set; } = true;
        public bool CompanionEnabled { get; set; } = true;
        public string ExeName { get; set; }
        public bool GenerateAction { get; set; } = true;
    }

    public class RiotGamesLibrarySettings : ObservableObject
    {
        private bool _CloseRiotClient = true;
        private string _RiotClientPath = RiotClient.InstallationPath;
        private string _LeaguePath = RiotGame.IsInstalled("rg-leagueoflegends") ? RiotGame.InstallPath("rg-leagueoflegends") : "Not Installed";
        private bool _LeaguePBE = false;
        private string _ValorantPath = RiotGame.IsInstalled("rg-valorant") ? RiotGame.InstallPath("rg-valorant") : "Not Installed";
        private string _LORPath = RiotGame.IsInstalled("rg-legendsofruneterra") ? RiotGame.InstallPath("rg-legendsofruneterra") : "Not Installed";
        private bool _LeagueUseShortCompName = true;
        private bool _ValUseShortCompName = true;
        private bool _FirstStart = true;

        public bool CloseRiotClient { get => _CloseRiotClient; set => SetValue(ref _CloseRiotClient, value); }
        public string RiotClientPath { get => _RiotClientPath; set => SetValue(ref _RiotClientPath, value);  }
        public string LeaguePath { get => _LeaguePath; set => SetValue(ref _LeaguePath, value);  }
        public bool LeaguePBE { get => _LeaguePBE; set => SetValue(ref _LeaguePBE, value); }
        public string ValorantPath { get => _ValorantPath; set => SetValue(ref _ValorantPath, value);  }
        public string LORPath { get => _LORPath; set => SetValue(ref _LORPath, value);  }
        public ObservableCollection<CompanionApp> LeagueCompanions { get; set; } = new ObservableCollection<CompanionApp>();
        public ObservableCollection<CompanionApp> ValorantCompanions { get; set; } = new ObservableCollection<CompanionApp>();
        public bool LeagueUseShortCompName { get => _LeagueUseShortCompName; set => SetValue(ref _LeagueUseShortCompName, value); }
        public bool ValUseShortCompName { get => _ValUseShortCompName; set => SetValue(ref _ValUseShortCompName, value); }
        public bool FirstStart { get => _FirstStart; set => SetValue(ref _FirstStart, value); }
    }

    public class RiotGamesLibrarySettingsViewModel : ObservableObject, ISettings
    {
        private readonly ILogger logger = LogManager.GetLogger();

        private readonly RiotGamesLibrary plugin;
        private RiotGamesLibrarySettings editingClone { get; set; }

        private RiotGamesLibrarySettings settings;
        public RiotGamesLibrarySettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<string> AddCompCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                Tuple<string, string, string> companion = SelectApp();
                if (companion != null)
                {
                    var compsList = (a == "rg-leagueoflegends") ? Settings.LeagueCompanions : Settings.ValorantCompanions;
                    foreach (var comp in compsList)
                    {
                        if (companion.Item1 == comp.ExePath)
                        {
                            Playnite.SDK.API.Instance.Dialogs.ShowErrorMessage("Selected app has already been added to companions list", "Error Adding Companion App");
                            return;
                        }
                    }
                    CompanionApp lc = new CompanionApp();
                    lc.ExePath = companion.Item1;
                    lc.LaunchArgs = companion.Item2;
                    if (companion.Item1.ToLower().Contains("overwolf"))
                    {
                        lc.ExeName = $"{Path.GetFileNameWithoutExtension(companion.Item3)} (Overwolf)";
                    }
                    else
                    {
                        lc.ExeName = Path.GetFileNameWithoutExtension(companion.Item1);
                    }

                    compsList.Add(lc);
                }
            });
        }

        public RelayCommand<CompanionApp> RemoveLeagueCompCommand
        {
            get => new RelayCommand<CompanionApp>((a) =>
            {
                RemoveCompanion(a, GamesEnums.League);
            });
        }

        public RelayCommand<CompanionApp> RemoveValorantCompCommand
        {
            get => new RelayCommand<CompanionApp>((a) =>
            {
                RemoveCompanion(a, GamesEnums.Valorant);
            });
        }

        private void RemoveCompanion(CompanionApp comp, GamesEnums rgame)
        {
            if (comp == null) { return; }
            string gameId = (rgame == GamesEnums.League) ? "rg-leagueoflegends" : "rg-valorant";
            var comps = (rgame == GamesEnums.League) ? Settings.LeagueCompanions : Settings.ValorantCompanions;
            if (comp.ExePath != string.Empty)
            {
                foreach (var game in Playnite.SDK.API.Instance.Database.Games)
                {
                    if (game.PluginId != plugin.Id || game.GameId != gameId)
                    {
                        continue;
                    }
                    for (int i = 0; i < game.GameActions.Count; i++)
                    {
                        if (game.GameActions[i].Name == $"Open {comp.ExeName}")
                        {
                            game.GameActions.Remove(game.GameActions[i]);
                            Playnite.SDK.API.Instance.Database.Games.Update(game);
                        }
                    }
                }
            }
            comps.Remove(comp);
        }

        private Tuple<string, string, string> SelectApp()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DereferenceLinks = false;
            openFileDialog.Filter = "Executable file (.exe)|*.exe";


            Nullable<bool> result = openFileDialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string lnkName = string.Empty;
                string targetname = openFileDialog.FileName;
                string args = string.Empty;
                if (openFileDialog.FileName.Contains(".lnk"))
                {
                    //logger.Debug("File chosen is a shortcut, trying to extract target and args");
                    // Open document
                    lnkName = openFileDialog.FileName;
                    string pathOnly = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                    string filenameOnly = System.IO.Path.GetFileName(openFileDialog.FileName);

                    Shell shell = new Shell();
                    Shell32.Folder folder = shell.NameSpace(pathOnly);
                    FolderItem folderItem = folder.ParseName(filenameOnly);
                    if (folderItem != null)
                    {
                        Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                        targetname = link.Target.Path;  // <-- main difference
                        args = link.Arguments;
                        if (targetname.StartsWith("{"))
                        { // it is prefixed with {54A35DE2-guid-for-program-files-x86-QZ32BP4}
                            int endguid = targetname.IndexOf("}");
                            if (endguid > 0)
                            {
                                targetname = "C:\\program files (x86)" + targetname.Substring(endguid + 1);
                            }
                        }
                        //string file = LnkToFile(openFileDialog.FileName);
                    }
                }
                return new Tuple<string, string, string>(targetname, args, lnkName);
            }
            return null;
        }

        public RiotGamesLibrarySettingsViewModel(RiotGamesLibrary plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<RiotGamesLibrarySettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new RiotGamesLibrarySettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and CloseRiotClient.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and CloseRiotClient.
            plugin.UpdateCompanionActions();
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            foreach (var comp in Settings.LeagueCompanions)
            {
                if (comp.ExePath != string.Empty && (!File.Exists(comp.ExePath) || Path.GetExtension(comp.ExePath) != ".exe"))
                {
                    errors.Add($"League companion {comp.ExePath} does not point to a valid executable file");
                    return false;
                }
                if (comp.ExePath == string.Empty)
                {
                    errors.Add($"Companion executable path cannot be empty");
                    return false;
                }
            }
            foreach (var comp in Settings.ValorantCompanions)
            {
                if (comp.ExePath != string.Empty && (!File.Exists(comp.ExePath) || Path.GetExtension(comp.ExePath) != ".exe"))
                {
                    errors.Add($"Valorant companion {comp.ExePath} does not point to a valid executable file");
                    return false;
                }
                if (comp.ExePath == string.Empty)
                {
                    errors.Add($"Companion executable path cannot be empty");
                    return false;
                }
            }
            return true;
        }
    }
}