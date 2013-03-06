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
        public Category CategoryModel { get; set; }
        //private string _name { get; set; }
        private BindableCollection<PlaylistViewModel> _playlists { get; set; }

        public string Name
        {
            get { return CategoryModel.name; }
            /*set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }*/
        }

        public BindableCollection<PlaylistViewModel> Playlists
        {
            get { return _playlists; }
            set
            {
                if (value == _playlists) return;
                _playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public CategoryViewModel(Category cat)
        {
            CategoryModel = cat;            
            Playlists = new BindableCollection<PlaylistViewModel>();
        }
    }
}
