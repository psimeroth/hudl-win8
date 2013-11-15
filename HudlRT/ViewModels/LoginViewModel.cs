using Caliburn.Micro;
using HudlRT.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Input;

namespace HudlRT.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                NotifyOfPropertyChange(() => UserName);
            }
        }

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

        // Bound to the visibility of the progress ring
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

        private bool rememberMe;
        public bool RememberMe
        {
            get { return rememberMe; }
            set
            {
                rememberMe = value;
                NotifyOfPropertyChange(() => RememberMe);
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
            DownloadAccessor.Instance.DeleteTempData();
            base.OnInitialize();

            ButtonText = "Login";
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
            RememberMe = false;

            //If Username exists in roaming settings, enter it for user
            String username = AppDataAccessor.GetUsername();
            if (username != null)
            {
                UserName = username;

                // Check to see if a password has been saved
                PasswordCredential cred = AppDataAccessor.GetPassword();
                if (cred != null)
                {
                    Password = cred.Password;
                    LoginAttempt();
                }
            }
        }

        public async void DemoLoginAttempt()
        {
            // Attempt to get the debug urls from a config file
            InitResponse initResponse = await ServiceAccessor.Init();

            // Show the user a call is being made in the background
            FormVisibility = "Collapsed";
            ProgressRingVisibility = "Visible";

            LoginResponse response = await ServiceAccessor.DemoLogin();
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                navigationService.NavigateToViewModel<HubViewModel>();
            }
            else if (response.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
                LoginFeedback = "Connection with server failed, please try again";
            }
            else if (response.status == SERVICE_RESPONSE.CREDENTIALS)
            {
                LoginFeedback = "Invalid Username or Password";
            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                await HandleNoConnection();
            }

            // Dismiss the loading indicator
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
            AppDataAccessor.SetDemoMode(true);
        }




        public async void LoginAttempt()
        {
            // Attempt to get the debug urls from a config file
            InitResponse initResponse = await ServiceAccessor.Init();

            if (initResponse.status == SERVICE_RESPONSE.SUCCESS)
            {
                if (Password == null || Password == "")
                {
                    Password = initResponse.Password;
                }
                if (UserName == null || UserName == "")
                {
                    UserName = initResponse.Username;
                }
            }

            // Get the username and password from the view
            string loginArgs = JsonConvert.SerializeObject(new LoginSender { Username = UserName, Password = Password });

            // Show the user a call is being made in the background
            ButtonText = "loading";
            FormVisibility = "Collapsed";
            ProgressRingVisibility = "Visible";

            LoginResponse response = await ServiceAccessor.Login(loginArgs);
            if (response.status == SERVICE_RESPONSE.SUCCESS)
            {
                if (AppDataAccessor.GetUsername() != userName)
                {
                    AppDataAccessor.SetUsername(UserName);
                }
                if (RememberMe)
                {
                    AppDataAccessor.SetPassword(Password);
                    AppDataAccessor.SetLoginDate(DateTime.Now.ToString());
                }
                navigationService.NavigateToViewModel<HubViewModel>();
            }
            else if (response.status == SERVICE_RESPONSE.NULL_RESPONSE)
            {
                LoginFeedback = "Connection with server failed, please try again";
            }
            else if (response.status == SERVICE_RESPONSE.CREDENTIALS)
            {
                LoginFeedback = "Invalid Username or Password";
            }
            else if (response.status == SERVICE_RESPONSE.NO_CONNECTION)
            {
                await HandleNoConnection();
            }

            // Dismiss the loading indicator
            ButtonText = "Login";
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        private async Task HandleNoConnection()
        {
            DateTime LastLogin = new DateTime();
            string loginDate = AppDataAccessor.GetLoginDate();
            if (loginDate != null)
            {
                await Task.Run(() => LastLogin = DateTime.Parse(AppDataAccessor.GetLoginDate()));
                    //need an async task in order to the page to navigate
                TimeSpan ts = DateTime.Now - LastLogin;
                if (ts.Days <= 14)
                {
                    navigationService.NavigateToViewModel<HubViewModel>();
                }
                else
                {
                    APIExceptionDialog.ShowNoInternetConnectionLoginDialog(null, null);
                }
            }
            else
            {
                APIExceptionDialog.ShowNoInternetConnectionLoginDialog(null, null);
            }
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
