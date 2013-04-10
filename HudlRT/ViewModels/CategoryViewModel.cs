using Caliburn.Micro;
using HudlRT.Models;

namespace HudlRT.ViewModels
{
    /// <summary>
    /// Used for binding to a list of categories
    /// </summary>
    public class CategoryViewModel : PropertyChangedBase
    {
        public Category CategoryModel { get; set; }
        private BindableCollection<PlaylistViewModel> _playlists { get; set; }

        public string Name
        {
            get { return CategoryModel.name; }
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
