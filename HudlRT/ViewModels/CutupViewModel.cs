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
        public string Name { get; set; }
        public string NumClips { get; set; }
        public bool IsDownloaded { get; set; }

        public static CutupViewModel FromCutup(Cutup cutup)
        {
            CutupViewModel cvm = new CutupViewModel();
            cvm.Name = cutup.name;
            cvm.NumClips = cutup.clipCount.ToString();
            cvm.IsDownloaded = false;
            return cvm;
        }
    }
}
