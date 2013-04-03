using Caliburn.Micro;
using HudlRT.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Input;
using Windows.UI.Notifications;
using NotificationsExtensions.TileContent;
using System.Net.Http;

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

                try
                {
                    SetupTile();
                }
                catch { }
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
                DateTime LastLogin = new DateTime();
                string loginDate = AppDataAccessor.GetLoginDate();
                if (loginDate != null)
                {
                    await Task.Run(() => LastLogin = DateTime.Parse(AppDataAccessor.GetLoginDate()));//need an async task in order to the page to navigate
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

            // Dismiss the loading indicator
            ButtonText = "Login";
            FormVisibility = "Visible";
            ProgressRingVisibility = "Collapsed";
        }

        private async void SetupTile()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();





            string lastTweet = await GetLastTweet();

            // create the wide template
            ITileWideText04 tileContent = TileContentFactory.CreateTileWideText04();
            tileContent.TextBodyWrap.Text = lastTweet;

            // create the square template and attach it to the wide template
            ITileSquareText04 squareContent = TileContentFactory.CreateTileSquareText04();
            squareContent.TextBodyWrap.Text = lastTweet;
            tileContent.SquareContent = squareContent;

            // send the notification
            TileNotification tn = tileContent.CreateNotification();
            tn.Tag = "HudlLTTwitter";
            tn.ExpirationTime = DateTime.Now.AddMonths(1);
            updater.Update(tn);



            // Create notification content based on a visual template.
            ITileWideImageAndText01 tileContent2 = TileContentFactory.CreateTileWideImageAndText01();

            List<string> urls = new List<string>();
            urls.Add("ms-appx:///Assets/dominate1.jpg");
            urls.Add("ms-appx:///Assets/dominate2.jpg");
            urls.Add("ms-appx:///Assets/dominate3.jpg");

            tileContent2.TextCaptionWrap.Text = "Dominate the Competition";
            tileContent2.Image.Src = urls[new Random().Next(0, 3)];
            tileContent2.Image.Alt = "Dominate";

            // create the square template and attach it to the wide template
            ITileSquareImage squareContent2 = TileContentFactory.CreateTileSquareImage();
            squareContent2.Image.Src = urls[new Random().Next(0, 3)];
            squareContent2.Image.Alt = "Dominate";
            tileContent2.SquareContent = squareContent2;

            // Send the notification to the app's application tile.
            TileNotification tn2 = tileContent2.CreateNotification();
            tn2.Tag = "HudlLTDominate";
            tn2.ExpirationTime = DateTime.Now.AddMonths(1);
            updater.Update(tn2);
        }


        class TweetDTO
        {
            public string text { get; set; }
        }
        private async Task<string> GetLastTweet()
        {

            // query twitter
            string url = "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=Hudl&count=2";

            var httpClient = new HttpClient();
            Uri uri = new Uri(url);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);
            string result = await response.Content.ReadAsStringAsync();

            List<TweetDTO> obj = JsonConvert.DeserializeObject<List<TweetDTO>>(result);
            return obj[0].text;
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
