/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных общего элемента списка
 * Элемент списка не требующий специального списка полей
 *
  --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Balboa.Common
{
    public class SavedPlaylistItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
 
        private string _fileName = string.Empty;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileName)));
                }
            }
        }

        private string _lastModificationDate = string.Empty;
        public string LastModificationDate
        {
            get { return _lastModificationDate; }
            set
            {
                if (_lastModificationDate != value)
                {
                    _lastModificationDate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastModificationDate)));
                }
            }
        }


        //public void Update(List<string> response)
        //{
        //    if (response == null) throw new ArgumentNullException(nameof(response));
            

        //    string[] items = response[0].Split(':');
        //    if (items.Length > 1)
        //        Name = items[1].Trim();
        //    else
        //        Name = "Undefined";
        //    response.RemoveAt(0);
        //}


        public void Update(List<string> response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            int i =0;

            do
            {
                string[] items = response[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();

                switch (tagname)
                {
                    case "playlist":
                        FileName = Utilities.ExtractFileName(tagvalue, false);
                         break;
                    case "last-modified":
                        LastModificationDate = Utilities.ExtractFileName(tagvalue, false);
                        break;
                }
                i++;
            }
            while ((i < response.Count) && (!response[i].StartsWith("playlist", StringComparison.OrdinalIgnoreCase)));

            response.RemoveRange(0, i);
        }


    }
}

