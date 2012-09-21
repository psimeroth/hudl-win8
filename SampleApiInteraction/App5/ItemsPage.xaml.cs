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
        PassToSplit gValue;
        

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
            gValue = (PassToSplit)navigationParameter;

            Task<string> test2 = ServiceAccessor.MakeApiCallGet(AppData.URL_BASE + AppData.URL_SERVICE_GET_TEAMS, "", gValue.Token);
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
            value.Token = gValue.Token;
            value.Title = ((TeamResponse)e.ClickedItem).Title;
            this.Frame.Navigate(typeof(GamesPage), value);
        }
    }
}
