using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HudlRT.ViewModels
{
    public class FilterViewModel : PropertyChangedBase
    {
        public int columnId;
        public SortType sortType;
        public VideoPlayerViewModel viewModel { get; set; }
        private BindableCollection<FilterCriteriaViewModel> filterCriteria;
        public BindableCollection<FilterCriteriaViewModel> FilterCriteria
        {
            get { return filterCriteria; }
            set
            {
                filterCriteria = value;
                NotifyOfPropertyChange(() => FilterCriteria);
            }
        }
        private string columnHeaderName;
        public string ColumnHeaderName
        {
            get { return columnHeaderName; }
            set
            {
                columnHeaderName = value;
                NotifyOfPropertyChange(() => ColumnHeaderName);
            }
        }
        private bool isAscendingChecked;
        public bool IsAscendingChecked
        {
            get { return isAscendingChecked; }
            set
            {
                isAscendingChecked = value;
                NotifyOfPropertyChange(() => IsAscendingChecked);
            }
        }
        private bool isDescendingChecked;
        public bool IsDescendingChecked
        {
            get { return isDescendingChecked; }
            set
            {
                isDescendingChecked = value;
                NotifyOfPropertyChange(() => IsDescendingChecked);
            }
        }
        private bool isNoneChecked;
        public bool IsNoneChecked
        {
            get { return isNoneChecked; }
            set
            {
                isNoneChecked = value;
                NotifyOfPropertyChange(() => IsNoneChecked);
            }
        }
        private string applyButtonVisibility;
        public string ApplyButtonVisibility
        {
            get { return applyButtonVisibility; }
            set
            {
                applyButtonVisibility = value;
                NotifyOfPropertyChange(() => ApplyButtonVisibility);
            }
        }
        private string removeButtonVisibility;
        public string RemoveButtonVisibility
        {
            get { return removeButtonVisibility; }
            set
            {
                removeButtonVisibility = value;
                NotifyOfPropertyChange(() => RemoveButtonVisibility);
            }
        }
        private string closeButtonVisibility;
        public string CloseButtonVisibility
        {
            get { return closeButtonVisibility; }
            set
            {
                closeButtonVisibility = value;
                NotifyOfPropertyChange(() => CloseButtonVisibility);
            }
        }

        public FilterViewModel(int columnId, string columnName, SortType sortType, BindableCollection<FilterCriteriaViewModel> filterCriteria, VideoPlayerViewModel viewModel)
        {
            this.columnId = columnId;
            this.ColumnHeaderName = columnName;
            this.sortType = sortType;
            this.filterCriteria = filterCriteria;
            this.viewModel = viewModel;
            IsAscendingChecked = false;
            IsDescendingChecked = false;
            IsNoneChecked = true;
            ApplyButtonVisibility = "Visible";
            RemoveButtonVisibility = "Collapsed";
            CloseButtonVisibility = "Visible";
        }

        public void ApplyFilter()
        {
            viewModel.ApplySelectedFilter();
        }

        public void RemoveFilter()
        {
            viewModel.RemoveSelectedFilter();
        }

        public void Click(FilterViewModel filter, SortType sortType)
        {
            filter.sortType = sortType;
        }

        public void setSortType(SortType newSortType)
        {
            sortType = newSortType;

            switch (sortType)
            {
                case SortType.Ascending:
                    IsAscendingChecked = true;
                    IsDescendingChecked = false;
                    IsNoneChecked = false;
                    break;
                case SortType.Descending:
                    IsAscendingChecked = false;
                    IsDescendingChecked = true;
                    IsNoneChecked = false;
                    break;
                case SortType.None:
                    IsAscendingChecked = false;
                    IsDescendingChecked = false;
                    IsNoneChecked = true;
                    break;
            }
        }
    }
}
