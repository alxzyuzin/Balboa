/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных общего элемента списка
 * Элемент списка не требующий специального списка полей
 *
  --------------------------------------------------------------------------*/
using System.ComponentModel;

namespace Balboa.Common
{
    public sealed class CommonGridItem: INotifyPropertyChanged, IUpdatable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotyfyPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
        }

        private string _name = string.Empty;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotyfyPropertyChanged("Name");
                }
            }
        }

        public void Update(MPDResponce responce)
        {
           string[] items = responce[0].Split(':');
            if (items.Length > 1)
                Name = items[1].Trim();
            else
                Name = "Undefined";
           responce.RemoveAt(0);
        }
    }
}
