/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Хранит данные текущего воспроизводимого трека.
 *
 --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Core;

namespace Balboa.Common
{
    public sealed class Song
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
 
        public string File { get; private set; }
        public string LastModified { get; private set; }
        public float Duration { get; private set; }
        public string Artist { get; private set; }
        public string Title { get; private set; }
        public string Album { get; private set; }
        public string Date { get; private set; }
        public string Track { get; private set; }
        public string Genre { get; private set; }
        public string Composer { get; private set; }
        public string AlbumArtist { get; private set; }
        public string Disc { get; private set; }
        public string Position { get; private set; }
        public int    Id { get; private set; }

        public Song()
        {
        
        }

        public Song(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        public void Update(MpdResponseCollection currentSongInfo)
        {
        //    Parse(currentSongInfo);
        }


        //private void Parse(MpdResponseCollection currentsonginfo)
        //{

        //    // Заполним пустые параметры значениями по умолчанию
        //    Title  = string.Empty;
        //    Artist = string.Empty;
        //    Album  = string.Empty;
        //    Date   = string.Empty;

        //    foreach (string item in currentsonginfo)
        //    {
        //        string tagvalue=string.Empty;
        //        string[] items = item.Split(':');
        //        if (items.Length > 1)
        //            tagvalue = items[1].Trim();
                
        //        switch (items[0])
        //        {
        //            case "file":          File = tagvalue; break;
        //            case "Last-Modified": LastModified = tagvalue; break;
        //            case "Time":          Duration = float.Parse(tagvalue,System.Globalization.CultureInfo.InvariantCulture); break;
        //            case "Artist":        Artist = tagvalue; break;
        //            case "Title":         Title = tagvalue; break;
        //            case "Album":         Album = tagvalue; break;
        //            case "Date":          Date = tagvalue; break;
        //            case "Track":         Track = tagvalue; break;
        //            case "Genre":         Genre = tagvalue; break;
        //            case "Composer":      Composer = tagvalue; break;
        //            case "AlbumArtist":   AlbumArtist = tagvalue; break;
        //            case "Disc":          Disc = tagvalue; break;
        //            case "Pos":           Position = tagvalue; break;
        //            case "Id":            Id = int.Parse(tagvalue, System.Globalization.CultureInfo.InvariantCulture); break;
        //        }
        //    }

        //    // Заполним пустые параметры значениями по умолчанию
        //    if (Title.Length == 0) Title = Utilities.ExtractFileName(File ?? "", true);
        //    if (Artist.Length == 0) Artist = " Unknown artist";
        //    if (Album.Length == 0) Album = " Unknown album";
        //    if (Date.Length == 0) Date = " Unknown year";
        //}

        public void Update(List<string> response)
        {
            // Заполним пустые параметры значениями по умолчанию
            Title = string.Empty;
            Artist = string.Empty;
            Album = string.Empty;
            Date = string.Empty;

            foreach (string item in response)
            {
                string tagvalue = string.Empty;
                string[] items = item.Split(':');
                if (items.Length > 1)
                    tagvalue = items[1].Trim();

                switch (items[0])
                {
                    case "file": File = tagvalue; break;
                    case "Last-Modified": LastModified = tagvalue; break;
                    case "Time": Duration = float.Parse(tagvalue, System.Globalization.CultureInfo.InvariantCulture); break;
                    case "Artist": Artist = tagvalue; break;
                    case "Title": Title = tagvalue; break;
                    case "Album": Album = tagvalue; break;
                    case "Date": Date = tagvalue; break;
                    case "Track": Track = tagvalue; break;
                    case "Genre": Genre = tagvalue; break;
                    case "Composer": Composer = tagvalue; break;
                    case "AlbumArtist": AlbumArtist = tagvalue; break;
                    case "Disc": Disc = tagvalue; break;
                    case "Pos": Position = tagvalue; break;
                    case "Id": Id = int.Parse(tagvalue, System.Globalization.CultureInfo.InvariantCulture); break;
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
