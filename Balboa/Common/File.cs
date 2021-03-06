﻿/*-----------------------------------------------------------------------
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
    internal enum FileNature { File, Directory, Playlist }

    internal class File: IComparable, INotifyPropertyChanged, IUpdatable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged!=null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name = string.Empty;
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

        private FileNature _nature = FileNature.File;
        public FileNature Nature
        {
            get { return _nature; }
            set
            {
                if (_nature != value)
                {
                    _nature = value;
                    NotifyPropertyChanged("Nature");
                }
            }

        }

        private string _lastModified = string.Empty;
        public string   LastModified
        {
            get { return _lastModified; }
            set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    NotifyPropertyChanged("LastModified");
                }
            }
        }

        private string _icon = string.Empty;
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

        private bool _justclosed = false;
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

        private BitmapImage _imagesource = null;
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

        public void Update(MpdResponseCollection response)
        {
            int i = 0;
            if (response == null)
                return;
            do
            {
                string[] items = response[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();
               
                switch (tagname)
                {
                   case "file":
                       Name = Utilities.ExtractFileName(tagvalue, false);
                       Nature = FileNature.File;
                       Icon += '\xE189';
                       break;           // 57737     // E189
                   case "directory":
                        Name = Utilities.ExtractFileName(tagvalue, false);
                        Nature = FileNature.Directory;
                        Icon += '\xE188';
                        break; // 57736    // E188
                   case "playlist": Name = tagvalue; Nature = FileNature.Playlist; break;
                   case "Last-Modified": LastModified = tagvalue; break;
                 }
                    i++;
             }
             while ((i < response.Count) && (!response[i].StartsWith("file",StringComparison.OrdinalIgnoreCase)) && 
                (!response[i].StartsWith("playlist", StringComparison.OrdinalIgnoreCase)) &&
                (!response[i].StartsWith("directory", StringComparison.OrdinalIgnoreCase)));
             response.RemoveRange(0, i);
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

        public int CompareTo(object obj)
        {
            File fitem = obj as File;
            if (this.Nature == FileNature.Directory && fitem.Nature == FileNature.Directory)
                {
                    return string.Compare(this.Name, fitem.Name, StringComparison.Ordinal);
                }

            if (this.Nature == FileNature.Directory && fitem.Nature == FileNature.File)
                {
                    return -1;
                }

            if (this.Nature == FileNature.File && fitem.Nature == FileNature.Directory)
                {
                    return 1;
                }

            if (this.Nature == FileNature.File && fitem.Nature == FileNature.File)
                {
                    return string.Compare(this.Name, fitem.Name, StringComparison.Ordinal);
                }
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            return this.CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            char[] c = this.Name.ToCharArray();
            return (int) c[0];
        }


        public static bool operator ==(File left, File right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            
            return left.Equals(right);
        }

        public static bool operator !=(File left, File right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }
    }
}
