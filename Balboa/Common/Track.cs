/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс для хранения данных одиночного treka
 *
  --------------------------------------------------------------------------*/
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace Balboa.Common
{
    public sealed class Track : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        public string Position { get; set; }
        public int    Id { get; set; }

        private bool _isPlaying = false;
        public bool   IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying!=value)
                {
                    _isPlaying = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
                }
            }
        }

        private bool _isSelected = false;
        public bool  IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        // Database properties

        public string ArtistSort { get; set; }          // same as artist, but for sorting.This usually omits prefixes such as "The". 
        public string AlbumSort { get; set; }           // same as album, but for sorting.
        public string AlbumArtistSort { get; set; }     // same as albumartist, but for sorting.
        public string Name { get; set; }                // a name for this song.This is not the song title. The exact meaning of this tag is not well-defined.It is often used by badly configured internet radio stations with broken tags to squeeze both the artist name and the song title in one tag. 
        public string Performer { get; set; }           // the artist who performed the song. 
        public string Comment { get; set; }             // a human-readable comment about this song.The exact meaning of this tag is not well-defined.
        public string MusicBrainzArtistId { get; set; } //  the artist id in the MusicBrainz database.
        public string MusicBrainzAlbumId { get; set; }  //  the album id in the MusicBrainz database.
        public string MusicBrainzAlbumArtistId { get; set; } //  the album artist id in the MusicBrainz database.
        public string MusicBrainzTrackId { get; set; }  //  the track id in the MusicBrainz database.
        public string MusicBrainzReleaseTrackId { get; set; } //  the release track id in the MusicBrainz database.

        public void Update(List<string> response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            int i = 0;
            do
            {
                string[] items = response[i].Split(':');
                string tagname = items[0].ToLower();
                string tagvalue = items[1].Trim();
                switch (tagname)
                {
                    case "file": File = tagvalue; break;
                    case "last-modified": LastModified = tagvalue; break;
                    case "time": Time = float.Parse(tagvalue, NumberStyles.Float, CultureInfo.InvariantCulture); break;
                    case "artist": Artist = tagvalue; break;
                    case "title": Title = tagvalue; break;
                    case "album": Album = tagvalue; break;
                    case "date": Date = tagvalue; break;
                    case "track": TrackNo = tagvalue; break;
                    case "genre": Genre = tagvalue; break;
                    case "composer": Composer = tagvalue; break;
                    case "albumartist": AlbumArtist = tagvalue; break;
                    case "disc": Disc = tagvalue; break;
                    case "pos": Position = tagvalue; break;
                    case "id": Id = int.Parse(tagvalue, NumberStyles.Integer, CultureInfo.InvariantCulture); break;
                    // Database properties
                    case "artistsort": ArtistSort = tagvalue; break;         // same as artist, but for sorting.This usually omits prefixes such as "The". 
                    case "albumsort": AlbumSort = tagvalue; break;          // same as album, but for sorting.
                    case "albumartistsort": AlbumArtistSort = tagvalue; break;    // same as albumartist, but for sorting.
                    case "name": Name = tagvalue; break;               // a name for this song.This is not the song title. The exact meaning of this tag is not well-defined.It is often used by badly configured internet radio stations with broken tags to squeeze both the artist name and the song title in one tag. 
                    case "performer": Performer = tagvalue; break;          // the artist who performed the song. 
                    case "comment": Comment = tagvalue; break;            // a human-readable comment about this song.The exact meaning of this tag is not well-defined.

                    case "musicbrainz_artistid": MusicBrainzArtistId = tagvalue; break;             //  the artist id in the MusicBrainz database.
                    case "musicbrainz_albumid": MusicBrainzAlbumId = tagvalue; break;               //  the album id in the MusicBrainz database.
                    case "musicbrainz_albumartistid": MusicBrainzAlbumArtistId = tagvalue; break;   //  the album artist id in the MusicBrainz database.
                    case "musicbrainz_trackid": MusicBrainzTrackId = tagvalue; break;               //  the track id in the MusicBrainz database.
                    case "musicbrainz_releasetrackid": MusicBrainzReleaseTrackId = tagvalue; break; //  the release track id in the MusicBrainz database.
                }
                // Заполним пустые параметры значениями по умолчанию
                if (Title == null) Title = Utilities.ExtractFileName(File ?? "", true);
                if (Artist == null) Artist = " Unknown artist";
                if (Album == null) Album = " Unknown album";
                if (Date == null) Date = " Unknown year";
                i++;
            }
            while ((i < response.Count) && (!response[i].StartsWith("file", System.StringComparison.Ordinal)));

            response.RemoveRange(0, i);
        }
    }
}
