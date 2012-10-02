using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HudlRT.Parameters;
using HudlRT.Models;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;

namespace HudlRT.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public PagePassParameter Parameter { get; set; }
        private BindableCollection<Clip> clips;
        public BindableCollection<Clip> Clips
        {
            get { return clips; }
            set
            {
                clips = value;
                NotifyOfPropertyChange(() => Clips);
            }
        }
        private Angle video;
        public Angle Video
        {
            get { return video; }
            set
            {
                video = value;
                NotifyOfPropertyChange(() => Video);
            }
        }

        public VideoPlayerViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            GetClipsByCutup(Parameter.Value);
        }

        public async void GetClipsByCutup(Cutup cutup)
        {
            var clips = await ServiceAccessor.MakeApiCallGet(ServiceAccessor.URL_SERVICE_GET_CLIPS.Replace("#", cutup.cutupId.ToString()));
            if (!clips.Equals(""))
            {
                cutup.clips = new BindableCollection<Clip>();
                var obj = JsonConvert.DeserializeObject<ClipResponseDTO>(clips);
                foreach (ClipDTO clipDTO in obj.ClipsList.Clips)
                {
                    Clip c = Clip.FromDTO(clipDTO);
                    if (c != null)
                    {
                        cutup.clips.Add(c);
                    }
                }
                Clips = cutup.clips;
            }
            else
            {
                
            }
            //(if Clips.count == 0) .. do something figure this out earlier somehow?
        }

        public void ClipSelected(ItemClickEventArgs eventArgs)
        {
            var clip = (Clip)eventArgs.ClickedItem;
            Video = clip.angles.ElementAt(0);
        }

        public void GoBack()
        {
            navigationService.NavigateToViewModel<HubViewModel>(Parameter);
        }
    }
}
