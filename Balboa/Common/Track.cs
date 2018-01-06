/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных одиночного treka
 *
  --------------------------------------------------------------------------*/

using System.ComponentModel;

namespace Balboa.Common
{
    public sealed class Track : INotifyPropertyChanged, IUpdatable
    {
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
             if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        

        private bool _isPlaying = false;
        private bool _isSelected = false;

        // Playlistitem properties
        public string File { get; set; }
        public string LastModified { get; set; }
        public float  Time { get; set; }
        public string Artist { get; set; }              // the artist name
        public string Title { get; set; }               // the song title.
        public string Album { get; set; }               // the album name
        public string Date { get; set; }                // the song's release date. This is usually a 4-digit year.
        public string TrackNo { get; set; }             // the track number within the album.
        public string Genre { get; set; }               // the music genre.
        public string Composer { get; set; }            // the artist who composed the song. 
        public string AlbumArtist { get; set; }         // on multi-artist albums, this is the artist name which shall be used for the whole album.
        public string Disc { get; set; }                // the disc number in a multi-disc album.
        public string Pos { get; set; }
        public int    Id { get; set; }
        public bool   IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying!=value)
                {
                    _isPlaying = value;
                   NotifyPropertyChanged("IsPlaying");
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        // Database properties
        public string Artistsort { get; set; }          // same as artist, but for sorting.This usually omits prefixes such as "The". 
        public string Albumsort { get; set; }           // same as album, but for sorting.
        public string Albumartistsort { get; set; }     // same as albumartist, but for sorting.
        public string Name { get; set; }                // a name for this song.This is not the song title. The exact meaning of this tag is not well-defined.It is often used by badly configured internet radio stations with broken tags to squeeze both the artist name and the song title in one tag. 
        public string Performer { get; set; }           // the artist who performed the song. 
        public string comment { get; set; }             // a human-readable comment about this song.The exact meaning of this tag is not well-defined.

        public string musicbrainz_artistid { get; set; } //  the artist id in the MusicBrainz database.
        public string musicbrainz_albumid { get; set; }  //  the album id in the MusicBrainz database.
        public string musicbrainz_albumartistid { get; set; } //  the album artist id in the MusicBrainz database.
        public string musicbrainz_trackid { get; set; }  //  the track id in the MusicBrainz database.
        public string musicbrainz_releasetrackid { get; set; } //  the release track id in the MusicBrainz database.

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
                    case "file":            File = tagvalue; break;
                    case "last-modified":   LastModified = tagvalue; break;
                    case "time":            Time = float.Parse(tagvalue); break;
                    case "artist":          Artist = tagvalue; break;
                    case "title":           Title = tagvalue; break;
                    case "album":           Album = tagvalue; break;
                    case "date":            Date = tagvalue; break;
                    case "track":           TrackNo = tagvalue; break;
                    case "genre":           Genre = tagvalue; break;
                    case "composer":        Composer = tagvalue; break;
                    case "albumartist":     AlbumArtist = tagvalue; break;
                    case "disc":            Disc = tagvalue; break;
                    case "pos":             Pos = tagvalue; break;
                    case "id":              Id = int.Parse(tagvalue); break;
                    // Database properties
                    case "artistsort":      Artistsort = tagvalue; break;         // same as artist, but for sorting.This usually omits prefixes such as "The". 
                    case "albumsort":       Albumsort = tagvalue; break;          // same as album, but for sorting.
                    case "albumartistsort": Albumartistsort = tagvalue; break;    // same as albumartist, but for sorting.
                    case "name":            Name = tagvalue; break;               // a name for this song.This is not the song title. The exact meaning of this tag is not well-defined.It is often used by badly configured internet radio stations with broken tags to squeeze both the artist name and the song title in one tag. 
                    case "performer":       Performer = tagvalue; break;          // the artist who performed the song. 
                    case "comment":         comment = tagvalue; break;            // a human-readable comment about this song.The exact meaning of this tag is not well-defined.

                    case "musicbrainz_artistid": musicbrainz_artistid = tagvalue; break;             //  the artist id in the MusicBrainz database.
                    case "musicbrainz_albumid": musicbrainz_albumid = tagvalue; break;               //  the album id in the MusicBrainz database.
                    case "musicbrainz_albumartistid": musicbrainz_albumartistid = tagvalue; break;   //  the album artist id in the MusicBrainz database.
                    case "musicbrainz_trackid": musicbrainz_trackid = tagvalue; break;               //  the track id in the MusicBrainz database.
                    case "musicbrainz_releasetrackid": musicbrainz_releasetrackid = tagvalue; break; //  the release track id in the MusicBrainz database.
                }
                // Заполним пустые параметры значениями по умолчанию
                 if (Title == null) Title = Utils.ExtractFileName(File??"", true);
                if (Artist == null) Artist = " Unknown artist";
                if (Album == null) Album = " Unknown album";
                if (Date == null) Date = " Unknown year";
                i++;
            }
            while ((i < responce.Count) && (!responce[i].StartsWith("file")));
            responce.RemoveRange(0, i);
        }
    }
}
