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
        private string _clipCount { get; set; }
        private long _cutupId { get; set; }
        private BindableCollection<Clip> _clips { get; set; }
        private string[] _displayColumns { get; set; }
        private bool _clipLoading { get; set; }

        public static CutupViewModel FromDTO(CutupDTO cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.CutupID;
            string clips;
            if (cutupDTO.ClipCount == 1)
            {
                clips = " Clip";
            }
            else
            {
                clips = " Clips";
            }
            cutup._clipCount = cutupDTO.ClipCount.ToString() + clips; 
            cutup._name = cutupDTO.Name;
            cutup._clipLoading = false;
            return cutup;
        }

        public static CutupViewModel FromCutup(Cutup cutupDTO)
        {
            CutupViewModel cutup = new CutupViewModel();
            cutup._cutupId = cutupDTO.cutupId;
            cutup._clipCount = cutupDTO.clipCount.ToString();
            cutup._name = cutupDTO.name;
            cutup._clipLoading = false;
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
        public string ClipCount        {
            get { return _clipCount; }
            set
            {
                if (value.Equals(_clipCount)) return;
                _clipCount = value;
                NotifyOfPropertyChange(() => ClipCount);
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
