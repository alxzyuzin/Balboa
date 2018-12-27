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
    public class ObservableObjectCollection<T>: System.Collections.ObjectModel.ObservableCollection<T>
        where T : INotifyPropertyChanged, IUpdatable
    {
        private const string _modName = "ObservableObjectCollection.cs";

        private MainPage    _mainPage;
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

        public ObservableObjectCollection(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public void Update(MpdResponseCollection responseData)
        {
           if (responseData == null)
                return;
            Clear();
            if(responseData.Count > 0)
                Parse(responseData);
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
        public void Parse(MpdResponseCollection response)
        {
            if (response == null)
                return;
           do
           {
                T collectionitem = (T)Activator.CreateInstance(typeof(T));
                collectionitem.PropertyChanged += ItemPropertyChanged;
                collectionitem.Update(response);
                Add(collectionitem);
            }
           while (response.Count > 0);
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
            if (comparer == null)
                throw new BalboaNullValueException(_modName, "Sort", "96", "comparer");

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
        public List<string> GetFieldValues(string fieldName)
        {
            List<string> values = new List<string>();
            Type type = typeof(T); 
            PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldName);
            if (propertyInfo == null)
            {
                string message = string.Format(CultureInfo.InvariantCulture,"Not found property {0} in type {1}", fieldName, type.Name);
                throw new BalboaException(_modName, "GetFieldValues", "135", message);
            }

            for (int i=0; i<Count; i++)
            {
                string value = (string)propertyInfo.GetValue(this[i]);
                if (!values.Contains(value))
                    values.Add(value);              
            }
            values.Sort();

            return values;  
        }

        public bool Contains(string fieldName, object fieldValue)
        {
            if (fieldValue == null)
                throw new BalboaNullValueException(_modName, "Contains", "150", "fieldValue");

            Type type = typeof(T);
            PropertyInfo propertyInfo = type.GetRuntimeProperty(fieldName);
            if (propertyInfo == null)
                throw new BalboaNullValueException(_modName, "Contains", "156", "propertyInfo");

            for (int i = 0; i < this.Count; i++)
            {
                object value = propertyInfo.GetValue(this[i]);
                if (value.ToString() == fieldValue.ToString())
                    return true;
            }
            return false;
        }

       
    }
}
