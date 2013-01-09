using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HudlRT.Common
{
    class APIExceptionDialog
    {
        static public void ShowNoInternetConnectionDialog(object sender, RoutedEventArgs e)
        {
            string message = "We didn't detect an internet connection.";
            message += "\nPlease connect to the internet, and then try again. If you have connected to the internet and this error persists, please leave feedback with the button below or email kyle.deterding@hudl.com";
            ShowExceptionDialog(message, sender, e);
        }

        static public void ShowGeneralExceptionDialog(object sender, RoutedEventArgs e)
        {
            string message = "Looks like something broke!";
            message += "\nPlease let us know what you were doing prior to seeing this error and we will work on getting this problem resolved. You can leave feedback with the button below or email kyle.deterding@hudl.com";

            ShowExceptionDialog(message, sender, e);
        }

        static public void ShowStatusCodeExceptionDialog(object sender, RoutedEventArgs e, string statusCode, string url)
        {
            string message = "Server Error.";
            message += "\nWhen contacting the server, we ran into an error. Please email kyle.deterding@hudl.com with the following URL and Error Code: \nURL: " + url + "\nStatus Code: " + statusCode;
            ShowExceptionDialog(message, sender, e);
        }

        static private async void ShowExceptionDialog(string text, object sender, RoutedEventArgs e)
        {
            string message = text;
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(message);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(
                "Submit Feedback",
                new UICommandInvokedHandler(CommandInvokedHandler)));
            messageDialog.Commands.Add(new UICommand(
                "Close",
                new UICommandInvokedHandler(CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }
        static private async void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Submit Feedback")
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:kyle.deterding@hudl.com?subject=HudlRT%20Feedback"));
            }
        }
    }
}
