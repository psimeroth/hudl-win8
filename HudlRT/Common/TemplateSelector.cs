using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HudlRT.Common
{
    /*public class TemplateSelector : DataTemplateSelector
    {
        protected override Windows.UI.Xaml.DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            var project = item as Project;

            var uiElement = container as UIElement;

            if (project.IsStarred)
            {
                VariableSizedWrapGrid.SetColumnSpan(uiElement, 1);
                VariableSizedWrapGrid.SetRowSpan(uiElement, 2);

                return App.Current.Resources["FavoriteProjectItemTemplate"] as DataTemplate;
            }

            VariableSizedWrapGrid.SetColumnSpan(uiElement, 1);
            VariableSizedWrapGrid.SetRowSpan(uiElement, 1);

            return App.Current.Resources["ProjectItemTemplate"] as DataTemplate;
        }
    }

    }*/
}
