using Caliburn.Micro;
using HudlRT.ViewModels;
using System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;

namespace HudlRT.Common
{
    class CharmsData
    {
        public static INavigationService navigationService { get; set; }
        public static void SettingCharmManager_LoginCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            eventArgs.Request.ApplicationCommands.Clear();
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy Policy", OpenPrivacyPolicy));
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("win8Support", "Support", OpenWin8Support));
        }

        public static void SettingCharmManager_HubCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            eventArgs.Request.ApplicationCommands.Clear();
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy Policy", OpenPrivacyPolicy));
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("win8Support", "Support", OpenWin8Support));
            if (AppDataAccessor.GetDemoMode())
            {
                eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("newToHudl", "New to hudl", OpenSignup));
            }
            var currentUserName = AppDataAccessor.GetUsername();
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("logout", "Log Out", Logout));
        }
      
        public static async void OpenPrivacyPolicy(IUICommand command)
        {
            Uri uri = new Uri("http://www.hudl.com/privacy/");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        public static void Logout(IUICommand command)
        {
            AppDataAccessor.RemovePasswords();
            AppDataAccessor.SetDemoMode(false);
            navigationService.NavigateToViewModel<LoginViewModel>();
        }

        public static async void OpenSignup(IUICommand command)
        {
            var uri = new Uri("http://www.hudl.com/signup");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
        
        public static async void OpenWin8Support(IUICommand command)
        {
            var uri = new Uri("http://public.hudl.com/support/mobile-apps/windows-8/");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
