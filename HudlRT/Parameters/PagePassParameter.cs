using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HudlRT.Models;
using Caliburn.Micro;
using HudlRT.ViewModels;

namespace HudlRT.Parameters
{
    public class PagePassParameter
    {
        public BindableCollection<Team> teams { get; set; }
        public BindableCollection<Season> seasons { get; set; }
        public BindableCollection<GameViewModel> games { get; set; }
        public BindableCollection<Category> categories { get; set; }
        public BindableCollection<Cutup> cutups { get; set; }
        public Team selectedTeam { get; set; }
        public Season selectedSeason { get; set; }
        public GameViewModel selectedGame { get; set; }
        public CategoryViewModel selectedCategory { get; set; }
        public Cutup selectedCutup { get; set; }
    }
}
