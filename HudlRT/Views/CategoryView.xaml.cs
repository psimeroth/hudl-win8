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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HudlRT.Views
{
    public sealed partial class CategoryView : UserControl
    {
        private SolidColorBrush textColor = null;

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
