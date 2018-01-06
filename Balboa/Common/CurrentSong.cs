/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Хранит данные текущего воспроизводимого трека.
 *
 --------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using Windows.UI.Core;

namespace Balboa.Common
{
    public sealed class CurrentSong : INotifyPropertyChanged
    {
        #region

        public event PropertyChangedEventHandler PropertyChanged;

        private async void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
              await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); });
            }
        }

        #endregion

        private MainPage _mainPage;

        private string _file;
        private string _lastModified;
        private float  _time;
        private string _artist;
        private string _title;
        private string _album;
        private string _date;
        private string _track;
        private string _genre;
        private string _composer;
        private string _albumArtist;
        private string _disc;
        private string _pos;
        private int    _id;

        public string File
        {
            get { return _file; }
            set
            {
                if (_file != value)
                {
                    _file = value;
                    NotifyPropertyChanged("File");
                }
            }
        }
        public string LastModified
        {
            get { return _lastModified; }
            set
            {
                if (_lastModified!=value)
                {
                    _lastModified = value;
                    NotifyPropertyChanged("LastModified");
                }
            }
        }
        public float Duration
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    NotifyPropertyChanged("Duration");
                }
            }
        }
        public string Artist
        {
            get { return _artist; }
            set
            {
                if(_artist != value)
                { 
                    _artist = value;
                    NotifyPropertyChanged("Artist");
                }
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                if (_album != value)
                {
                    _album = value;
                    NotifyPropertyChanged("Album");
                }
            }
        }
        public string Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    NotifyPropertyChanged("Date");
                }
            }
        }
        public string Track
        {
            get { return _track; }
            set
            {
                if (_track != value)
                {
                    _track = value;
                    NotifyPropertyChanged("Track");
                }
            }
        }
        public string Genre
        {
            get { return _genre; }
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    NotifyPropertyChanged("Genre");
                }
            }
        }
        public string Composer
        {
            get { return _composer; }
            set
            {
                if (_composer != value)
                {
                    _composer = value;
                    NotifyPropertyChanged("Composer");
                }
            }
        }
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set
            {
                if (_albumArtist != value)
                {
                    _albumArtist = value;
                    NotifyPropertyChanged("AlbumArtist");
                }
            }
        }
        public string Disc
        {
            get { return _disc; }
            set
            {
                if (_disc != value)
                {
                    _disc = value;
                    NotifyPropertyChanged("Disc");
                }
            }
        }
        public string Pos
        {
            get { return _pos; }
            set
            {
                if (_pos != value)
                {
                    _pos = value;
                    NotifyPropertyChanged("Pos");
                }
            }
        }
        public int    Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public CurrentSong(MainPage mainpage)
        {
            _mainPage = mainpage;
        }

        public void Update(MPDResponce currentsonginfo)
        {
            Parse(currentsonginfo);
        }

        private void Parse(MPDResponce currentsonginfo)
        {

            // Заполним пустые параметры значениями по умолчанию
            Title  = string.Empty;
            Artist = string.Empty;
            Album  = string.Empty;
            Date   = string.Empty;

            foreach (string item in currentsonginfo)
            {
                string tagvalue=string.Empty;
                string[] items = item.Split(':');
                if (items.Length > 1)
                    tagvalue = items[1].Trim();
                
                switch (items[0])
                {
                    case "file":          File = tagvalue; break;
                    case "Last-Modified": LastModified = tagvalue; break;
                    case "Time":          Duration = float.Parse(tagvalue); break;
                    case "Artist":        Artist = tagvalue; break;
                    case "Title":         Title = tagvalue; break;
                    case "Album":         Album = tagvalue; break;
                    case "Date":          Date = tagvalue; break;
                    case "Track":         Track = tagvalue; break;
                    case "Genre":         Genre = tagvalue; break;
                    case "Composer":      Composer = tagvalue; break;
                    case "AlbumArtist":   AlbumArtist = tagvalue; break;
                    case "Disc":          Disc = tagvalue; break;
                    case "Pos":           Pos = tagvalue; break;
                    case "Id":            Id = int.Parse(tagvalue); break;
                }
            }

            // Заполним пустые параметры значениями по умолчанию
            if (Title.Length == 0) Title = Utils.ExtractFileName(File ?? "", true);
            if (Artist.Length == 0) Artist = " Unknown artist";
            if (Album.Length == 0) Album = " Unknown album";
            if (Date.Length == 0) Date = " Unknown year";
        }
    }
}
