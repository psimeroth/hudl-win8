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

            catVM.Playlists = new BindableCollection<CutupViewModel>();

            return catVM;
        }
    }
}
