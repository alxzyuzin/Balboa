/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс - наследник стандартного элемента управления Listview, 
 * Класс позволяет устанавливать свойство Background для элемента списка.
 *
 --------------------------------------------------------------------------*/


using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Balboa.Common
{
    internal class ListViewExtended : ListView
    {
               
        public int Count
        {
            get { return Items.Count; }
        }

        public static DependencyProperty ItemBackgroundProperty = DependencyProperty.Register("ItemBackground", typeof(Brush), typeof(ListViewExtended), null);

        public Brush ItemBackground
        {
            get { return (Brush)GetValue(ItemBackgroundProperty); }
            set { SetValue(ItemBackgroundProperty, (Brush)value); }
        }


        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)

        {
            base.PrepareContainerForItemOverride(element, item);
            var listViewItem = element as ListViewItem;

            if (listViewItem != null)
            {
                listViewItem.Background = ItemBackground;
            }
        }

        /*
        /// Эта реализация PrepareContainerForItemOverride
        /// выполняет привязку (DataBinding) свойства IsSelected элемента списка (элемента коллекции ListView.Items)
        /// к свойству IsSelected ListviewItem.
 
                protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
                {
                    base.PrepareContainerForItemOverride(element, item);

                    ListViewItem listItem = element as ListViewItem;

                    Binding binding = new Binding();
                    binding.Mode = BindingMode.TwoWay;
                    binding.Source = item;
                    binding.Path = new PropertyPath("IsSelected");
                    listItem.SetBinding(ListViewItem.IsSelectedProperty, binding);
                }
        */
    }
}
