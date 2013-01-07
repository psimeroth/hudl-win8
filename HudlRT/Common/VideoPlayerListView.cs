using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace HudlRT.Common
{
    public class VideoPlayerListView : ListView
    {
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }
        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
