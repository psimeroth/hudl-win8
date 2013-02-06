using HudlRT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HudlRT.Common
{
    public class TemplateSelector : DataTemplateSelector
    {
        protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            GameViewModel game = item as GameViewModel;

            if (game.isLargeView)
            {
                return  this.LargeTemplate;
            }
            return this.SmallTemplate;
        }

        public DataTemplate LargeTemplate { get; set; }
        public DataTemplate SmallTemplate { get; set; }
    }
}
