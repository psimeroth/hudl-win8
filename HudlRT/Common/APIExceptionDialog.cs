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
            message += "\nPlease connect to the internet, and then try again if you wish to view non-downloaded content.";
            ShowExceptionDialog(message, false, sender, e);
        }

        static public void ShowNoInternetConnectionLoginDialog(object sender, RoutedEventArgs e)
        {
            string message = "We didn't detect an internet connection.";
            message += "\nTo watch downloaded content while offline, please choose \"Remember Me\" next time you login";
            ShowExceptionDialog(message, false, sender, e);
        }

        static public void ShowGeneralExceptionDialog(object sender, RoutedEventArgs e)
        {
            string message = "Looks like something broke!";
            message += "\nPlease let us know what you were doing prior to seeing this error and we will work on getting this problem resolved. You can leave feedback with the button below or email kyle.deterding@hudl.com";

            ShowExceptionDialog(message, true, sender, e);
        }

        static public void ShowStatusCodeExceptionDialog(object sender, RoutedEventArgs e, string statusCode, string url)
        {
            string message;
            if (statusCode == "Unauthorized")
            {
                message = "Unauthorized Access.";
                message += "\nYou cannot access this content - if you think you should have access, please email support@hudl.com.";
            }

            else
            {
                message = "Server Error.";
                message += "\nWhen contacting the server, we ran into an error. Please email kyle.deterding@hudl.com with the following URL and Error Code: \nURL: " + url + "\nStatus Code: " + statusCode;
            }

            ShowExceptionDialog(message, true, sender, e);
        }

        static public async void ShowExceptionDialog(string text, bool feedback, object sender, RoutedEventArgs e)
        {
            string message = text;
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(message);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            if (feedback)
            {
                messageDialog.Commands.Add(new UICommand(
                    "Submit Feedback",
                    new UICommandInvokedHandler(CommandInvokedHandler)));
            }
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
