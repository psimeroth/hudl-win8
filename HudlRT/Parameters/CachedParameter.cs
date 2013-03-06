using Caliburn.Micro;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace HudlRT.Parameters
{
    public static class CachedParameter
    {
        public static Season season { get; set; }
        public static Playlist playlist { get; set; }
        public static BindableCollection<HubGroupViewModel> hubGroups { get; set; }
        public static BindableCollection<Season> hubSeasons { get; set; }
        public static Season hubSelectedSeason { get; set; }
        public static BindableCollection<CategoryViewModel> sectionCategories { get; set; }
        public static string sectionGameId { get; set; }

        public static void resetCache(){
            playlist = null;
            hubGroups = null;
            season = null;
        }
    }
}
