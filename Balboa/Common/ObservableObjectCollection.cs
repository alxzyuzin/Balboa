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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Windows.UI.Core;


namespace Balboa.Common
{
    public class ObsevableObjectCollection<T>: ObservableCollection<T>
        where T : INotifyPropertyChanged, IUpdatable
    {
        private MainPage    _mainPage;
       // private MethodInfo  _method;
       // private EventInfo   _event;
       // private object[]    _parseinputparameters = new object[1];
        //
        // Summary:
        //     Occurs when the collection changes.
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        private delegate void NotifyCollectionItemPropertyChangedEventHandler() ;

        private async void NotifyCollectionChanged()
        {
            if (CollectionChanged != null)
            {
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); });
            }
        }
  

        public ObsevableObjectCollection(MainPage mainpage)
        {
            _mainPage = mainpage;

        }

        public void Update(MPDResponce responcedata)
        {
            Clear();
            if(responcedata.Count > 0)
                Parse(responcedata);
            NotifyCollectionChanged();
        }

        /// <summary>
        ///  Создаёт новый экземпляр элемента коллекции, вызывает метод Parse созданного элемента и передаёт 
        /// этому методу набор данных полученных от сервера.
        /// Экземпляр элемента коллекции забирает из полученного набора данных свою порцию и разносит данные
        ///  по своим свойствам.
        /// Остаток данных возвращается для обработки следующим элементом
        /// </summary>
        /// <param name="responce"></param>
        public void Parse(MPDResponce responce)
        {
           do
           {
                T collectionitem = (T)Activator.CreateInstance(typeof(T));
                collectionitem.PropertyChanged += ItemPropertyChanged;
                collectionitem.Update(responce);
                Add(collectionitem);
            }
           while (responce.Count > 0);
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected")
                NotifyCollectionChanged();
        }


        public void ClearAndNotify()
        {
            base.Clear();
            NotifyCollectionChanged();
        }

        
        public void Sort(GenericComparer<T> comparer)
        {
            bool unsorted = false;

            do
            {
                unsorted = false;
                int i = 0;
                int j = 1;

                while (j < Count)
                {
                   if (comparer.Compare(this[i],this[j]) == 1)
                   {
                       Move(j, i);
                       unsorted = true;
                   }
                   i++;
                   j++;
               }
            }
            while (unsorted);
            NotifyCollectionChanged();
        }

        /// <summary>
        /// Возвращает отсортированный список уникальных значений заданного поля коллекции
        /// </summary>
        /// <returns></returns>
        public List<string> GetFieldValues(string fieldname)
        {
            List<string> values = new List<string>();
            Type type = typeof(T); 
            PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldname);

            for (int i=0; i<this.Count; i++)
            {
                string value = (string)propertyInfo.GetValue(this[i]);
                if (!values.Contains(value))
                    values.Add(value);              
            }
            values.Sort();

            return values;  
        }

        public bool Contains(string fieldname, object fieldvalue)
        {
            Type type = typeof(T);
            PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldname);
            for (int i = 0; i < this.Count; i++)
            {
                object value = propertyInfo.GetValue(this[i]);
                if (value.ToString() == fieldvalue.ToString())
                    return true;
            }
            return false;
        }

       
    }
}
