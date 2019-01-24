/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для установки стиля элемента списка файлов.
 *
 --------------------------------------------------------------------------*/

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Balboa.Common
{
    internal sealed class FileItemStyleSelector : StyleSelector
    { 

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            var listitem = item as File;

            Style style = new Style(typeof(GridViewItem));
            // Default Style
            style.Setters.Add(new Setter(GridViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            style.Setters.Add(new Setter(GridViewItem.MarginProperty, new Thickness(0, 0, 2, 2)));
            style.Setters.Add(new Setter(GridViewItem.BackgroundProperty, new SolidColorBrush(Color.FromArgb(0x10, 0xF0, 0xF0, 0xF0))));
 
            style.Setters.Add(new Setter(GridViewItem.WidthProperty, 400));
            style.Setters.Add(new Setter(GridViewItem.HeightProperty, 70));

            // Custom IsPlaying Style
            if (listitem.JustClosed)
            {
                // style.Setters.Add(new Setter(GridViewItem.ForegroundProperty, new SolidColorBrush(Colors.Orange)));
                style.Setters.Add(new Setter(GridViewItem.BorderThicknessProperty, new Thickness(2.0)));
                style.Setters.Add(new Setter(GridViewItem.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(0x80, 0xFC, 0xFC, 0xFC))));
            }
            return style;
        }
    }
}
