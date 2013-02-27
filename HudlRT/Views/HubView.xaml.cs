using HudlRT.Common;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HubView : LayoutAwarePage
    {
        public HubView()
        {
            this.InitializeComponent();
            ItemsByCategory.SelectionMode = ListViewSelectionMode.Multiple;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Grid_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
        }

        private void SeasonsDropDown_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
