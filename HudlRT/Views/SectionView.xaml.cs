using HudlRT.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HudlRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SectionView : LayoutAwarePage
    {
        public SectionView()
        {
            this.InitializeComponent();
            CategoriesGridView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CategoriesGridView.SelectedIndex = -1;
            LoadingRing.IsActive = false;
        }
    }
}
