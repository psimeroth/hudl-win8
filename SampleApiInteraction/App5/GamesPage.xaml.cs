using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using App5.Common;
using System.Threading.Tasks;
using Newtonsoft.Json;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace App5
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class GamesPage : App5.Common.LayoutAwarePage
    {
        PassToSplit gValue;
        public GamesPage()
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
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
            gValue = (PassToSplit)navigationParameter;

            Task<string> test2 = ServiceAccessor.MakeApiCall(AppData.URL_BASE + AppData.URL_SERVICE_GET_SCHEDULE.Replace("#", gValue.teamID.ToString()), "GET", "", gValue.Token);
            var asyncAction2 = test2.AsAsyncOperation<string>().Completed += AsyncActionHandler2;
            Group gp = new Group { Title = gValue.Title + " Games" };
            this.DefaultViewModel["Group"] = gp;
        }

        private void AsyncActionHandler2(IAsyncOperation<string> asyncInfo, AsyncStatus asyncStatus)
        {
            string teamCallRetVal = asyncInfo.GetResults();
            //teamCallRetVal = teamCallRetVal.Replace('\\', ' ');
            List<Game> response = JsonConvert.DeserializeObject<List<Game>>(teamCallRetVal);


            //Items = response;
            this.DefaultViewModel["Items"] = response;
            foreach (Game r in response)
            {
                r.Title = r.opponent;
            }


            this.DefaultViewModel["Items"] = response;
        }

        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var gameId = ((Game)e.ClickedItem).gameId;
            PassToSplit value = new PassToSplit();
            value.teamID = 4313432;
            value.Token = gValue.Token;
            value.Title = ((Game)e.ClickedItem).Title;
            this.Frame.Navigate(typeof(CutupsPage), value);
        }
    }
}
