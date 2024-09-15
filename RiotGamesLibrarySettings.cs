using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RiotGamesLibrary
{
    public class RiotGamesLibrarySettings : ObservableObject
    {
        private string _LeagueCompanionExe = string.Empty;
        private string _LeagueCompanionExeArgs = string.Empty;
        private bool _CloseRiotClient = true;
        private bool _CloseCompanionWithLeague = true;
        private string _RiotClientPath = RiotClient.InstallationPath;
        private string _LeaguePath = RiotClient.LeagueInstalled ? RiotClient.LeagueInstallPath : "Not Installed";
        private bool _LeaguePBE = false;
        private string _ValorantPath = RiotClient.ValorantInstalled ? RiotClient.ValorantInstallPath : "Not Installed";
        private string _ValorantCompanionExe = string.Empty;
        private string _ValorantCompanionExeArgs = string.Empty;
        private bool _CloseCompanionWithValorant = true;
        private string _LORPath = RiotClient.LORInstalled ? RiotClient.LORInstallPath : "Not Installed";

        public string LeagueCompanionExe { get => _LeagueCompanionExe; set => SetValue(ref _LeagueCompanionExe, value); }
        public bool CloseRiotClient { get => _CloseRiotClient; set => SetValue(ref _CloseRiotClient, value); }
        public bool CloseCompanionWithLeague { get => _CloseCompanionWithLeague; set => SetValue(ref _CloseCompanionWithLeague, value); }
        public string RiotClientPath { get => _RiotClientPath; set => SetValue(ref _RiotClientPath, value);  }
        public string LeaguePath { get => _LeaguePath; set => SetValue(ref _LeaguePath, value);  }
        public bool LeaguePBE { get => _LeaguePBE; set => SetValue(ref _LeaguePBE, value); }
        public string ValorantPath { get => _ValorantPath; set => SetValue(ref _ValorantPath, value);  }
        public string ValorantCompanionExe { get => _ValorantCompanionExe; set => SetValue(ref _ValorantCompanionExe, value); }
        public bool CloseCompanionWithValorant { get => _CloseCompanionWithValorant; set => SetValue(ref _CloseCompanionWithValorant, value); }
        public string LORPath { get => _LORPath; set => SetValue(ref _LORPath, value);  }
        public string LeagueCompanionExeArgs { get => _LeagueCompanionExeArgs; set => SetValue(ref _LeagueCompanionExeArgs, value); }
        public string ValorantCompanionExeArgs { get => _ValorantCompanionExeArgs; set => SetValue(ref _ValorantCompanionExeArgs, value); }
    }

    public class RiotGamesLibrarySettingsViewModel : ObservableObject, ISettings
    {
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
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            if (Settings.LeagueCompanionExe != string.Empty && (!File.Exists(Settings.LeagueCompanionExe) || Path.GetExtension(Settings.LeagueCompanionExe) != ".exe"))
            {
                errors.Add("League Companion Path does not point to an executable file");
                return false;
            }
            if (Settings.ValorantCompanionExe != string.Empty && (!File.Exists(Settings.ValorantCompanionExe) || Path.GetExtension(Settings.ValorantCompanionExe) != ".exe"))
            {
                errors.Add("Valorant Companion Path does not point to an executable file");
                return false;
            }
            return true;
        }
    }
}