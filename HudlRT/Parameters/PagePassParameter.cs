using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HudlRT.Models;
using Caliburn.Micro;

namespace HudlRT.Parameters
{
    public class PagePassParameter
    {
        public Cutup Value { get; set; }
        public BindableCollection<Team> teams { get; set; }
        public BindableCollection<Season> seasons { get; set; }
        public BindableCollection<Game> games { get; set; }
        public BindableCollection<Category> categories { get; set; }
        public BindableCollection<Cutup> cutups { get; set; }
    }
}
