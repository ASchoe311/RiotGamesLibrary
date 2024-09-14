using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.IO;

namespace RiotGamesLibrary
{
    public partial class RiotGamesLibrarySettingsView : UserControl
    {
        private IPlayniteAPI playniteAPI = API.Instance;
        public RiotGamesLibrarySettingsView()
        {
            InitializeComponent();
        }

        private void ChooseLeagueCompanionBtn(object sender, RoutedEventArgs e)
        {
            var file = playniteAPI.Dialogs.SelectFile("Executable File|*.exe");
            if (file != string.Empty)
            {
                var path = System.IO.Path.GetFullPath(file);
                LeagueCompanionPathTxt.Text = path;
            }
        }

        private void ChooseValorantCompanionBtn(object sender, RoutedEventArgs e)
        {
            var file = playniteAPI.Dialogs.SelectFile("Executable File|*.exe");
            if (file != string.Empty)
            {
                var path = System.IO.Path.GetFullPath(file);
                ValorantCompanionPathTxt.Text = path;
            }
        }

    }
}