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

        private string _lastModified;
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

        private float _time;
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

        private string _artist;
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

        private string _title;
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

        private string _album;
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

        private string _date;
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

        private string _track;
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

        private string _genre;
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

        private string _composer;
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

        private string _albumArtist;
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

        private string _disc;
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

        private string _position;
        public string Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }

        private int _id;
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

        public CurrentSong(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public void Update(MpdResponseCollection currentSongInfo)
        {
            Parse(currentSongInfo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void Parse(MpdResponseCollection currentsonginfo)
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
                    case "Time":          Duration = float.Parse(tagvalue,System.Globalization.CultureInfo.InvariantCulture); break;
                    case "Artist":        Artist = tagvalue; break;
                    case "Title":         Title = tagvalue; break;
                    case "Album":         Album = tagvalue; break;
                    case "Date":          Date = tagvalue; break;
                    case "Track":         Track = tagvalue; break;
                    case "Genre":         Genre = tagvalue; break;
                    case "Composer":      Composer = tagvalue; break;
                    case "AlbumArtist":   AlbumArtist = tagvalue; break;
                    case "Disc":          Disc = tagvalue; break;
                    case "Pos":           Position = tagvalue; break;
                    case "Id":            Id = int.Parse(tagvalue, System.Globalization.CultureInfo.InvariantCulture); break;
                }
            }

            // Заполним пустые параметры значениями по умолчанию
            if (Title.Length == 0) Title = Utilities.ExtractFileName(File ?? "", true);
            if (Artist.Length == 0) Artist = " Unknown artist";
            if (Album.Length == 0) Album = " Unknown album";
            if (Date.Length == 0) Date = " Unknown year";
        }
    }
}
