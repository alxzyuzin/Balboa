/*-----------------------------------------------------------------------
 * Copyright 2019 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Хранит данные текущего воспроизводимого трека.
 *
 --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Balboa.Common
{
    public sealed class Song
    {
        public string File { get; private set; }
        public string LastModified { get; private set; }
        public float  Duration { get; private set; }
        public string Artist { get; private set; } = " Unknown artist";
        public string Title { get; private set; }
        public string Album { get; private set; } = " Unknown album";
        public string Date { get; private set; }  = " Unknown year";
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

        public void Update(List<string> response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            Clear();

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
                    case "Artist": Artist = tagvalue.Length > 0 ? tagvalue: " Unknown artist"; break;
                    case "Title": Title = tagvalue.Length>0 ? tagvalue : Utilities.ExtractFileName(File ?? "", true); break;
                    case "Album": Album = tagvalue.Length > 0 ? tagvalue : " Unknown album"; break;
                    case "Date": Date = tagvalue.Length > 0 ? tagvalue : " Unknown year"; break;
                    case "Track": Track = tagvalue; break;
                    case "Genre": Genre = tagvalue; break;
                    case "Composer": Composer = tagvalue; break;
                    case "AlbumArtist": AlbumArtist = tagvalue; break;
                    case "Disc": Disc = tagvalue; break;
                    case "Pos": Position = tagvalue; break;
                    case "Id": Id = int.Parse(tagvalue, System.Globalization.CultureInfo.InvariantCulture); break;
                }
            }
        }

        private void Clear()
        {
            File = string.Empty;
            LastModified = string.Empty;
            Duration = 0;
            Artist = " Unknown artist";
            Title = string.Empty;
            Album = " Unknown album"; 
            Date = " Unknown year";
            Track = string.Empty;
            Genre = string.Empty;
            Composer = string.Empty;
            AlbumArtist = string.Empty;
            Disc = string.Empty;
            Position = string.Empty;
            Id = -1;

        }
    }
}
