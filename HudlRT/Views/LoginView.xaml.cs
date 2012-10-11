using HudlRT.Common;
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

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginView : LayoutAwarePage
    {
        private int keyboardOffset = 0;

        public LoginView()
        {
            this.InitializeComponent();

            // Register a handler to slide the login form up if a virtual keyboard in displayed on the screen.
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += (s, args) =>
                {
                    keyboardOffset = (int)args.OccludedRect.Height;
                    loginStackPanel.Margin = new Thickness(loginStackPanel.Margin.Left, -1 * keyboardOffset, loginStackPanel.Margin.Right, loginStackPanel.Margin.Bottom);
                };

            // Register a handler to slide the login form down if a virtual keyboard is removed from the screen.
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += (s, args) =>
                {
                    loginStackPanel.Margin = new Thickness(loginStackPanel.Margin.Left, 0, loginStackPanel.Margin.Right, loginStackPanel.Margin.Bottom);
                };
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
