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


    class BetaDialog
    {
        static public async void ShowBetaDialog(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            string message = "Welcome to Hudl for Windows 8!";
            message += "\n\nThis application is still in beta, meaning we're getting feedback from a select number of users and working hard to address any issues as they come up.";
            message += "\n\nIf you're experiencing any problems or have suggestions on how to improve the app, we'd love to here from you!";
            message += " You can click on the 'Submit Feedback' button in this dialog to send one of our developers an email. Thanks for participating in the beta!";
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
                await Windows.System.Launcher.LaunchUriAsync(new Uri("mailto:kyle.deterding@hudl.com?subject=HudlRT%20Beta%20Feedback"));
            }
        }
    }
}
