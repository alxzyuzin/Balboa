/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Contain infomation from "stats" command result 
 *
  --------------------------------------------------------------------------*/

using System;
using System.Globalization;
using System.ComponentModel;
using Windows.UI.Core;
using System.Collections.Generic;

namespace Balboa.Common
{
    
    public class Statistic : INotifyPropertyChanged
    {
        private const string _modName = "Statistic.cs";
        #region

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
               // await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); });
            }
        }

        #endregion

        private MainPage _mainPage;

        private int _artists = 0;   // artists: number of artists
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

        private int _albums=0;
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

        private int _songs = 0;     // songs: number of songs
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

        private int _uptime = 0;    // uptime: daemon uptime in seconds
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

        private int _dbPlaytime = 0; // db_playtime: sum of all song times in the db
        public int DbPlaytime  // db_playtime: sum of all song times in the db
        {
            get { return _dbPlaytime; }
            private set
            {
                if (_dbPlaytime != value)
                {
                    _dbPlaytime = value;
                    NotifyPropertyChanged("DbPlaytime");
                }
            }
        }

        private int _dbUpdate = 0; // db_update: last db update in UNIX time
        public int DbUpdate  // db_update: last db update in UNIX time
        {
            get { return _dbUpdate; }
            private set
            {
                if (_dbUpdate != value)
                {
                    _dbUpdate = value;
                    NotifyPropertyChanged("DbUpdate");
                }
            }
        }

        private DateTime _dbUpdateDT; // last db update in Date Time
        public DateTime DbUpdateDT
        {
            get { return _dbUpdateDT; }
            private set
            {
                if (_dbUpdateDT != value)
                {
                    _dbUpdateDT = value;
                    NotifyPropertyChanged("DbUpdateDT");
                }
            }
        }

        private int _playtime = 0;  // playtime: time length of music played
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
        
        public Statistic(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public Statistic()
        {
            
        }
        //public void Update(MpdResponseCollection statsInfo)
        //{
        //    if (statsInfo == null)
        //        throw new BalboaNullValueException(_modName, "Update", "158", "statsinfo");


        //    foreach (string item in statsInfo)
        //    {
        //        string[] statsitem = item.Split(':');
        //        switch (statsitem[0])
        //        {
        //          case "artists": Artists = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //          case "albums": Albums = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //          case "songs": Songs = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //          case "uptime": Uptime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //          case "db_playtime": DbPlaytime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //          case "db_update":
        //                  DbUpdate = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
        //                    // Unix timestamp is seconds past epoch
        //                  DateTime db_update_dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //                  DbUpdateDT = db_update_dt.AddSeconds(DbUpdate).ToLocalTime();
        //                  break;
        //          case "playtime": Playtime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
        //         }
        //     }
        //}

        public void Update(List<string> statsInfo)
        {
            if (statsInfo == null)
                throw new BalboaNullValueException(_modName, "Update", "158", "statsinfo");


            foreach (string item in statsInfo)
            {
                string[] statsitem = item.Split(':');
                switch (statsitem[0])
                {
                    case "artists": Artists = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "albums": Albums = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "songs": Songs = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "uptime": Uptime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "db_playtime": DbPlaytime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    case "db_update":
                        DbUpdate = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
                        // Unix timestamp is seconds past epoch
                        DateTime db_update_dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        DbUpdateDT = db_update_dt.AddSeconds(DbUpdate).ToLocalTime();
                        break;
                    case "playtime": Playtime = int.Parse(statsitem[1], NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                }
            }
        }
    }
}
