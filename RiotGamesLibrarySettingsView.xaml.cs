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
using Microsoft.Win32;
using Shell32;

namespace RiotGamesLibrary
{
    public partial class RiotGamesLibrarySettingsView : UserControl
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI playniteAPI = API.Instance;
        public RiotGamesLibrarySettingsView()
        {
            InitializeComponent();
        }

        private Tuple<string, string> SelectApp()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.DereferenceLinks = false;
            openFileDialog.Filter = "Executable file (.exe)|*.exe";

            Nullable<bool> result = openFileDialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string targetname = openFileDialog.FileName;
                string args = string.Empty;
                if (openFileDialog.FileName.Contains(".lnk"))
                {
                    //logger.Debug("File chosen is a shortcut, trying to extract target and args");
                    // Open document
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
                return new Tuple<string, string>(targetname, args);
            }
            return null;
        }

        private void ChooseLeagueCompanionBtn(object sender, RoutedEventArgs e)
        {

            Tuple<string, string> companion = SelectApp();
            if (companion != null)
            {
                LeagueCompanionPathTxt.Text = companion.Item1;
                LeagueCompanionPathArgsTxt.Text = companion.Item2;
            }
        }

        private void ChooseValorantCompanionBtn(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> companion = SelectApp();
            if (companion != null)
            {
                ValorantCompanionPathTxt.Text = companion.Item1;
                ValorantCompanionPathArgsTxt.Text = companion.Item2;
            }
        }
    }
}
