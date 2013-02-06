using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    /// <summary>
    /// Used for binding to a list of categories
    /// </summary>
    public class CategoryViewModel : PropertyChangedBase
    {
        public string CategoryID { get; set; }

        private string _name { get; set; }
        private BindableCollection<CutupViewModel> _playlists { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public BindableCollection<CutupViewModel> Playlists
        {
            get { return _playlists; }
            set
            {
                if (value == _playlists) return;
                _playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public static CategoryViewModel FromCategory(Category cat)
        {
            CategoryViewModel catVM = new CategoryViewModel();
            catVM.Name = cat.name;
            catVM.CategoryID = cat.categoryId;

            catVM.Playlists = new BindableCollection<CutupViewModel>();

            Cutup cut = new Cutup()
            {
	            clipCount=133,
	            clips = new BindableCollection<Clip>(),
	            cutupId = "4813205",
	            displayColumns= null,
	            name = "HHS vs HHS 11/2/2012",
	            thumbnailLocation = "http://vh.hudl.com/7/7/57083/1087992/RK/001/7q8s_L.jpg"
            };
            catVM.Playlists.Add(CutupViewModel.FromCutup(cut));

            return catVM;
        }
    }
}
