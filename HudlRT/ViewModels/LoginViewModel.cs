using Caliburn.Micro;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.Storage;
using HudlRT.Models;
using Windows.UI.Xaml.Input;
using Windows.UI.ApplicationSettings;

namespace HudlRT.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        private string loginFeedback;
        public string LoginFeedback
        {
            get { return loginFeedback; }
            set 
            { 
                loginFeedback = value;
                NotifyOfPropertyChange(() => LoginFeedback);
            }
        }

        private string buttonText;
        public string ButtonText
        {
            get { return buttonText; }
            set
            {
                buttonText = value;
                NotifyOfPropertyChange(() => ButtonText);
            }
        }

        // Bound to the visibility of the login form stack panel
        private string formVisibility;
        public string FormVisibility
        {
            get { return formVisibility; }
            set
            {
                formVisibility = value;
                NotifyOfPropertyChange(() => FormVisibility);
            }
        }

        // Bound to the visibility of the progress ring. Swaps with 
        private string progressRingVisibility;
        public string ProgressRingVisibility
        {
            get { return progressRingVisibility; }
            set
            {
                progressRingVisibility = value;
                NotifyOfPropertyChange(() => ProgressRingVisibility);
            }
        }

        private readonly INavigationService navigationService;
        public LoginViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
            CharmsData.navigationService = navigationService;
            SettingsPane.GetForCurrentView().CommandsRequested += CharmsData.SettingCharmManager_LoginCommandsRequested;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            //Login = new LoginModel();
            ButtonText = "Login";
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        public async void LoginAttempt()
        {
            // Get the username and password from the view
#if DEBUG
            if ((UserName == null || UserName == "" || UserName == "windows8") && (Password == null || Password == ""))
            {
                UserName = "windows8";
                Password = "rightmeow!";
            }
#endif
            string loginArgs = JsonConvert.SerializeObject(new LoginSender { Username = UserName, Password = Password });

            // Show the user a call is being made in the background
            ButtonText = "loading";
            FormVisibility = "Collapsed";
            ProgressRingVisibility = "Visible";

            // Call the login web service
            var loginResponse = await ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOGIN, loginArgs);

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!string.IsNullOrEmpty(loginResponse))
            {
                var obj = JsonConvert.DeserializeObject<LoginResponseDTO>(loginResponse);
                ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"] = obj.Token;
                ApplicationData.Current.RoamingSettings.Values["hudl-userId"] = obj.UserId;
                LoginFeedback = "";

                //save username to app data
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["UserName"] = UserName;

                //need to save privileges to roamingsettings
                string urlExtension = "privileges/" + ApplicationData.Current.RoamingSettings.Values["hudl-userId"].ToString();
                var privilegesResponse = await ServiceAccessor.MakeApiCallGet(urlExtension);
                if (!string.IsNullOrEmpty(privilegesResponse))
                {
                    //Needs to be improved in the future if we want to 
                    if ( privilegesResponse.Contains("Win8App") )
                    {
                         navigationService.NavigateToViewModel<SectionViewModel>();
                    }
                    else
                    {
                        navigationService.NavigateToViewModel<FeatureDisabledViewModel>();
                    }
                }
                else
                {
                    LoginFeedback = "Connection with server failed, please try again";
                }
            }
            else
            {
                LoginFeedback = "Invalid Username or Password.";
            }

            // Dismiss the loading indicator
            ButtonText = "login";
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }


        /// <summary>
        /// Key handler for the password field of the login view.
        /// </summary>
        /// <param name="e">Key up event which occured.</param>
        public void KeyPress(KeyRoutedEventArgs e)
        {
            // If the key pressed was Enter call the Login method.
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                LoginAttempt();
            }
        }
    }
}
