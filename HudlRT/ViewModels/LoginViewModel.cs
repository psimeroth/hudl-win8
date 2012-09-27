using Caliburn.Micro;
using System;
using Windows.UI.Xaml.Controls;
using HudlRT.Common;
using Newtonsoft.Json;
using Windows.Storage;
using HudlRT.Models;

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

        private readonly INavigationService navigationService;
        public LoginViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

        }

        public async void Login()
        {
            // Get the username and password from the view
            if (UserName == null && Password == null)
            {
                UserName = "jacobataylor09@gmail.com";
                Password = "abcd";
            }
            string loginArgs = JsonConvert.SerializeObject(new LoginSender { Username = UserName, Password = Password });
            var login = await ServiceAccessor.MakeApiCallPost(ServiceAccessor.URL_SERVICE_LOGIN, loginArgs);

            // Once the async call completes check the response, if good show the hub view, if not show an error message.
            if (!login.Equals(""))
            {
                var obj = JsonConvert.DeserializeObject<LoginResponseDTO>(login);
                ApplicationData.Current.RoamingSettings.Values["hudl-authtoken"] = obj.Token;
                LoginFeedback = "";
                navigationService.NavigateToViewModel(typeof(HubViewModel));
            }
            else
            {
                LoginFeedback = "Please check your username and password.";
            }
        }
    }
}
