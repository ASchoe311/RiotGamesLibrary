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
using System.Collections.ObjectModel;
using System.Globalization;

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
        //public class RowConverter : IMultiValueConverter
        //{
        //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        //    {
        //        return Tuple.Create(values[0], values[1]);
        //    }
        //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }

}
