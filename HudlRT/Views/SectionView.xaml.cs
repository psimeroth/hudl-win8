using HudlRT.Common;
using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SectionView : LayoutAwarePage
    {
        private double FULL_OPAQUE = 1;
        private double FADED_OPAQUE = 0.5;

        private int selectedIndex { get; set; }
        private bool rightClicked { get; set; }

        public SectionView()
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
            Cutups.IsEnabled = true;
            Schedule.IsEnabled = true;
            Categories.IsEnabled = true;
            GoBack.IsEnabled = true;
            SeasonsDropDown.IsEnabled = true;
            SeasonsDropDown.Opacity = FULL_OPAQUE;
            Logo.Opacity = FULL_OPAQUE;
            ProgressRing.Visibility = Visibility.Collapsed;
        }

        private void ListViewItemPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ListView l = (ListView)sender;
            selectedIndex = l.SelectedIndex;
            rightClicked = true;
            e.Handled = true;
        }

        private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightClicked)
            {
                ListView l = (ListView)sender;
                l.SelectedIndex = selectedIndex;
                rightClicked = false;
            }
            Schedule.ScrollIntoView(Schedule.SelectedItem);
        }

        private void GridViewItemPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            GridView l = (GridView)sender;
            selectedIndex = l.SelectedIndex;
            rightClicked = true;
            e.Handled = true;
        }

        private void GridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightClicked)
            {
                GridView l = (GridView)sender;
                l.SelectedIndex = selectedIndex;
                rightClicked = false;
            }
            Schedule.ScrollIntoView(Schedule.SelectedItem);
        }

        private void ResetComboBoxColor(object sender, object e)
        {
            SectionViewModel vm = (SectionViewModel)this.DataContext;
            vm.SeasonSelected(SeasonsDropDown.SelectedItem);
        }

        private void Cutup_Selected(object sender, ItemClickEventArgs e)
        {
            SeasonsDropDown.Opacity = FADED_OPAQUE;
            Logo.Opacity = FADED_OPAQUE;
        }
    }
}
