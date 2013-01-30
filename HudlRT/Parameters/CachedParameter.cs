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

namespace HudlRT.Parameters
{
    public static class CachedParameter
    {
        public static bool isInitialized = false;
        public static string categoryId { get; set; }
        public static string gameId { get; set; }
        public static BindableCollection<Season> seasonsDropDown { get; set; }
        public static Season seasonSelected { get; set; }
        public static Game hubViewNextGame { get; set; }
        public static Game hubViewPreviousGame { get; set; }
                
        public static BindableCollection<GameViewModel> sectionViewGames { get; set; }
        public static GameViewModel sectionViewGameSelected { get; set; }
                
        public static BindableCollection<CategoryViewModel> sectionViewCategories { get; set; }
        public static CategoryViewModel sectionViewCategorySelected { get; set; }
                
        public static BindableCollection<CutupViewModel> sectionViewCutups { get; set; }
                
        public static CutupViewModel sectionViewCutupSelected { get; set; }
                
        public static BindableCollection<Clip> videoPageClips { get; set; }
                
        public static Cutup selectedCutup { get; set; }

        public static BindableCollection<CutupViewModel> downloadedCutups { get; set; }

        public static CancellationTokenSource cts = new CancellationTokenSource();

        public static bool noConnection = false;
        


        public static void resetCache(){
            isInitialized = false;
            categoryId = null;
            gameId = null;
            seasonsDropDown = null;
            seasonSelected = null;
            hubViewNextGame = null;
            hubViewPreviousGame = null;
            sectionViewGames = null;
            sectionViewGameSelected = null;
            sectionViewCategories = null;
            sectionViewCategorySelected = null;
            sectionViewCutups = null;
            sectionViewCutupSelected = null;
            videoPageClips = null;
            selectedCutup = null;
            downloadedCutups = null;
        }
    }
}
