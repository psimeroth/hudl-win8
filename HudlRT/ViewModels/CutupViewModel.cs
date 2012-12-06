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
    /// Used for binding to a list of cutups
    /// </summary>
    public class CutupViewModel : PropertyChangedBase
    {
        private string _name { get; set; }
        private int _clipCount { get; set; }
        private long _cutupId { get; set; }
        private BindableCollection<Clip> _clips { get; set; }
        private string[] _displayColumns { get; set; }
        private bool _clipLoading { get; set; }
        private double _opacity { get; set; }
        private string _thumbnail { get; set; }

        public static CutupViewModel FromDTO(CutupDTO cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.CutupID;
            cutup._clipCount = cutupDTO.ClipCount; 
            cutup._name = cutupDTO.Name;
            cutup._clipLoading = false;
            cutup._opacity = 1.0;
            cutup._thumbnail = cutupDTO.Thumbnailpath ?? "ms-appx:///Assets/Hudl_Metro150 thumbCentered.png";
            return cutup;
        }

        public static CutupViewModel FromCutup(Cutup cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.cutupId;
            cutup._clipCount = cutupDTO.clipCount;
            cutup._name = cutupDTO.name;
            cutup._clipLoading = false;
            cutup._opacity = 1.0;
            cutup._thumbnail = cutupDTO.thumbnailLocation ?? "ms-appx:///Assets/Hudl_Metro150 thumbCentered.png";

            return cutup;
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

        public long CutupId
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
    }
}
