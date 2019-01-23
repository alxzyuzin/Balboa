/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Базовый класс для создания классов списков объектов в приложении.
 * Все классы списков объектов являются наследниками этого класса
 * Класс реализует интерфейс INotifyCollectionChanged и функцию Sort
 * Классы объектов включаемых в данную корекцию должны реализовывать метод Update
 --------------------------------------------------------------------------*/


using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Windows.UI.Core;


namespace Balboa.Common
{

    public class TrackCollection<Track> : ObservableCollection<Track>
      where Track : INotifyPropertyChanged
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public void NotifyCollectionChanged()
        {

            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));


        }

    }

//public class ObservableObjectCollection<T>: ObservableCollection<T>
//        where T : INotifyPropertyChanged
//    {
 
// //       public override event NotifyCollectionChangedEventHandler CollectionChanged;

 
//        //public void Sort(GenericComparer<T> comparer)
//        //{
//        //    if (comparer == null) throw new ArgumentNullException(nameof(comparer));

//        //    bool unsorted = false;
//        //    do
//        //    {
//        //        unsorted = false;
//        //        int i = 0;
//        //        int j = 1;

//        //        while (j < Count)
//        //        {
//        //           if (comparer.Compare(this[i],this[j]) == 1)
//        //           {
//        //               Move(j, i);
//        //               unsorted = true;
//        //           }
//        //           i++;
//        //           j++;
//        //       }
//        //    }
//        //    while (unsorted);
//        //    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
//        // }

//        /// <summary>
//        /// Возвращает отсортированный список уникальных значений заданного поля коллекции
//        /// </summary>
//        /// <returns></returns>
//        //public List<string> GetFieldValues(string fieldName)
//        //{
//        //    List<string> values = new List<string>();
//        //    Type type = typeof(T); 
//        //    PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldName);
//        //    if (propertyInfo == null)
//        //    {
//        //        string message = string.Format(CultureInfo.InvariantCulture,"Not found property {0} in type {1}", fieldName, type.Name);
//        //        throw new BalboaException("ObservableObjectCollection", "GetFieldValues", "135", message);
//        //    }

//        //    for (int i=0; i<Count; i++)
//        //    {
//        //        string value = (string)propertyInfo.GetValue(this[i]);
//        //        if (!values.Contains(value))
//        //            values.Add(value);              
//        //    }
//        //    values.Sort();

//        //    return values;  
//        //}

//        //public bool Contains(string fieldName, object fieldValue)
//        //{
//        //    if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
//        //    if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
 
//        //    Type type = typeof(T);
//        //    PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldName);

//        //    if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
 
//        //    for (int i = 0; i < this.Count; i++)
//        //    {
//        //        object value = propertyInfo.GetValue(this[i]);
//        //        if (value.ToString() == fieldValue.ToString())
//        //            return true;
//        //    }
//        //    return false;
//        //}
       
//    }
}
