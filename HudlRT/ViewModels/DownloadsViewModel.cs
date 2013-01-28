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
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;

namespace HudlRT.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private Boolean deleting = false;

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

        private double downloadProgress;
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                NotifyOfPropertyChange(() => DownloadProgress);
            }
        }

        private Visibility cancelButton_Visibility;
        public Visibility CancelButton_Visibility
        {
            get { return cancelButton_Visibility; }
            set
            {
                cancelButton_Visibility = value;
                NotifyOfPropertyChange(() => CancelButton_Visibility);
            }
        }

        private Visibility deleteButton_Visibility;
        public Visibility DeleteButton_Visibility
        {
            get { return deleteButton_Visibility; }
            set
            {
                deleteButton_Visibility = value;
                NotifyOfPropertyChange(() => DeleteButton_Visibility);
            }
        }

        private Visibility confirmButton_Visibility;
        public Visibility ConfirmButton_Visibility
        {
            get { return confirmButton_Visibility; }
            set
            {
                confirmButton_Visibility = value;
                NotifyOfPropertyChange(() => ConfirmButton_Visibility);
            }
        }

        private Visibility progress_Visibility;
        public Visibility Progress_Visibility
        {
            get { return progress_Visibility; }
            set
            {
                progress_Visibility = value;
                NotifyOfPropertyChange(() => Progress_Visibility);
            }
        }

        private Visibility no_downloads_Visibility;
        public Visibility NoDownloadsVisibility
        {
            get { return no_downloads_Visibility; }
            set
            {
                no_downloads_Visibility = value;
                NotifyOfPropertyChange(() => NoDownloadsVisibility);
            }
        }        
        
        private Visibility backButton_Visibility;
        public Visibility BackButton_Visibility
        {
            get { return backButton_Visibility; }
            set
            {
                backButton_Visibility = value;
                NotifyOfPropertyChange(() => BackButton_Visibility);
            }
        }

        private String download_Contents;
        public String Download_Contents
        {
            get { return download_Contents; }
            set
            {
                download_Contents = value;
                NotifyOfPropertyChange(() => Download_Contents);
            }
        }

        public DownloadsViewModel(INavigationService navigationService): base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            Cutups = new BindableCollection<CutupViewModel>();
            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            NoDownloadsVisibility = Visibility.Collapsed;
            Progress_Visibility = Visibility.Collapsed;
            if (!ServiceAccessor.ConnectedToInternet())
            {
                BackButton_Visibility = Visibility.Collapsed;
            }
            await GetDownloads();
        }

        private void Cutups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NoDownloadsVisibility = Cutups.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        public async void CutupSelected(ItemClickEventArgs eventArgs)
        {
            var cutup = (CutupViewModel)eventArgs.ClickedItem;
            if (!deleting)
            {     
                CachedParameter.selectedCutup = new Cutup { cutupId = cutup.CutupId, clips = cutup.Clips, displayColumns = cutup.DisplayColumns, clipCount = cutup.ClipCount, name = cutup.Name };
                CachedParameter.sectionViewCutupSelected = cutup;
                navigationService.NavigateToViewModel<VideoPlayerViewModel>();
            }
            else
            {
                cutup.CheckBox = !cutup.CheckBox;
                CheckBoxSelected();
            }
        }

        public void CheckBoxSelected()
        {
            bool checkFound = false;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                if (cutupVM.CheckBox)
                {
                    checkFound = true;
                    ConfirmButton_Visibility = Visibility.Visible;
                }
            }
            if (!checkFound)
            {
                ConfirmButton_Visibility = Visibility.Collapsed;
            }
        }

        public void GoBack()
        {
            navigationService.GoBack();
        }

        public void Delete_Playlists()
        {
            deleting = true;
            DeleteButton_Visibility = Visibility.Collapsed;
            CancelButton_Visibility = Visibility.Visible;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupVM.CheckBox_Visibility = Visibility.Visible;
                if (cutupVM.CheckBox)
                {
                    ConfirmButton_Visibility = Visibility.Visible;
                }
            }
        }

        public void Cancel_Delete()
        {
            deleting = false; 
            DeleteButton_Visibility = Visibility.Visible;
            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            foreach (CutupViewModel cutupVM in Cutups)
            {
                cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                cutupVM.CheckBox = false;
            }
        }

        public async void Confirm_Delete()
        {
            CutupViewModel[] copy = new CutupViewModel[Cutups.Count];
            Cutups.CopyTo(copy, 0);
            foreach (CutupViewModel cutupVM in copy)
            {
                if (cutupVM.CheckBox)
                {
                    await RemoveDownload(cutupVM);
                    Cutups.Remove(cutupVM);
                }
            }

            CancelButton_Visibility = Visibility.Collapsed;
            ConfirmButton_Visibility = Visibility.Collapsed;
            var totalClips = 0;
            if (!Cutups.Any())
            {
                DeleteButton_Visibility = Visibility.Collapsed;
                NoDownloadsVisibility = Visibility.Visible;
            }
            else
            {
                DeleteButton_Visibility = Visibility.Visible;
                NoDownloadsVisibility = Visibility.Collapsed;
                foreach (CutupViewModel cutupVM in Cutups)
                {
                    cutupVM.CheckBox_Visibility = Visibility.Collapsed;
                    totalClips += cutupVM.ClipCount;
                }
            }
            long totalsize = 0;
            foreach (CutupViewModel c in Cutups)
            {
                totalsize += c.TotalCutupSize;
            }
            long megabytes = (totalsize / 1048576);
            if (Cutups.Count > 0 && megabytes < 1)
            {
                megabytes = 1;
            }
            Download_Contents = "Cutups: " + Cutups.Count + " | Clips: " + totalClips + " | Size: " + megabytes + " MB";
        }


        private async Task GetDownloads()
        {
            long totalsize = 0;
            var totalClips = 0;
            Cutups = await CachedParameter.downloadAccessor.GetDownloads();
            if (!Cutups.Any())
            {
                DeleteButton_Visibility = Visibility.Collapsed;
                NoDownloadsVisibility = Visibility.Visible;
            }
            foreach (CutupViewModel cVM in Cutups)
            {
                totalsize += cVM.TotalCutupSize;
                totalClips += cVM.ClipCount;
            }
            long megabytes = (totalsize / (1048576));
            if (Cutups.Count > 0 && megabytes < 1)
            {
                megabytes = 1;
            }
            Download_Contents = "Cutups: " + Cutups.Count + " | Clips: " + totalClips + " | Size: " + megabytes + " MB";
            Cutups.CollectionChanged += Cutups_CollectionChanged;
        }

        private async Task RemoveDownload(CutupViewModel cutup)
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(AppDataAccessor.GetUsername() + cutup.CutupId.ToString());
                try
                {
                    folder.DeleteAsync();
                    CachedParameter.downloadedCutups.Remove(cutup);
                }
                catch (Exception) { }
            }
            catch (Exception)
            {

            }
        }

        
    }

}
