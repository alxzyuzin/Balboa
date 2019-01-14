using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class CurrentTrackPanel : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;
 
        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private Song _song = new Song();

        private AlbumArt _albumArt = new AlbumArt();
        public AlbumArt AlbumArt
        {
            get { return _albumArt; }
            private set
            {
                if (_albumArt != value)
                {
                    _albumArt = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumArt)));
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title!=value)
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Artist)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Album)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumArtist)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Composer)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Disc)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Track)));
                }
            }
        }

        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
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
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Genre)));
                }
            }
        }

        private string _file;
        public string File
        {
            get { return _file; }
            set
            {
                if (_file != value)
                {
                    _file = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(File)));
                }
            }
        }

        public CurrentTrackPanel()
        {
            this.InitializeComponent();
         }

        public CurrentTrackPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            _server = server;
            _server.DataReady += _server_DataReady;
            _server.CurrentSong();
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server?.CurrentSong();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "currentsong")
                   UpdateControlData(mpdData.Content);
            }
        }

        private async  void UpdateControlData(List<string> serverData)
        {
            _song.Update(serverData);
            Title       = _song.Title;
            Artist      = _song.Artist;
            Album       = _song.Album;
            Date        = _song.Date;
            AlbumArtist = _song.AlbumArtist;
            Composer    = _song.Composer;
            Disc        = _song.Disc;
            Track       = _song.Track;
            Duration    = _song.Duration.ToString();
            Genre       = _song.Genre;
            File        = _song.File;
            if (_song.File != null)
            {
               await AlbumArt.LoadImageData(_server.MusicCollectionFolder, _song.File, _server.AlbumCoverFileNames);
               await AlbumArt.UpdateImage();
            }
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }
    }
}
