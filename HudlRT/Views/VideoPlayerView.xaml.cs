﻿using System.Threading.Tasks;
using HudlRT.Common;
using HudlRT.Models;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Media.PlayTo;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

namespace HudlRT.Views
{
    public sealed partial class VideoPlayerView : LayoutAwarePage
    {
        private const int POPUP_WIDTH = 346;
        private const int VIDEO_CONTROLS_WIDTH = 105;
        private const int COLUMN_WIDTH = 130;
        private const int GRID_HEADER_FONT_SIZE = 22;
 
        // PlayTo variables
        private PlayToManager playToManager = null;
        bool streaming = false;

        private Grid timelineContainer;

        private bool rightClicked { get; set; }
        private bool itemClicked { get; set; }
        private bool controlsFaded;
        private string _rootNamespace;
        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }
        private int count = 0;
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
        private bool isControlDown { get; set; }
        private VideoPlayerViewModel videoPlayerViewModel { get; set; }
        
       

        public VideoPlayerView()
        {
            this.InitializeComponent();

            //gridHeaderScroll.ViewChanged += gridHeaderScroll_ViewChanged;
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
            videoPlayerViewModel = (VideoPlayerViewModel)this.DataContext;
            videoPlayerViewModel.GridHeadersTextSorted = new List<string>();
            videoPlayerViewModel.GridHeadersTextUnsorted = new List<string>();
            initializeGrid(videoPlayerViewModel);
            initializeClipDataBar(videoPlayerViewModel);
            controlsFaded = false;

            videoPlayerViewModel.listView = FilteredClips;
            videoPlayerViewModel.SortFilterPopupControl = SortFilterPopup;
            videoPlayerViewModel.ColumnHeaderTextBlocks = gridHeaders.Children.Select(border => (TextBlock)((Border)border).Child).ToList<TextBlock>();
            videoPlayerViewModel.setVideoMediaElement(videoMediaElement);
            videoPlayerViewModel.TopAppBar = TopAppBar;
            videoPlayerViewModel.BottomAppBar = BottomAppBar;

            // Register for a PlayTo event 
            playToManager = PlayToManager.GetForCurrentView();
            playToManager.SourceRequested += playToManager_SourceRequested;
            

        }

        void Connection_Transferred(PlayToConnection sender, PlayToConnectionTransferredEventArgs args)
        {
            throw new NotImplementedException();
        }

        private Windows.UI.Core.CoreDispatcher dispatcher = Window.Current.CoreWindow.Dispatcher;

        private async void playToManager_SourceRequested(PlayToManager sender, PlayToSourceRequestedEventArgs args)
        {
            streaming = true;
            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    Windows.Media.PlayTo.PlayToSourceRequest sr = args.SourceRequest;
                    Windows.Media.PlayTo.PlayToSource controller = null;
                    Windows.Media.PlayTo.PlayToSourceDeferral deferral =
                        args.SourceRequest.GetDeferral();

                    try
                    {
                        
                            controller = videoMediaElement.PlayToSource;
                            videoMediaElement.PlayToSource.Connection.Transferred += Connection_Transferred;
                        
                    }
                    catch (Exception ex)
                    {
                       new MessageDialog("Exception encountered: " + ex.Message).ShowAsync();
                    }

                    sr.SetSource(controller);
                    deferral.Complete();
                }
                catch (Exception ex)
                {
                    new MessageDialog("Exception encountered: " + ex.Message).ShowAsync();
                }
            });


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
                    b.BorderThickness = new Thickness(0, 0, 2, 0);
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

        private void initializeClipDataBar(VideoPlayerViewModel vm)
        {
            int i = 0;
            foreach (var header in vm.GridHeaders)
            {
                TextBlock textBlock_title = (TextBlock)XamlReader.Load(@"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Margin=""20,0,5,0"" FontWeight=""Bold"" Foreground=""{StaticResource HudlMediumGray}"" FontSize=""22"" Text=""{Binding GridHeaders[X]}""/>".Replace("X", i.ToString()));
                TextBlock textBlock_data = (TextBlock)XamlReader.Load(@"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" DataContext=""{Binding SelectedClip}"" Margin=""5,0,10,0"" Foreground=""{StaticResource HudlMediumGray}"" FontSize=""22"" Text=""{Binding Path=breakDownData[X]}""/>".Replace("X", i.ToString()));
                ClipDataText.Children.Add(textBlock_title);
                ClipDataText.Children.Add(textBlock_data);

                i++;
            }
        }

        private void filteredClips_Loaded(object sender, RoutedEventArgs e)
        {
            filteredListScrollViewer = FilteredClips.GetFirstDescendantOfType<ScrollViewer>();
            //filteredListScrollViewer.ViewChanged += filteredListScrollViewer_ViewChanged;
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

            videoPlayerViewModel.PrepareSortFilterPopup(id);

            if (!SortFilterPopup.IsOpen)
            {
                RootPopupBorder.Width = POPUP_WIDTH;
                SortFilterPopup.HorizontalOffset = Window.Current.Bounds.Width - POPUP_WIDTH - VIDEO_CONTROLS_WIDTH;

                var currentViewState = ApplicationView.Value;
                if (currentViewState != ApplicationViewState.Filled)
                {
                    SortFilterPopup.IsOpen = true;
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
            playToManager.SourceRequested -= playToManager_SourceRequested;
        }

        private void VideosList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightClicked)
            {
                ListView listView = (ListView)sender;
                videoPlayerViewModel.SetClip((Clip)listView.SelectedItem);

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

        #region XAML Video Player Functions
        private void btn_release(object sender, RoutedEventArgs e)
        {
            videoPlayer_Resume();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_Play();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_Stop();
        }

        private void btnFastForward_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_FastForward();
        }

        private void btnFastReverse_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_FastReverse();
        }

        private void btnSlowReverse_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_SlowReverse();
        }

        private void btnSlowForward_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer_SlowForward();
        }
        #endregion
        
        private void setPlayVisible()
        {
            btnPause.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;

            snapped_btnPause.Visibility = Visibility.Collapsed;
            snapped_btnPlay.Visibility = Visibility.Visible;
        }

        private void videoPlayer_Resume()
        {
            if (playerState == VideoPlayerState.Paused)
            {
                videoPlayer_Pause();
            }
            else
            {
                videoPlayer_Play();
            }
        }

        private void videoPlayer_Play()
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

        private void videoPlayer_Pause()
        {
            rewindStopwatch.Stop();
            rewindTimer.Stop();
            playerState = VideoPlayerState.Paused;
            videoMediaElement.Pause();
            // Here we need to collapse and expand both full and non full screen buttons
            setPlayVisible();
        }

        private void videoPlayer_Stop()
        {
            rewindStopwatch.Stop();
            rewindTimer.Stop();
            playerState = VideoPlayerState.Paused;
            videoMediaElement.Stop();
            setPlayVisible();
            setPrevVisible();
            
        }

        private void videoPlayer_FastForward()
        {
            videoMediaElement.DefaultPlaybackRate = 2.0;
            videoMediaElement.Play();

            setPauseVisible();
            setStopVisibile();
        }

        private void videoPlayer_SlowForward()
        {
            videoMediaElement.DefaultPlaybackRate = 0.5;
            videoMediaElement.Play();

            setPauseVisible();
            setStopVisibile();
        }

        private void videoPlayer_FastReverse()
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

        private void videoPlayer_SlowReverse()
        {
            rewindPosition = videoMediaElement.Position;
            isFastRewind = false;
            rewindStopwatch.Reset();
            videoMediaElement.Pause();
            rewindStopwatch.Start();
            rewindTimer.Start();
            setPauseVisible();
        }

        private void VideoPage_KeyUp(object sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Control)
            {
                isControlDown = false;
                e.Handled = true;
            }
            else if (e.VirtualKey == VirtualKeyHelper.NextTrack || e.VirtualKey == VirtualKeyHelper.PreviousTrack)
            {
                videoPlayer_Resume();
                e.Handled = true;
            }
            else if ((e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.Right) && isControlDown)
            {
                videoPlayer_Resume();
                e.Handled = true;
            } 
            else if(e.VirtualKey == Windows.System.VirtualKey.Up && isControlDown)
            {
                e.Handled = true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Down && isControlDown) 
            {
                e.Handled = true;
            }
            else
            {
                keyPressTimer.Stop();
                if (keyPressTimer.ElapsedMilliseconds < keyPressLength)
                {
                    if (e.VirtualKey == Windows.System.VirtualKey.Down)
                    {
                        if (playerState == VideoPlayerState.Paused)
                        {
                            videoPlayer_Play();
                        }
                        else
                        {
                            videoPlayer_Pause();
                        }
                        e.Handled = true;
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Up)
                    {
                        rewindKeyPressTimer.Stop();
                        videoPlayer_Resume();
                        videoPlayerViewModel.ResetClip();
                        e.Handled = true;
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                    {
                        videoPlayerViewModel.GoToNextClip();
                        e.Handled = true;
                    }
                    else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                    {
                        rewindKeyPressTimer.Stop();
                        videoPlayer_Resume();
                        videoPlayerViewModel.GoToPreviousClip();
                        e.Handled = true;
                    }
                }
                else
                {
                    videoPlayer_Resume();
                }
                keyPressTimer.Reset();
            }
        }

        private void VideoPage_KeyDown(object sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (!e.KeyStatus.WasKeyDown)
            {
                rewindKeyPressTimer.Stop();
                if (e.VirtualKey == Windows.System.VirtualKey.Down)
                {
                    if (isControlDown) //Tag
                    {
                        
                    }
                    else 
                    {
                        videoPlayer_SlowForward();
                        keyPressTimer.Start();
                    }
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Up)
                {
                    if (isControlDown) //Full Screen
                    {
                        if (TopAppBar.IsOpen || BottomAppBar.IsOpen)
                        {
                            TopAppBar.IsOpen = false;
                            BottomAppBar.IsOpen = false;
                        }
                        else
                        {
                            TopAppBar.IsOpen = true;
                            BottomAppBar.IsOpen = true;
                        }
                    }
                    else
                    {
                        rewindKey = e;
                        keyPressTimer.Start();
                        rewindKeyPressTimer.Start();
                    }
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Right || e.VirtualKey == Windows.System.VirtualKey.PageDown)
                {
                    if (isControlDown) //Remote Fast Forward
                    {
                        videoPlayer_FastForward();
                    }
                    else
                    {
                        videoPlayer_FastForward();
                        keyPressTimer.Start();
                    }
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Left || e.VirtualKey == Windows.System.VirtualKey.PageUp)
                {
                    if (isControlDown) //Remote Fast Reverse
                    {
                        videoPlayer_FastReverse();
                    }
                    else
                    {
                        rewindKey = e;
                        keyPressTimer.Start();
                        rewindKeyPressTimer.Start();
                    }
                    e.Handled = true;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Control)
                {
                    isControlDown = true;
                }
                else if (e.VirtualKey == VirtualKeyHelper.PreviousTrack) //Previous Media Key
                {
                    if (isControlDown) //Remote Slow Reverse
                    {
                        videoPlayer_SlowReverse();
                        e.Handled = true;
                    }
                    else //Remote Previous
                    {
                        videoPlayerViewModel.GoToPreviousClip();
                        e.Handled = true;
                    }
                }
                else if (e.VirtualKey == VirtualKeyHelper.NextTrack) //Next Media Key
                {
                    if (isControlDown) //Remote Slow Forward
                    {
                        videoPlayer_SlowForward();
                        e.Handled = true;
                    }
                    else //Remote Next
                    {
                        videoPlayerViewModel.GoToNextClip();
                        e.Handled = true;
                    }
                }
                else if (e.VirtualKey == VirtualKeyHelper.PlayPause) //Play/Pause Media Key
                {
                    if (playerState == VideoPlayerState.Paused)
                    {
                        videoPlayer_Play();
                    }
                    else
                    {
                        videoPlayer_Pause();
                    }
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

                    videoPlayer_SlowReverse();
                } 
                else if (rewindKey.VirtualKey == Windows.System.VirtualKey.Left || rewindKey.VirtualKey == Windows.System.VirtualKey.PageUp)
                {
                    videoPlayer_FastReverse();
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

            ClipDataScrollViewer.ScrollToHorizontalOffset(0);
        }

        void videoMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            setPlayVisible();
            setPrevVisible();

            videoPlayerViewModel.NextClip(NextAngleEvent.mediaEnded);
        }

        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string hr = GetHresultFromErrorMessage(e);
        }

        private void videoMediaElement_MediaStarted(object sender, RoutedEventArgs e)
        {
            var x = videoMediaElement.ControlPanel.GetDescendants();
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
            ApplicationViewState currentViewState = ApplicationView.Value;

            if (currentViewState != ApplicationViewState.Snapped  && !controlsFaded)
            {
                if (TopAppBar.IsOpen == false || BottomAppBar.IsOpen == false)
                {
                    TopAppBar.IsOpen = true;
                    BottomAppBar.IsOpen = true;
                }
                else if (TopAppBar.IsOpen == true || BottomAppBar.IsOpen == true)
                {
                    TopAppBar.IsOpen = false;
                    BottomAppBar.IsOpen = false;
                }
            }
            else if (controlsFaded)
            {
                MoreBtn_Click(null, null);
            }
        }

        private ObjectAnimationUsingKeyFrames initilizeSlideUpKeyFrames()
        {
            ObjectAnimationUsingKeyFrames slideUpAnimation = new ObjectAnimationUsingKeyFrames();
            for (int i = 1; i <= 20; i++)
            {
                DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame();
                frame.KeyTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(4.55 * i));
                frame.Value = new Thickness(0, 0, 0, Math.Pow(5, .16491060811 * i));

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
            PlaybackOptionsPopup.IsOpen = false;
            TopAppBar.IsOpen = false;
            BottomAppBar.IsOpen = false;
            ApplicationViewState currentViewState = ApplicationView.Value;

            videoMediaElement.Width = Window.Current.Bounds.Width;
            videoMediaElement.Height = Window.Current.Bounds.Height;
            videoContainer.Width = Window.Current.Bounds.Width;
            videoContainer.Height = Window.Current.Bounds.Height;

            if (currentViewState == ApplicationViewState.Snapped)
            {
                BottomAppBar.Visibility = Visibility.Collapsed;
                TopAppBar.Visibility = Visibility.Collapsed;
                VideoControls.Visibility = Visibility.Collapsed;
                ClipDataGrid.Visibility = Visibility.Collapsed;
                LessBtn.Visibility = Visibility.Collapsed;
                MoreBtn.Visibility = Visibility.Collapsed;
                snapped_mainGrid.Visibility = Visibility.Visible;

                if (timelineContainer != null)
                {
                    Grid grid = (Grid)timelineContainer.Children[0];
                    grid.Margin = new Thickness(5, 6, 5, 6);
                }
                AppBarClosed_Snapped();
            }
            else
            {
                BottomAppBar.Visibility = Visibility.Visible;
                TopAppBar.Visibility = Visibility.Visible;
                snapped_mainGrid.Visibility = Visibility.Collapsed;
                ClipDataGrid.Visibility = Visibility.Visible;
                VideoControls.Visibility = Visibility.Visible;
                if (controlsFaded)
                {
                    MoreBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    LessBtn.Visibility = Visibility.Visible;
                }

                if (timelineContainer != null)
                {
                    Grid grid = (Grid)timelineContainer.Children[0];
                    grid.Margin = new Thickness(30, 6, 50, 6);
                }
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
            try
            {
                FiltersList.ScrollIntoView(FiltersList.Items[0]);
            }
            catch { }
        }

        private void AppBarClosed(object sender, object e)
        {
            ClipDataGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;

            ApplicationViewState currentViewState = ApplicationView.Value;

            if (timelineContainer == null)
            {
                try
                {
                    timelineContainer = (Grid)videoMediaElement.ControlPanel.GetDescendantsOfType<Grid>().ElementAt(2);
                }
                catch { }
            }

            try
            {
                Storyboard sb = new Storyboard();

                RepositionThemeAnimation repositionTimelineAnimation = new RepositionThemeAnimation();
                RepositionThemeAnimation repositionBtnAnimation = new RepositionThemeAnimation();
                FadeInThemeAnimation fadeInAnimation = new FadeInThemeAnimation();
                FadeInThemeAnimation fadeInBtn = new FadeInThemeAnimation();

                Storyboard.SetTarget(fadeInAnimation, ClipDataGrid as DependencyObject);
                Storyboard.SetTarget(fadeInBtn, LessBtn as DependencyObject);

                Storyboard.SetTarget(repositionTimelineAnimation, timelineContainer as DependencyObject);
                repositionTimelineAnimation.FromVerticalOffset = -204;

                Storyboard.SetTarget(repositionBtnAnimation, LessBtn as DependencyObject);
                repositionBtnAnimation.FromVerticalOffset = -204;

                sb.Children.Add(repositionTimelineAnimation);
                sb.Children.Add(repositionBtnAnimation);
                sb.Children.Add(fadeInAnimation);
                sb.Children.Add(fadeInBtn);

                timelineContainer.Margin = new Thickness(0);
                LessBtn.Margin = new Thickness(0);

                sb.Begin();
            }
            catch { }
        }

        private void AppBarClosed_Snapped()
        {
            if (timelineContainer == null)
            {
                try
                {
                    timelineContainer = (Grid)videoMediaElement.ControlPanel.GetDescendantsOfType<Grid>().ElementAt(2);
                }
                catch { }
            }

            try
            {
                timelineContainer.Margin = new Thickness(0);
            }
            catch { }
        }

        private void AppBarOpened(object sender, object e)
        {
            ApplicationViewState currentViewState = ApplicationView.Value;

            if (currentViewState != ApplicationViewState.Snapped)
            {
                if (timelineContainer == null)
                {
                    try
                    {
                        timelineContainer = (Grid)videoMediaElement.ControlPanel.GetDescendantsOfType<Grid>().ElementAt(2);
                    }
                    catch { }
                }

                try
                {
                    ClipDataGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    Storyboard sb = new Storyboard();

                    RepositionThemeAnimation repositionTimelineAnimation = new RepositionThemeAnimation();
                    RepositionThemeAnimation repositionBtnAnimation = new RepositionThemeAnimation();
                    FadeOutThemeAnimation fadeOutAnimation = new FadeOutThemeAnimation();
                    FadeOutThemeAnimation fadeOutBtn = new FadeOutThemeAnimation();

                    Storyboard.SetTarget(fadeOutAnimation, ClipDataGrid as DependencyObject);
                    Storyboard.SetTarget(fadeOutBtn, LessBtn as DependencyObject);

                    Storyboard.SetTarget(repositionTimelineAnimation, timelineContainer as DependencyObject);
                    repositionTimelineAnimation.FromVerticalOffset = 204;

                    Storyboard.SetTarget(repositionBtnAnimation, LessBtn as DependencyObject);
                    repositionBtnAnimation.FromVerticalOffset = 204;

                    sb.Children.Add(repositionTimelineAnimation);
                    sb.Children.Add(repositionBtnAnimation);
                    sb.Children.Add(fadeOutAnimation);
                    sb.Children.Add(fadeOutBtn);

                    timelineContainer.Margin = new Thickness(0, 0, 0, 204);

                    sb.Begin();
                }
                catch { }
            }
        }

        private void CloseOptionsPopup(object sender, RoutedEventArgs e)
        {
            PlaybackOptionsPopup.IsOpen = false;
            OptionsMenu.Focus(FocusState.Pointer);
        }

        private void SortFilterPopup_Closed_1(object sender, object e)
        {
            VideoPlayerViewModel vm = (VideoPlayerViewModel)this.DataContext;
            if (vm.FilteredClips.Any())
            {
                vm.SetClip(vm.FilteredClips[0]);
            }
        }

        private void LessBtn_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = new Storyboard();

            FadeOutThemeAnimation fadeOutTimeline = new FadeOutThemeAnimation();
            FadeOutThemeAnimation fadeOutClipData = new FadeOutThemeAnimation();
            FadeOutThemeAnimation fadeOutVideoControls = new FadeOutThemeAnimation();

            Storyboard.SetTarget(fadeOutTimeline, timelineContainer as DependencyObject);
            Storyboard.SetTarget(fadeOutClipData, ClipDataGrid as DependencyObject);
            Storyboard.SetTarget(fadeOutVideoControls, VideoControls as DependencyObject);

            sb.Children.Add(fadeOutTimeline);
            sb.Children.Add(fadeOutClipData);
            sb.Children.Add(fadeOutVideoControls);

            sb.Begin();

            LessBtn.Visibility = Visibility.Collapsed;
            MoreBtn.Visibility = Visibility.Visible;

            controlsFaded = true;

        }

        private void MoreBtn_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = new Storyboard();

            FadeInThemeAnimation fadeInTimeline = new FadeInThemeAnimation();
            FadeInThemeAnimation fadeInClipData = new FadeInThemeAnimation();
            FadeInThemeAnimation fadeInVideoControls = new FadeInThemeAnimation();

            Storyboard.SetTarget(fadeInTimeline, timelineContainer as DependencyObject);
            Storyboard.SetTarget(fadeInClipData, ClipDataGrid as DependencyObject);
            Storyboard.SetTarget(fadeInVideoControls, VideoControls as DependencyObject);

            sb.Children.Add(fadeInTimeline);
            sb.Children.Add(fadeInClipData);
            sb.Children.Add(fadeInVideoControls);

            sb.Begin();

            MoreBtn.Visibility = Visibility.Collapsed;
            LessBtn.Visibility = Visibility.Visible;

            controlsFaded = false;
        }
    }

    public enum VideoPlayerState
    {
        Playing = 0,
        Paused = 1
    }
}