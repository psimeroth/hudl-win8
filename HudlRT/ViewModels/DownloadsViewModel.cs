using Caliburn.Micro;
using HudlRT.Models;
using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using HudlRT.Parameters;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace HudlRT.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        public CachedParameter Parameter { get; set; }
       

        private BindableCollection<CutupViewModel> _cutups { get; set; }
        public BindableCollection<CutupViewModel> Cutups
        {
            get { return _cutups; }
            set
            {
                _cutups = value;
                NotifyOfPropertyChange(() => Cutups);
            }
        }

        public DownloadsViewModel(INavigationService navigationService): base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            await GetDownloads();
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            await GetClipsByCutup(cutup);
        }

        public async Task GetClipsByCutup(CutupViewModel cutup)
        {
            ClipResponse response = await ServiceAccessor.GetCutupClips(cutup);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                cutup.Clips = response.clips;
                string[] clipCount = cutup.ClipCount.ToString().Split(' ');
                //UpdateCachedParameter();
                Parameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = Int32.Parse(clipCount[0]), name = cutup.Name };
                Parameter.sectionViewCutupSelected = cutup;
                //Parameter.videoPageClips = Parameter.selectedCutup.clips;

                //await GetDownloads();
                //await DownloadCutups(new List<Cutup>{Parameter.selectedCutup});
                //await RemoveDownload(Parameter.selectedCutup);
                navigationService.NavigateToViewModel<VideoPlayerViewModel>(Parameter);
            }
            else
            {
                Common.APIExceptionDialog.ShowExceptionDialog(null, null);
            }
        }

        private async Task GetDownloads()
        {
            Cutups = new BindableCollection<CutupViewModel>();
            var downloadFolders = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Downloads downloads = new Downloads();
            foreach (StorageFolder folder in downloadFolders)
            {
                if (folder.Name.Contains(AppDataAccessor.GetUsername()))
                {
                    StorageFile model = await folder.GetFileAsync("DownloadsModel");
                    string text = await Windows.Storage.FileIO.ReadTextAsync(model);
                    Cutup savedCutup = JsonConvert.DeserializeObject<Cutup>(text);
                    CutupViewModel cutupVM = new CutupViewModel {ClipCount = savedCutup.clipCount, Clips = savedCutup.clips, Name = savedCutup.name, Thumbnail = savedCutup.thumbnailLocation, CutupId = savedCutup.cutupId, DisplayColumns = savedCutup.displayColumns};
                    Cutups.Add(cutupVM);
                }
            }
        }

        
    }

}
