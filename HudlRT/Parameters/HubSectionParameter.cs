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
    public class HubSectionParameter
    {
        public long categoryId { get; set; }
        public long gameId { get; set; }
        public BindableCollection<Season> seasonsDropDown { get; set; }
        public Season seasonSelected { get; set; }
        public Game nextGame { get; set; }
        public Game previousGame { get; set; }
        public BindableCollection<GameViewModel> games { get; set; }
    }
}
