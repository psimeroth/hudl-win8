using HudlRT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Animation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : LayoutAwarePage
    {
        private double y = 0;

        public LoginView()
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
            if (e.Parameter != null)
            {
                SplashScreen splash = (SplashScreen)e.Parameter;
                splash.Dismissed += new TypedEventHandler<SplashScreen, object>(DismissedEventHandler);

                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-x"] = splash.ImageLocation.Left;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-y"] = splash.ImageLocation.Top;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-height"] = splash.ImageLocation.Height;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-width"] = splash.ImageLocation.Width;
                loginStackPanel.Margin = new Thickness(0, splash.ImageLocation.Top, 0, 0);
            }
            else
            {
                loginStackPanel.Margin = new Thickness(0, 0, 0, 0);
                loginFormStackPanel.Opacity = 1;
            }

            // Set the login image here
            var height = (double)ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-height"];
            var width = (double)ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-width"];
            var x = (double)ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-x"];
            y = (double)ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-y"];

            loginImage.Height = height;
            loginImage.Width = Width;

            //If Username exists in roaming settings, enter it for user
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["UserName"] != null)
            {
                UserName.Text = roamingSettings.Values["UserName"].ToString();
                UserName.SelectionStart = UserName.Text.ToCharArray().Length;
                UserName.SelectionLength = 0;
            }
        }

        void DismissedEventHandler(SplashScreen sender, object e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, PageLoadAnimations);
        }

        private void PageLoadAnimations()
        {
            ((RepositionThemeAnimation)PositionLoginForm.Children.ElementAt(0)).FromVerticalOffset = y + 10;
            PositionLoginForm.Begin();
            loginStackPanel.Margin = new Thickness(0, -10, 0, 0);
            FadeInForm.BeginTime = new TimeSpan(0, 0, 1);
            FadeInForm.Begin();
            FadeInBackground.BeginTime = new TimeSpan(0, 0, 1);
            FadeInBackground.Begin();
        }

        /// <summary>
        /// Click handler for the New to Hudl button. Simply redirects to the Hudl signup webpage.
        /// </summary>
        /// <param name="sender">The New to Hudl button.</param>
        /// <param name="e">Associated click event.</param>
        private async void RedirectToSignup(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.hudl.com/signup"));
        }

    }
}
