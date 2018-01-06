using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Balboa.Common;

namespace Balboa.Common
{
	public sealed class PlaylistItemStyleSelector : StyleSelector
    {
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            Track listitem = item as Track;

            Style style = new Style(typeof(ListViewItem));
            // Default Style
            style.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty,HorizontalAlignment.Stretch)) ;
            style.Setters.Add(new Setter(ListViewItem.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0x10, 0xF0, 0xF0, 0xF0))));

            // Custom IsPlaying Style
            if (listitem.IsPlaying)
            {
                style.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, new SolidColorBrush(Colors.Orange))); 
                //      style.Setters.Add(new Setter(ListViewItem.FontSizeProperty, 30)); 
//                style.Setters.Add(new Setter(ListViewItem.MarginProperty, new Thickness(40)));
                style.Setters.Add(new Setter(ListViewItem.ForegroundProperty, new SolidColorBrush(Colors.Orange)));
                
                //style.Setters.Add(new Setter(ListViewItem.BackgroundProperty, new SolidColorBrush(Colors.Aqua)));
                style.Setters.Add(new Setter(ListViewItem.BorderThicknessProperty, new Thickness(1.0))); 
                style.Setters.Add(new Setter(ListViewItem.BorderBrushProperty, new SolidColorBrush(Colors.Orange)));

               // style.Setters.Add(new Setter(ListViewItemIsSelectedProperty, true));
            }
                return style;
        }
    }
}
