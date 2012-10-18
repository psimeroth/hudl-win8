using HudlRT.Common;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPlayerView : LayoutAwarePage
    {
        private int selectedIndex { get; set; }
        private bool rightClicked { get; set; }
        private bool _isFullscreenToggle = false;
        public bool IsFullscreen
        {
            get { return _isFullscreenToggle; }
            set { _isFullscreenToggle = value; }
        }

        private Size _previousVideoContainerSize = new Size();
        private Size _previousVideoSize = new Size();

        private string _rootNamespace;

        private Brush background;
        double expandedWidth;
        double shrunkenWidth;

        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }

        public VideoPlayerView()
        {
            this.InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var library = Windows.Storage.KnownFolders.VideosLibrary;
            var queryOptions = new Windows.Storage.Search.QueryOptions();
            queryOptions.FolderDepth = Windows.Storage.Search.FolderDepth.Deep;
            queryOptions.IndexerOption = Windows.Storage.Search.IndexerOption.UseIndexerWhenAvailable;

            var fileQuery = library.CreateFileQueryWithOptions(queryOptions);

            var fif = new Windows.Storage.BulkAccess.FileInformationFactory(fileQuery,
                Windows.Storage.FileProperties.ThumbnailMode.VideosView, 190,
                Windows.Storage.FileProperties.ThumbnailOptions.UseCurrentScale, false);

            var dataSource = fif.GetVirtualizedFilesVector();
            
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void VideosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Clips.ScrollIntoView(((Windows.UI.Xaml.Controls.ListView)sender).SelectedItem);

            if (rightClicked)
            {
                ListView l = (ListView)sender;
                l.SelectedIndex = selectedIndex;
                rightClicked = false;
            }
        }

        private void FullscreenToggle()
        {
            this.IsFullscreen = !this.IsFullscreen;

            if (this.IsFullscreen)
            {
                background = RootGrid.Background;
                RootGrid.Background = new SolidColorBrush();
                dataPanel.Background = new SolidColorBrush();
                // Hide all non full screen controls
                header.Visibility = Visibility.Collapsed;
                header.UpdateLayout();
                Clips.Visibility = Visibility.Collapsed;
                Clips.UpdateLayout();
                TransportControlsPanel_Left.Visibility = Visibility.Collapsed;
                TransportControlsPanel_Right.Visibility = Visibility.Collapsed;
                gridHeaders.Visibility = Visibility.Collapsed;

                // Show the full screen controls
                full_mainGrid.Visibility = Visibility.Visible;

                // Save the video containers size
                _previousVideoContainerSize.Width = videoContainer.ActualWidth;
                _previousVideoContainerSize.Height = videoContainer.ActualHeight;
                _previousVideoSize.Width = videoMediaElement.ActualWidth;
                _previousVideoSize.Height = videoMediaElement.ActualHeight;

                // Set the video container to fullscreen
                videoContainer.Width = Window.Current.Bounds.Width;
                videoContainer.Height = Window.Current.Bounds.Height;
                videoMediaElement.Width = Window.Current.Bounds.Width;
                videoMediaElement.Height = Window.Current.Bounds.Height;
                VideoGrid.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                RootGrid.Background = background;
                // Show the non full screen controls
                header.Visibility = Visibility.Visible;
                Clips.Visibility = Visibility.Visible;
                TransportControlsPanel_Left.Visibility = Visibility.Visible;
                TransportControlsPanel_Right.Visibility = Visibility.Visible;
                gridHeaders.Visibility = Visibility.Visible;

                // Hide the full screen controls
                full_mainGrid.Visibility = Visibility.Collapsed;

                // Reset the video container to it's original height
                videoContainer.Width = _previousVideoContainerSize.Width;
                videoContainer.Height = _previousVideoContainerSize.Height;
                videoMediaElement.Width = _previousVideoSize.Width;
                videoMediaElement.Height = _previousVideoSize.Height;
                
                VideoGrid.Margin = new Thickness(0, 70, 0, 0);
            }
        }

        private void btnFullScreenToggle_Click(object sender, RoutedEventArgs e)
        {
            FullscreenToggle();
        }

        private void VideoContainer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (IsFullscreen && e.Key == Windows.System.VirtualKey.Escape)
            {
                FullscreenToggle();
            }

            e.Handled = true;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (videoMediaElement.DefaultPlaybackRate != 1)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
                videoMediaElement.PlaybackRate = 1.0;
            }

            videoMediaElement.Play();

            // Here we need to collapse and expand both full and non full screen buttons
            setPauseVisible();
            setStopVisibile();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.Pause();

            // Here we need to collapse and expand both full and non full screen buttons
            setPlayVisible();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.Stop();
            setPlayVisible();
            setPrevVisible();
        }

        private void btnFastForward_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = 2.0;
            videoMediaElement.Play();
            
            setPauseVisible();
            setStopVisibile();
        }

        private void btnFastReverse_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = -2.0;
            videoMediaElement.Play();
            setPauseVisible();
        }

        private void btnSlowReverse_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = -0.5;
            videoMediaElement.Play();
            setPauseVisible();
        }

        private void btnSlowForward_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = 0.5;
            videoMediaElement.Play();
            
            setPauseVisible();
            setStopVisibile();
        }

        void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            setPlayVisible();
            expandedWidth = videoMediaElement.ActualWidth;
            shrunkenWidth = expandedWidth * .7;
        }

        void videoMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            setPlayVisible();
            setPrevVisible();
        }

        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string hr = GetHresultFromErrorMessage(e);

        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10; 

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return hr;
        }

        private void videoMediaElement_Tapped(object sender, TappedRoutedEventArgs e)
        {

            if (btnPlay.Visibility == Visibility.Collapsed)
            {
                btnPause_Click(null, null);
            }
            else
            {
                btnPlay_Click(null, null);
            }
        }

        private void setPlayVisible()
        {
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;

            full_btnPause.Visibility = Visibility.Collapsed;
            full_btnPlay.Visibility = Visibility.Visible;
        }

        private void setPauseVisible()
        {
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;

            full_btnPlay.Visibility = Visibility.Collapsed;
            full_btnPause.Visibility = Visibility.Visible;
        }

        private void setStopVisibile()
        {
            btnReverse.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;

            full_btnReverse.Visibility = Visibility.Collapsed;
            full_btnStop.Visibility = Visibility.Visible;
        }

        private void setPrevVisible()
        {
            btnStop.Visibility = Visibility.Collapsed;
            btnReverse.Visibility = Visibility.Visible;

            full_btnStop.Visibility = Visibility.Collapsed;
            full_btnReverse.Visibility = Visibility.Visible;
        }

        private void btnExpandGrid_Click(object sender, RoutedEventArgs e)
        {
            btnExpandGrid.Visibility = Visibility.Collapsed;
            btnCollapseGrid.Visibility = Visibility.Visible;

            double width = videoMediaElement.ActualWidth * .7;

            mainGrid.RowDefinitions.ElementAt(1).Height = new GridLength(375);
            Container1.RowDefinitions.First().Height = new GridLength(375);

            videoContainer.Height = 350;
            videoMediaElement.Height = 350;
            videoMediaElement.Width = shrunkenWidth;
       
        }

        private void btnCollapseGrid_Click(object sender, RoutedEventArgs e)
        {
            btnCollapseGrid.Visibility = Visibility.Collapsed;
            btnExpandGrid.Visibility = Visibility.Visible;

            mainGrid.RowDefinitions.ElementAt(1).Height = new GridLength(525);
            Container1.RowDefinitions.First().Height = new GridLength(525);

            videoContainer.Height = 500;
            videoMediaElement.Height = 500;
            videoMediaElement.Width = expandedWidth;
        }

        private void ListViewItemPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ListView l = (ListView)sender;
            selectedIndex = l.SelectedIndex;
            rightClicked = true;
            e.Handled = true;
        }
    }
}
