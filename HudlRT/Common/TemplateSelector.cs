using HudlRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HudlRT.Common
{
    public class TemplateSelector : DataTemplateSelector
    {
        protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            GameViewModel game = item as GameViewModel;

            if (game.IsLargeView)
            {
                return  this.LargeTemplate;
            }
            return this.SmallTemplate;
        }

        public DataTemplate LargeTemplate { get; set; }
        public DataTemplate SmallTemplate { get; set; }
        public DataTemplate OtherCategoryTemplate { get; set; }
    }
}
