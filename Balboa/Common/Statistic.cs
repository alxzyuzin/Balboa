/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Contain infomation from "stats" command result 
 *
  --------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using Windows.UI.Core;

namespace Balboa.Common
{
    
    public class Statistic : INotifyPropertyChanged
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

        private int     _artists = 0;   // artists: number of artists
        private int     _albums = 0;    // albums: number of albums
        private int     _songs = 0;     // songs: number of songs
        private int     _uptime = 0;    // uptime: daemon uptime in seconds
        private int     _db_Playtime = 0; // db_playtime: sum of all song times in the db
        private int     _db_Update = 0; // db_update: last db update in UNIX time
        private DateTime _db_UpdateDT; // last db update in Date Time
        private int     _playtime = 0;  // playtime: time length of music played


        public int Artists // artists: number of artists
        {
            get { return _artists; }
            private set
            {
                if (_artists != value)
                {
                    _artists = value;
                    NotifyPropertyChanged("Artists");
                }
            }
        } 

        public int Albums // albums: number of albums
        {
            get { return _albums; }
            private set
            {
                if (_albums != value)
                {
                    _albums = value;
                    NotifyPropertyChanged("Albums");
                }
            }
        } 

        public int Songs // songs: number of songs
        {
            get { return _songs; }
            private set
            {
                if (_songs != value)
                {
                    _songs = value;
                    NotifyPropertyChanged("Songs");
                }
            }
        } 

        public int Uptime  // uptime: daemon uptime in seconds
        {
            get { return _uptime; }
            private set
            {
                if (_uptime != value)
                {
                    _uptime = value;
                    NotifyPropertyChanged("Uptime");
                }
            }
        } 

        public int DB_Playtime  // db_playtime: sum of all song times in the db
        {
            get { return _db_Playtime; }
            private set
            {
                if (_db_Playtime != value)
                {
                    _db_Playtime = value;
                    NotifyPropertyChanged("DB_Playtime");
                }
            }
        }

        public int DB_Update  // db_update: last db update in UNIX time
        {
            get { return _db_Update; }
            private set
            {
                if (_db_Update != value)
                {
                    _db_Update = value;
                    NotifyPropertyChanged("DB_Update");
                }
            }
        }

        public DateTime DB_UpdateDT
        {
            get { return _db_UpdateDT; }
            private set
            {
                if (_db_UpdateDT != value)
                {
                    _db_UpdateDT = value;
                    NotifyPropertyChanged("DB_UpdateDT");
                }
            }
        }


        public int Playtime  // playtime: time length of music played
        {
            get { return _playtime; }
            private set
            {
                if (_playtime != value)
                {
                    _playtime = value;
                    NotifyPropertyChanged("Playtime");
                }
            }

        }
        
        public Statistic(MainPage mainpage)
        {
            _mainPage = mainpage;
        }
  
        public void Update(MPDResponce statsinfo)
        {
            foreach (string item in statsinfo)
            {
                string[] statsitem = item.Split(':');
                switch (statsitem[0])
                {
                  case "artists": Artists = int.Parse(statsitem[1]); break;
                  case "albums": Albums = int.Parse(statsitem[1]); break;
                  case "songs": Songs = int.Parse(statsitem[1]); break;
                  case "uptime": Uptime = int.Parse(statsitem[1]); break;
                  case "db_playtime": DB_Playtime = int.Parse(statsitem[1]); break;
                  case "db_update":
                          DB_Update = int.Parse(statsitem[1]);
                            // Unix timestamp is seconds past epoch
                            DateTime db_update_dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            DB_UpdateDT = db_update_dt.AddSeconds(DB_Update).ToLocalTime();
                            break;
                  case "playtime": Playtime = int.Parse(statsitem[1]); break;
                 }
             }
        }
    }
}
