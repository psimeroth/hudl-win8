using Caliburn.Micro;
using HudlRT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    /// <summary>
    /// Used for binding to a list of cutups
    /// </summary>
    public class CutupViewModel : PropertyChangedBase
    {
        private const int FONT_SIZE = 28;
        private const Visibility FULL_VISIBILITY = Visibility.Visible;
        private const bool CLIP_LOADING = false;
        private const double OPACITY = 1.0;

        private long _totalCutupSize { get; set; }
        private string _name { get; set; }
        private int _clipCount { get; set; }
        private string _cutupId { get; set; }
        private BindableCollection<Clip> _clips { get; set; }
        private string[] _displayColumns { get; set; }
        private bool _clipLoading { get; set; }
        private double _opacity { get; set; }
        private string _thumbnail { get; set; }
        private Visibility _thumbnailVisibility { get; set; }
        private Visibility _nameVisibility { get; set; }
        private GridLength _width { get; set; }
        private double _fontSize { get; set; }
        private Boolean _checkbox { get; set; }
        private Visibility _downloadedVisibility { get; set; }

        private Visibility _checkbox_visibility { get; set; }

        public static CutupViewModel FromDTO(CutupDTO cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.PlaylistId;
            cutup._clipCount = cutupDTO.ClipCount; 
            cutup._name = cutupDTO.Name;
            cutup._clipLoading = CLIP_LOADING;
            cutup._opacity = OPACITY;
            cutup._thumbnail = cutupDTO.Thumbnailpath ?? "ms-appx:///Assets/Hudl_Metro150 thumbCentered.png";
            cutup._nameVisibility = FULL_VISIBILITY;
            cutup._thumbnailVisibility = FULL_VISIBILITY;
            cutup._width = new GridLength(180);
            cutup._fontSize = FONT_SIZE;
            return cutup;
        }

        public static CutupViewModel FromCutup(Cutup cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.cutupId;
            cutup._clipCount = cutupDTO.clipCount;
            cutup._name = cutupDTO.name;
            cutup._clipLoading = CLIP_LOADING;
            cutup._opacity = OPACITY;
            cutup._thumbnail = cutupDTO.thumbnailLocation ?? "ms-appx:///Assets/Hudl_Metro150 thumbCentered.png";
            cutup._nameVisibility = FULL_VISIBILITY;
            cutup._thumbnailVisibility = FULL_VISIBILITY;
            cutup._width = new GridLength(180);
            cutup._fontSize = FONT_SIZE;
            cutup._checkbox_visibility = Visibility.Collapsed;
            cutup._downloadedVisibility = Visibility.Collapsed;
            return cutup;
        }

        public Visibility DownloadedVisibility
        {
            get { return _downloadedVisibility; }
            set
            {
                _downloadedVisibility = value;
                NotifyOfPropertyChange(() => DownloadedVisibility);
            }
        } 

        public Boolean CheckBox
        {
            get { return _checkbox; }
            set
            {
                _checkbox = value;
                NotifyOfPropertyChange(() => CheckBox);
            }
        }

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

        public long TotalCutupSize
        {
            get { return _totalCutupSize; }
            set
            {
                if (value == _totalCutupSize) return;
                _totalCutupSize = value;
                NotifyOfPropertyChange(() => TotalCutupSize);
            }
        }

        public Visibility CheckBox_Visibility
        {
            get { return _checkbox_visibility; }
            set
            {
                if (value == _checkbox_visibility) return;
                _checkbox_visibility = value;
                NotifyOfPropertyChange(() => CheckBox_Visibility);
            }
        }

        public string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (value == _thumbnail) return;
                _thumbnail = value;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        public bool ClipLoading
        {
            get { return _clipLoading; }
            set
            {
                if (value == _clipLoading) return;
                _clipLoading = value;
                NotifyOfPropertyChange(() => ClipLoading);
            }
        }

        public double Opacity
        {
            get { return _opacity; }
            set
            {
                if (value == _opacity) return;
                _opacity = value;
                NotifyOfPropertyChange(() => Opacity);
            }
        }

        public BindableCollection<Clip> Clips
        {
            get { return _clips; }
            set
            {
                if (value == _clips) return;
                _clips = value;
                NotifyOfPropertyChange(() => Clips);
            }
        }

        public int ClipCount        {
            get { return _clipCount; }
            set
            {
                if (value.Equals(_clipCount)) return;
                _clipCount = value;
                NotifyOfPropertyChange(() => ClipCount);
            }
        }

        public string ClipCountDisplay
        {
            get
            {
                return ClipCount + ((ClipCount == 1) ? " Clip" : " Clips");
            }
        }

        public string CutupId
        {
            get { return _cutupId; }
            set
            {
                if (value == _cutupId) return;
                _cutupId = value;
                NotifyOfPropertyChange(() => CutupId);
            }
        }

        public string[] DisplayColumns
        {
            get { return _displayColumns; }
            set
            {
                if (value == _displayColumns) return;
                _displayColumns = value;
                NotifyOfPropertyChange(() => DisplayColumns);
            }
        }

        public Visibility Name_Visibility
        {
            get { return _nameVisibility; }
            set
            {
                if (value == _nameVisibility) return;
                _nameVisibility = value;
                NotifyOfPropertyChange(() => Name_Visibility);
            }
        }

        public Visibility Thumbnail_Visibility
        {
            get { return _thumbnailVisibility; }
            set
            {
                if (value == _thumbnailVisibility) return;
                _thumbnailVisibility = value;
                NotifyOfPropertyChange(() => Thumbnail_Visibility);
            }
        }

        public GridLength Width
        {
            get { return _width; }
            set
            {
                if (value == _width) return;
                _width = value;
                NotifyOfPropertyChange(() => Width);
            }
        }

        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                if (value == _fontSize) return;
                _fontSize = value;
                NotifyOfPropertyChange(() => FontSize);
            }
        }
    }
}
