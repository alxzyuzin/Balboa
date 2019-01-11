/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных общего элемента списка
 * Элемент списка не требующий специального списка полей
 *
  --------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.ComponentModel;

namespace Balboa.Common
{
    public sealed class CommonGridItem: INotifyPropertyChanged
    {
        private const string _fileName = "CommonGridItem.cs";

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

        //public void Update(MpdResponseCollection response)
        //{
        //    if (response == null)
        //    {
        //        throw new BalboaException(_fileName, this.GetType().FullName,"Update","43","Parameter 'response' is null");
        //    }

        //   string[] items = response[0].Split(':');
        //    if (items.Length > 1)
        //        Name = items[1].Trim();
        //    else
        //        Name = "Undefined";
        //   response.RemoveAt(0);
        //}

        public void Update(List<string> response)
        {
            if (response == null)
            {
                throw new BalboaException(_fileName, this.GetType().FullName, "Update", "43", "Parameter 'response' is null");
            }

            string[] items = response[0].Split(':');
            if (items.Length > 1)
                Name = items[1].Trim();
            else
                Name = "Undefined";
            response.RemoveAt(0);
        }
    }
}
