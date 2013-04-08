using Caliburn.Micro;

namespace HudlRT.ViewModels
{
    public class FilterCriteriaViewModel : PropertyChangedBase
    {
        public string Name { get; set; }
        private int id { get; set; }
        public VideoPlayerViewModel viewModel { get; set; }
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyOfPropertyChange(() => IsChecked);
            }
        }

        public FilterCriteriaViewModel(int id, string name, VideoPlayerViewModel viewModel)
        {
            this.id = id;
            Name = name;
            IsChecked = false;
            this.viewModel = viewModel;
        }

        public void ApplyFilter()
        {
            viewModel.ApplySelectedFilter();
        }
    }
}
