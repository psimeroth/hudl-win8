using HudlRT.Models;
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

namespace HudlRT.Views
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPrototypeView : HudlRT.Common.LayoutAwarePage
    {
        public HubPrototypeView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        private void HubGames_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var x = (HubPageGameEntry)sender;
        }

        private void GridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var x = (GridView)sender;
            var y = e.AddedItems.FirstOrDefault();
        }
    }
}