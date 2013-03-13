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
using HudlRT.Models;
using HudlRT.Parameters;
using Windows.UI.Xaml.Markup;
using Windows.UI;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Documents;
using WinRTXamlToolkit.Controls.Extensions;

namespace HudlRT.Views
{
    public sealed partial class VideoPlayerView : LayoutAwarePage
    {
        private const int POPUP_WIDTH = 346;
        private const int COLUMN_WIDTH = 130;
        private const int GRID_HEADER_FONT_SIZE = 22;

        private Grid timelineContainer;

        private bool rightClicked { get; set; }
        private bool itemClicked { get; set; }
        private string _rootNamespace;
        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }
        private int count = 0;
        private TranslateTransform dragTranslation;
        private System.Diagnostics.Stopwatch keyPressTimer = new System.Diagnostics.Stopwatch();
        private long keyPressLength = 225;
        private DispatcherTimer rewindTimer { get; set; }
        private ScrollViewer filteredListScrollViewer { get; set; }
        private bool isFastRewind { get; set; }
        private Stopwatch rewindStopwatch { get; set; }
        private TimeSpan rewindPosition { get; set; }
        private VideoPlayerState playerState { get; set; }
        private DispatcherTimer rewindKeyPressTimer { get; set; }
        private Windows.UI.Core.KeyEventArgs rewindKey { get; set; }

        public VideoPlayerView()
        {
            this.InitializeComponent();

            FilteredClips.RenderTransform = this.dragTranslation;
            FilteredClips.Loaded += filteredClips_Loaded;

            btnFastForward.AddHandler(PointerPressedEvent, new PointerEventHandler(btnFastForward_Click), true);
            btnSlowForward.AddHandler(PointerPressedEvent, new PointerEventHandler(btnSlowForward_Click), true);
            btnFastReverse.AddHandler(PointerPressedEvent, new PointerEventHandler(btnFastReverse_Click), true);
            btnSlowReverse.AddHandler(PointerPressedEvent, new PointerEventHandler(btnSlowReverse_Click), true);

            Window.Current.CoreWindow.KeyDown += VideoPage_KeyDown;
            Window.Current.CoreWindow.KeyUp += VideoPage_KeyUp;

            rewindTimer = new DispatcherTimer();
            rewindTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            rewindTimer.Tick += rewindTimerTick;
            rewindStopwatch = new Stopwatch();
            playerState = VideoPlayerState.Paused;

            rewindKeyPressTimer = new DispatcherTimer();
            rewindKeyPressTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            rewindKeyPressTimer.Tick += rewindKeyPressTimerTick;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            videoMediaElement.Width = Window.Current.Bounds.Width;
            videoMediaElement.Height = Window.Current.Bounds.Height;
            VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
            vm.GridHeadersTextSorted = new List<string>();
            vm.GridHeadersTextUnsorted = new List<string>();
            initializeGrid(vm);

            vm.listView = FilteredClips;
            vm.SortFilterPopupControl = SortFilterPopup;
            vm.ColumnHeaderTextBlocks = gridHeaders.Children.Select(border => (TextBlock)((Border)border).Child).ToList<TextBlock>();
            vm.setVideoMediaElement(videoMediaElement);
        }

        private void initializeGrid(VideoPlayerViewModel vm)
        {
			Playlist playlist = vm.Parameter.playlist;
            double screenWidth = Window.Current.Bounds.Width;
            string[] displayColumns = playlist.displayColumns;
            var template = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""> <Grid MinWidth='" + screenWidth + "'> <Grid.ColumnDefinitions> @ </Grid.ColumnDefinitions> % </Grid> </DataTemplate>";
            string columnDefinitions = "";
            string rowText = "";
            if (displayColumns != null)
            {
                for (int i = 0; i < displayColumns.Length; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(130);
                    gridHeaders.ColumnDefinitions.Add(col);
                    columnDefinitions += String.Format(@"<ColumnDefinition Width=""{0}"" /> ", COLUMN_WIDTH);
                    rowText = rowText + @"<TextBlock Grid.Column=""X"" HorizontalAlignment = ""Center"" TextWrapping=""NoWrap"" VerticalAlignment=""Center"" Text =""{Binding Path=breakDownData[X]}""/>".Replace("X", i.ToString());
                    Border b = new Border();
                    b.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0, 0));
                    b.BorderThickness = new Thickness(0, 0, 1, 0);
                    TextBlock t = new TextBlock();
                    Run text = new Run();
                    vm.GridHeadersTextSorted.Add(trimHeaderText(displayColumns[i], true));
                    vm.GridHeadersTextUnsorted.Add(trimHeaderText(displayColumns[i], false));
                    text.Text = vm.GridHeadersTextUnsorted.Last();
                    t.Inlines.Add(text);
                    b.SetValue(Grid.RowProperty, 0);
                    b.SetValue(Grid.ColumnProperty, i);
                    t.Style = (Style)Application.Current.Resources["VideoPlayer_TextBlockStyle_GridHeader"];
                    
                    t.Tag = i;
                    t.PointerReleased += columnHeaderClick;

                    b.Child = t;
                    t.FontSize = GRID_HEADER_FONT_SIZE;
                    gridHeaders.Children.Add(b);
                }
            }
            template = template.Replace("@", columnDefinitions).Replace("%", rowText);

            var dt = (DataTemplate)XamlReader.Load(template);
            FilteredClips.ItemTemplate = dt;
        }

        private string trimHeaderText(string headerText, bool addIcon)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = headerText;
            textBlock.FontSize = GRID_HEADER_FONT_SIZE;

            if (addIcon)
            {
                Run text = new Run();
                text.Text = " \u25B2";
                textBlock.Inlines.Add(text);
            }

            textBlock.Measure(new Size(double.MaxValue, double.MaxValue));

            if(textBlock.ActualWidth > COLUMN_WIDTH - 10){
                while (textBlock.ActualWidth > COLUMN_WIDTH - 10)
                {
                    headerText = headerText.Remove(headerText.Length - 1).Trim();
                    textBlock.Text = String.Concat(headerText, "...");
                    if (addIcon)
                    {
                        Run text = new Run();
                        text.Text = " \u25B2";
                        textBlock.Inlines.Add(text);
                    }
                    textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
                }
                headerText = String.Concat(headerText, "...");
            }
            
            return headerText;
        }

        private void filteredClips_Loaded(object sender, RoutedEventArgs e)
        {
            filteredListScrollViewer = FilteredClips.GetFirstDescendantOfType<ScrollViewer>();
            filteredListScrollViewer.ViewChanged += filteredListScrollViewer_ViewChanged;
        }

        void filteredListScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            gridHeaderScroll.ScrollToHorizontalOffset(filteredListScrollViewer.HorizontalOffset);
        }

        void gridHeaderScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (filteredListScrollViewer != null)
            {
                filteredListScrollViewer.ScrollToHorizontalOffset(gridHeaderScroll.HorizontalOffset);
            }
        }

        private void columnHeaderClick(object sender, PointerRoutedEventArgs e)
        {
            int id = (int)((TextBlock)sender).Tag;
            
            VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
            vm.PrepareSortFilterPopup(id);

            if (!SortFilterPopup.IsOpen)
            {
                RootPopupBorder.Width = POPUP_WIDTH;
                SortFilterPopup.HorizontalOffset = Window.Current.Bounds.Width - POPUP_WIDTH;

                var currentViewState = ApplicationView.Value;
                if (currentViewState != ApplicationViewState.Filled)
                {
                    SortFilterPopup.IsOpen = true;
                }
            }
            
            /* 
             * RemoveFilterBtn visibility is set to visible by default, so we do not want to display it the first time a grid header is clicked.
             * So, we use a variable count to ensure we don't display the RemoveFilterBtn the first time a header is clicked.
             */
            if (RemoveFilterBtn.Visibility == Visibility.Visible && count > 0 && FilterButtonsGrid.ColumnDefinitions.Count < 3)
            {
                FilterButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetColumn(CloseBtn, 2);

            }
            else
            {
                if (FilterButtonsGrid.ColumnDefinitions.Count > 2 && RemoveFilterBtn.Visibility == Visibility.Collapsed)
                {
                    FilterButtonsGrid.ColumnDefinitions.RemoveAt(2);
                    Grid.SetColumn(CloseBtn, 1);
                }
            }

            //Only increment count when a header other than Play # is clicked
            if (id != 0)
            {
                count++;
            }
        }

        private void closeSettingsPopupClicked(object sender, RoutedEventArgs e)
        {
            if (SortFilterPopup.IsOpen) { SortFilterPopup.IsOpen = false; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            gridHeaders.Children.Clear();
            gridHeaders.ColumnDefinitions.Clear();
        }

        private void VideosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightClicked)
            {
                ListView listView = (ListView)sender;
                VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
                vm.SetClip((Clip)listView.SelectedItem);

                rightClicked = false;
            }
            else if (itemClicked)
            {
                itemClicked = false;
            }
            else
            {
                try
                {
                    FilteredClips.ScrollIntoView(FilteredClips.SelectedItem, ScrollIntoViewAlignment.Default);
                }
                catch { }
            }
        }

        private void btn_release(object sender, RoutedEventArgs e)
        {
            if(playerState == VideoPlayerState.Paused)
            {
                btnPause_Click(null, null);
            }
            else
            {
                btnPlay_Click(null, null);
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            rewindTimer.Stop();
            if (videoMediaElement.DefaultPlaybackRate != 1)
            {
                videoMediaElement.DefaultPlaybackRate = 1.0;
                videoMediaElement.PlaybackRate = 1.0;
            }

            playerState = VideoPlayerState.Playing;
            videoMediaElement.Play();

            // Here we need to collapse and expand both full and non full screen buttons
            setPauseVisible();
            setStopVisibile();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            rewindStopwatch.Stop();
            rewindTimer.Stop();
            playerState = VideoPlayerState.Paused;
            videoMediaElement.Pause();
            // Here we need to collapse and expand both full and non full screen buttons
            setPlayVisible();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            rewindStopwatch.Stop();
            rewindTimer.Stop();
            playerState = VideoPlayerState.Paused;
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
            rewindPosition = videoMediaElement.Position;
            isFastRewind = true;
            rewindStopwatch.Reset();
            videoMediaElement.Pause();
            rewindStopwatch.Start();
            rewindTimer.Start();
            setPauseVisible();
            setStopVisibile();
        }

        private void btnSlowReverse_Click(object sender, RoutedEventArgs e)
        {
            rewindPosition = videoMediaElement.Position;
            isFastRewind = false;
            rewindStopwatch.Reset();
            videoMediaElement.Pause();
            rewindStopwatch.Start();
            rewindTimer.Start();
            setPauseVisible();
        }

        private void btnSlowForward_Click(object sender, RoutedEventArgs e)
        {
            videoMediaElement.DefaultPlaybackRate = 0.5;
            videoMediaElement.Play();

            setPauseVisible();
            setStopVisibile();
        }

        private void setPlayVisible()
        {
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;

            snapped_btnPause.Visibility = Visibility.Collapsed;
            snapped_btnPlay.Visibility = Visibility.Visible;
        }

        private void VideoPage_KeyUp(object sender, Windows.UI.Core.KeyEventArgs e)
        {
            keyPressTimer.Stop();
            if (keyPressTimer.ElapsedMilliseconds < keyPressLength)
            {
                if (e.VirtualKey == Windows.System.VirtualKey.Down)
                {
                    if (playerState == VideoPlayerState.Paused)
                    {
                        btnPlay_Click(null, null);
                    }
                    else
                    {
                        btnPause_Click(null, null);
                    }
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Up)
                {
                    rewindKeyPressTimer.Stop();
                    btn_release(null, null);
                    VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
                    vm.ResetClip();
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                {
                    VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
                    vm.GoToNextClip();
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                {
                    rewindKeyPressTimer.Stop();
                    btn_release(null, null);
                    VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
                    vm.GoToPreviousClip();
                    e.Handled = true;
                }
            }
            else
            {
                btn_release(null, null);
            }
            keyPressTimer.Reset();
        }

        private void VideoPage_KeyDown(object sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (!e.KeyStatus.WasKeyDown)
            {
                rewindKeyPressTimer.Stop();
                if (e.VirtualKey == Windows.System.VirtualKey.Down)
                {
                    btnSlowForward_Click(null, null);
                    keyPressTimer.Start();
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Up)
                {
                    rewindKey = e;
                    keyPressTimer.Start();
                    rewindKeyPressTimer.Start();
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                {
                    btnFastForward_Click(null, null);
                    keyPressTimer.Start();
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                {
                    rewindKey = e;
                    keyPressTimer.Start();
                    rewindKeyPressTimer.Start();
                    e.Handled = true;
                }
            }
        }

        void rewindKeyPressTimerTick(object sender, object e)
        {
            if (keyPressTimer.ElapsedMilliseconds > keyPressLength && !rewindKey.KeyStatus.IsKeyReleased)
            {
                if (rewindKey.VirtualKey == Windows.System.VirtualKey.Up)
                {

                    btnSlowReverse_Click(null, null);
                } 
                else if (rewindKey.VirtualKey == Windows.System.VirtualKey.Left || rewindKey.VirtualKey == Windows.System.VirtualKey.PageUp)
                {
                    btnFastReverse_Click(null, null);
                }
                rewindKeyPressTimer.Stop();
            }
            else if (keyPressTimer.ElapsedMilliseconds > keyPressLength)
            {
                rewindKeyPressTimer.Stop();
            }
        }

        void videoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            playerState = VideoPlayerState.Playing;

            videoMediaElement.DefaultPlaybackRate = 1.0;
            videoMediaElement.PlaybackRate = 1.0;
            setPauseVisible();
            setStopVisibile();
        }

        void videoMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            setPlayVisible();
            setPrevVisible();

            VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
            vm.NextClip(NextAngleEvent.mediaEnded);
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
            if (TopAppBar.IsOpen == false || BottomAppBar.IsOpen == false)
            {
                TopAppBar.IsOpen = true;
                BottomAppBar.IsOpen = true;

                if (timelineContainer == null)
                {
                    timelineContainer = (Grid)videoMediaElement.ControlPanel.GetDescendantsOfType<Grid>().ElementAt(2);
                }

                Storyboard sb = new Storyboard();

                RepositionThemeAnimation animation = new RepositionThemeAnimation();

                Storyboard.SetTarget(animation, timelineContainer as DependencyObject);
                animation.FromVerticalOffset = 204;

                sb.Children.Add(animation);

                timelineContainer.Margin = new Thickness(0, 0, 0, 204);

                sb.Begin();
            }
        }

        private ObjectAnimationUsingKeyFrames initilizeSlideUpKeyFrames()
        {
            ObjectAnimationUsingKeyFrames slideUpAnimation = new ObjectAnimationUsingKeyFrames();
            for (int i = 1; i <= 20; i++)
            {
                DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame();
                frame.KeyTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(4.55 * i));
                frame.Value = new Thickness(0, 0, 0, Math.Pow(5, 0.16460148371 * i));

                slideUpAnimation.KeyFrames.Add(frame);
            }

            return slideUpAnimation;
        }

        private void setPauseVisible()
        {
            btnPlay.Visibility = Visibility.Collapsed;
            btnPause.Visibility = Visibility.Visible;

            snapped_btnPlay.Visibility = Visibility.Collapsed;
            snapped_btnPause.Visibility = Visibility.Visible;
        }

        private void setStopVisibile()
        {
            btnReverse.Visibility = Visibility.Collapsed;
            btnStop.Visibility = Visibility.Visible;

            snapped_btnReverse.Visibility = Visibility.Collapsed;
            snapped_btnStop.Visibility = Visibility.Visible;
        }

        private void setPrevVisible()
        {
            btnStop.Visibility = Visibility.Collapsed;
            btnReverse.Visibility = Visibility.Visible;

            snapped_btnStop.Visibility = Visibility.Collapsed;
            snapped_btnReverse.Visibility = Visibility.Visible;
        }

        private void ListViewItemPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            rightClicked = true;
            e.Handled = true;
        }
        private void ListViewItemClicked(object sender, ItemClickEventArgs e)
        {
            itemClicked = true;
        }

        void rewindTimerTick(object sender, object e)
        {
            rewindStopwatch.Stop();
            if (isFastRewind)
            {
                rewindPosition = rewindPosition.Subtract(new TimeSpan(0, 0, 0, 0, Convert.ToInt32(rewindStopwatch.ElapsedMilliseconds * 2)));
            }
            else
            {
                rewindPosition = rewindPosition.Subtract(new TimeSpan(0, 0, 0, 0, Convert.ToInt32(rewindStopwatch.ElapsedMilliseconds / 2)));
            }
            videoMediaElement.Position = rewindPosition;

            rewindStopwatch.Reset();
            rewindStopwatch.Start();
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SortFilterPopup.IsOpen = false;
            TopAppBar.IsOpen = false;
            BottomAppBar.IsOpen = false;
            var currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.Snapped)
            {
                mainGrid.Visibility = Visibility.Collapsed;
                snapped_mainGrid.Visibility = Visibility.Visible;
            }
            else
            {
                snapped_mainGrid.Visibility = Visibility.Collapsed;
                mainGrid.Visibility = Visibility.Visible;
            }
        }

        private void openOptionsMenu(object sender, RoutedEventArgs e)
        {
            PlaybackOptionsPopup.IsOpen = true;
        }

        private void PlaybackOptionsPopup_Opened_1(object sender, object e)
        {
            SortFilterPopup.IsOpen = false;
        }

        private void SortFilterPopup_Opened_1(object sender, object e)
        {
            PlaybackOptionsPopup.IsOpen = false;
        }

        private void AppBarClosed(object sender, object e)
        {
            Storyboard sb = new Storyboard();

            RepositionThemeAnimation animation = new RepositionThemeAnimation();

            Storyboard.SetTarget(animation, timelineContainer as DependencyObject);
            animation.FromVerticalOffset = -204;

            sb.Children.Add(animation);

            timelineContainer.Margin = new Thickness(0);

            sb.Begin();
        }
    }

    public enum VideoPlayerState
    {
        Playing = 0,
        Paused = 1
    }
}