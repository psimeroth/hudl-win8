using App5.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using App5.Common;
using System.Net;
using System.Threading.Tasks;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace App5
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : App5.Common.LayoutAwarePage
    {
        bool loginDone = false;
        string loginRetVal = null;
        bool teamCallDone = false;
        string teamCallRetVal = null;
        //List<TeamResponse> Items;
        

        public ItemsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);

            const string username = AppData.LOGIN_USERNAME;
            const string password = AppData.LOGIN_PASSWORD;

            var loginArgs = JsonConvert.SerializeObject(new LoginSender {Username = username, Password = password});

            
            Task<string> test = ServiceAccessor.MakeApiCall(AppData.URL_BASE_SECURE + AppData.URL_SERVICE_LOGIN, "POST", loginArgs, "");
            var asyncAction = test.AsAsyncOperation<string>().Completed += AsyncActionHandler;

            //while (!loginDone) { }

            //var obj = JsonConvert.DeserializeObject<LoginProfileDTO>(loginRetVal);

            //Task<string> test2 = ServiceAccessor.MakeApiCall(AppData.URL_BASE + AppData.URL_SERVICE_GET_TEAMS, "GET", "", obj.Token);
            //var asyncAction2 = test.AsAsyncOperation<string>().Completed += AsyncActionHandler2;

            //while (!teamCallDone) { }



            //this.DefaultViewModel["Items"] = sampleDataGroups;
        }

        private void AsyncActionHandler(IAsyncOperation<string> asyncInfo, AsyncStatus asyncStatus)
        {
            loginRetVal = asyncInfo.GetResults();
            loginDone = true;
            var obj = JsonConvert.DeserializeObject<LoginProfileDTO>(loginRetVal);

            Task<string> test2 = ServiceAccessor.MakeApiCall(AppData.URL_BASE + AppData.URL_SERVICE_GET_TEAMS, "GET", "", obj.Token);
            var asyncAction2 = test2.AsAsyncOperation<string>().Completed += AsyncActionHandler2;
        }

        private void AsyncActionHandler2(IAsyncOperation<string> asyncInfo, AsyncStatus asyncStatus)
        {
            teamCallRetVal = asyncInfo.GetResults();
            //teamCallRetVal = teamCallRetVal.Replace('\\', ' ');
            List<TeamResponse> response = JsonConvert.DeserializeObject<List<TeamResponse>>(teamCallRetVal);
            //Items = response;
            this.DefaultViewModel["Items"] = response;
            foreach(TeamResponse r in response)
            {
                if (r.name == "")
                {
                    r.name = "No Name";
                }
                r.Title = r.name;
            }
            teamCallDone = true;
        }


        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((TeamResponse)e.ClickedItem).teamId;
            PassToSplit value = new PassToSplit();
            value.teamID = groupId;
            value.Token = JsonConvert.DeserializeObject<LoginProfileDTO>(loginRetVal).Token;
            value.Title = ((TeamResponse)e.ClickedItem).Title;
            this.Frame.Navigate(typeof(SplitPage), value);
        }
    }
}
