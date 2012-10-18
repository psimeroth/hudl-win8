using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;


namespace HudlRT.Common
{
    class CharmsData
    {
        public static void SettingCharmManager_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            eventArgs.Request.ApplicationCommands.Clear();
            eventArgs.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy Policy", OpenPrivacyPolicy));
        }

        public static async void OpenPrivacyPolicy(IUICommand command)
        {
            Uri uri = new Uri("http://www.hudl.com/privacy/");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

    }
}
