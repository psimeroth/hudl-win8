using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App4
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isFullscreen = false;
        private Size previousVideoContainerSize = new Size();

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void StopMedia(object sender, RoutedEventArgs e)
        {
            media.Stop();
        }

        private void PauseMedia(object sender, RoutedEventArgs e)
        {
            media.Pause();
        }
        
        private void PlayMedia(object sender, RoutedEventArgs e)
        {
            media.Play();
        }

        private void FullMedia(object sender, RoutedEventArgs e)
        {
            ToggleFull();
        }

        private void KeyUpMedia(object sender, KeyRoutedEventArgs e)
        {
            if (this.isFullscreen && 
                (e.Key == Windows.System.VirtualKey.Escape ||
                e.Key == Windows.System.VirtualKey.Q
                ))
            {
                ToggleFull();
            }

            e.Handled = true;
        }

        private void ToggleFull()
        {
            // Toggle the variable to keep track of full screen
            this.isFullscreen = !this.isFullscreen;

            if (this.isFullscreen)
            {
                TransportControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                this.previousVideoContainerSize.Width = mediaContainer.ActualWidth;
                this.previousVideoContainerSize.Height = mediaContainer.ActualHeight;

                mediaContainer.Width = Window.Current.Bounds.Width;
                mediaContainer.Height = Window.Current.Bounds.Height;
                media.Width = Window.Current.Bounds.Width;
                media.Height = Window.Current.Bounds.Height;
            }

            else
            {
                TransportControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;

                mediaContainer.Width = this.previousVideoContainerSize.Width;
                mediaContainer.Height = this.previousVideoContainerSize.Height;
                media.Width = this.previousVideoContainerSize.Width;
                media.Height = this.previousVideoContainerSize.Height;
            }
        }
    }
}
