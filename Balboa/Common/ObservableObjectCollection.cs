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
}
