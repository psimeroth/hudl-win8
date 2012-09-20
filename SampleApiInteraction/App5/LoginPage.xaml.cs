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
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App5
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
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

        private void loginSubmit_Click(object sender, RoutedEventArgs e)
        {
            string loginArgs;
            if (username.Text.Length != 0)
            {
                loginArgs = JsonConvert.SerializeObject(new LoginSender { Username = username.Text, Password = password.Password });
            }
            else
            {
                loginArgs = JsonConvert.SerializeObject(new LoginSender { Username = AppData.LOGIN_USERNAME, Password = AppData.LOGIN_PASSWORD });
            }


            Task<string> test = ServiceAccessor.MakeApiCall(AppData.URL_BASE_SECURE + AppData.URL_SERVICE_LOGIN, "POST", loginArgs, "");
            var asyncAction = test.AsAsyncOperation<string>().Completed += AsyncActionHandler;


        }

        private void AsyncActionHandler(IAsyncOperation<string> asyncInfo, AsyncStatus asyncStatus)
        {
            var loginRetVal = asyncInfo.GetResults();

            try
            {
                var obj = JsonConvert.DeserializeObject<LoginProfileDTO>(loginRetVal);


                PassToSplit value = new PassToSplit();
                value.Token = obj.Token;
                this.Frame.Navigate(typeof(ItemsPage), value);
                
                LoginErrorMessage.Text = "";
            }
            catch (Exception ex)
            {
                LoginErrorMessage.Text = "Login Failed. Check your username and password.";
            }
        }

        private void KeyUpMedia(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                loginSubmit_Click(sender, e);
            }
        }
    }
}
