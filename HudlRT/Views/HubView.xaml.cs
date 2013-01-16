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
        private int selectedIndex { get; set; }
        private bool rightClicked { get; set; }

        public HubView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The 
        /// 
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Enable all clickable elements
            VideoLibrary_Button.IsEnabled = true;
            LastViewed_Button.IsEnabled = true;
            PreviousGameCategories.IsEnabled = true;
            NextGameCategories.IsEnabled = true;
            SeasonsDropDown.IsEnabled = true;

            // Unfade all text
            SeasonsDropDown.Opacity = 1;
            Logo.Opacity = 1;
            NoGamesGrid.Opacity = 1;
            NextGameHeader_Text.Opacity = 1;
            NextGameHeaderDate_Text.Opacity = 1;
            PreviousGameHeader_Text.Opacity = 1;
            PreviousGameHeaderDate_Text.Opacity = 1;
        }

        private void ListViewItemPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ListView l = (ListView)sender;
            selectedIndex = l.SelectedIndex;
            rightClicked = true;
            e.Handled = true;
        }

        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightClicked)
            {
                ListView l = (ListView)sender;
                l.SelectedIndex = selectedIndex;
                rightClicked = false;
            }
        }

        private void ResetComboBoxColor(object sender, object e)
        {
            SeasonsDropDown.Background = new SolidColorBrush(Color.FromArgb(0x00, 0, 0, 0));
            SeasonsDropDown.BorderBrush = new SolidColorBrush(Color.FromArgb(0x00, 0, 0, 0));
            HubViewModel vm = (HubViewModel)this.DataContext;
            vm.SeasonSelected(SeasonsDropDown.SelectedItem);
        }

        private void LastViewedClicked(object sender, object e)
        {
            disablePageElements();
        }

        private void VideoLibraryClicked(object sender, object e)
        {
            disablePageElements();
        }

        private void CategorySelected(object sender, ItemClickEventArgs e)
        {
            disablePageElements();
        }

        private void disablePageElements()
        {
            // Diable all clickable elements
            VideoLibrary_Button.IsEnabled = false;
            LastViewed_Button.IsEnabled = false;
            PreviousGameCategories.IsEnabled = false;
            NextGameCategories.IsEnabled = false;
            SeasonsDropDown.IsEnabled = false;

            // Fade all text
            SeasonsDropDown.Opacity = 0.5;
            Logo.Opacity = 0.5;
            NoGamesGrid.Opacity = 0.5;
            NextGameHeader_Text.Opacity = 0.5;
            NextGameHeaderDate_Text.Opacity = 0.5;
            PreviousGameHeader_Text.Opacity = 0.5;
            PreviousGameHeaderDate_Text.Opacity = 0.5;
        }
    }
}
