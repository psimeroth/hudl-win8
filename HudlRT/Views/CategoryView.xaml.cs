using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HudlRT.Views
{
    public sealed partial class CategoryView : UserControl
    {

        public CategoryView()
        {
            this.InitializeComponent();
        }

        private void Name_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Name.Opacity = .5;
        }

        private void Name_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Name.Opacity = 1;
        }    
    }
}
