/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных одиночного файла.
 * Функция  CompareTo реализует интерфейс IComparable
 *
 --------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace Balboa.Common
{
    public enum FileType { File, Directory, Playlist }

    public class File: IComparable, INotifyPropertyChanged, IUpdatable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged!=null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string      _name = string.Empty;
        private FileType    _type = FileType.File;
        private string      _lastModifyed = string.Empty;
        private string      _icon = string.Empty;
        private bool        _justclosed = false;
        private BitmapImage _imagesource = null;

        public string   Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        public FileType Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    NotifyPropertyChanged("Type");
                }
            }

        }
        public string   LastModifyed
        {
            get { return _lastModifyed; }
            set
            {
                if (_lastModifyed != value)
                {
                    _lastModifyed = value;
                    NotifyPropertyChanged("LastModifyed");
                }
            }
        }
        public string   Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }
        }
        public bool     JustClosed
        {
            get { return _justclosed; }
            set
            {
                if (_justclosed != value)
                {
                    _justclosed = value;
                    NotifyPropertyChanged("JustClosed");
                }
            }
        }
        public BitmapImage ImageSource
        {
            get { return _imagesource; }
            set
            {
                if (_imagesource != value)
                {
                    _imagesource = value;
                    NotifyPropertyChanged("ImageSource");
                }
            }
        }

        public void Update(MPDResponce responce)
        {
            int i = 0;
            do
            {
                string[] items = responce[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();
               
                switch (tagname)
                {
                   case "file":
                       Name = Utils.ExtractFileName(tagvalue, false);
                       Type = FileType.File;
                       Icon += '\xE189';
                       break;           // 57737     // E189
                   case "directory":
                        Name = Utils.ExtractFileName(tagvalue, false);
                        Type = FileType.Directory;
                        Icon += '\xE188';
                        break; // 57736    // E188
                   case "playlist": Name = tagvalue; Type = FileType.Playlist; break;
                   case "Last-Modified": LastModifyed = tagvalue; break;
                 }
                    i++;
             }
             while ((i < responce.Count) && (!responce[i].StartsWith("file")) && (!responce[i].StartsWith("playlist")) && (!responce[i].StartsWith("directory")));
            responce.RemoveRange(0, i);
        }
 
        /// <summary>
        /// Реализует интерфейс IComparable
        /// Сравнивает текущий объект с объектом переданным в качестве входного параметра
        /// Для сортировки в порядке возрастания считаем что объекты с типом FileSystemItemType.Directory 
        /// меньше объектов с типом FileSystemItemType.File
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        /// -1 - если текущий объект меньше переданного
        ///  0 - если текущий объект равен переданному
        ///  1 - если текущий объект больше переданного
        /// </returns>

        public int CompareTo(object item)
        {
            File fitem = item as File;
            if (this.Type == FileType.Directory && fitem.Type == FileType.Directory)
                {
                    return string.Compare(this.Name, fitem.Name, StringComparison.Ordinal);
                }

            if (this.Type == FileType.Directory && fitem.Type == FileType.File)
                {
                    return -1;
                }

            if (this.Type == FileType.File && fitem.Type == FileType.Directory)
                {
                    return 1;
                }

            if (this.Type == FileType.File && fitem.Type == FileType.File)
                {
                    return string.Compare(this.Name, fitem.Name, StringComparison.Ordinal);
                }
            return 0;
        }
    }
}
