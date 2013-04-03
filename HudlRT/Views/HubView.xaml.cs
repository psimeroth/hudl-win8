using HudlRT.Common;
using Windows.UI.Xaml.Controls;
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
    }
}
