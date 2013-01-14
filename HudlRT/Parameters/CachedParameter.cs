using Caliburn.Micro;
using HudlRT.Models;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.Parameters
{
    public class CachedParameter
    {
        public string categoryId { get; set; }
        public string gameId { get; set; }
        public BindableCollection<Season> seasonsDropDown { get; set; }
        public Season seasonSelected { get; set; }
        public Game hubViewNextGame { get; set; }
        public Game hubViewPreviousGame { get; set; }

        public BindableCollection<GameViewModel> sectionViewGames { get; set; }
        public GameViewModel sectionViewGameSelected { get; set; }

        public BindableCollection<CategoryViewModel> sectionViewCategories { get; set; }
        public CategoryViewModel sectionViewCategorySelected { get; set; }

        public BindableCollection<CutupViewModel> sectionViewCutups { get; set; }

        public CutupViewModel sectionViewCutupSelected { get; set; }

        public BindableCollection<Clip> videoPageClips { get; set; }

        public Cutup selectedCutup { get; set; }
    }
}
