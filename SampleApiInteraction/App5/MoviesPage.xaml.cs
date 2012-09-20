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
using App5.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace App5
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MoviesPage : App5.Common.LayoutAwarePage
    {
        PassToSplit gValue;
        public MoviesPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
            gValue = (PassToSplit)navigationParameter;
            File file = new File { fileName = gValue.fileLocation };
            Group gp = new Group { Title = gValue.Title };
            this.DefaultViewModel["Group"] = gp;
            this.DefaultViewModel["Item"] = file;
        }


        private bool isFullscreen = false;
        private Size previousVideoContainerSize = new Size();


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
                //TransportControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                this.previousVideoContainerSize.Width = mediaContainer.ActualWidth;
                this.previousVideoContainerSize.Height = mediaContainer.ActualHeight;

                mediaContainer.Width = Window.Current.Bounds.Width;
                mediaContainer.Height = Window.Current.Bounds.Height;
                media.Width = Window.Current.Bounds.Width;
                media.Height = Window.Current.Bounds.Height;
            }

            else
            {
                //TransportControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;

                mediaContainer.Width = this.previousVideoContainerSize.Width;
                mediaContainer.Height = this.previousVideoContainerSize.Height;
                media.Width = this.previousVideoContainerSize.Width;
                media.Height = this.previousVideoContainerSize.Height;
            }
        }
    }
}
