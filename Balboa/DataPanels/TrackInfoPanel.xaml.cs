using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class TrackInfoPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Server _server;
        private Status _status = new Status();
        private Song _song = new Song();
        //        private int _currentSongID = -1;

        public Orientation Orientation { get; set; }
        //private Orientation _orientation = Orientation.Vertical;
        //public Orientation Orientation
        //{
        //    get { return _orientation; }
        //    private set
        //    {
        //        if (_orientation != value)
        //        {
        //            _orientation = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Orientation)));

        //        }
        //    }
        //}

        private Visibility _panelVisibility = Visibility.Visible;
        public Visibility PanelVisibility
        {
            get { return _panelVisibility; }
            private set
            {
                if (_panelVisibility != value)
                {
                    _panelVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PanelVisibility)));
                }
            }
        }

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
            private set
            {
                if (_title != value)
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
            private set
            {
                if (_artist != value)
                {
                    _artist = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Artist)));
                }
            }
        }


        public TrackInfoPanel()
        {
            this.InitializeComponent();
            InnerStackPanel.Orientation = this.Orientation == Orientation.Vertical ?
                                        Orientation.Horizontal : Orientation.Vertical;

        }

        public TrackInfoPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += ServerDataReady;
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += ServerDataReady;
        }

        public void Update()
        {
            _server.CurrentSong();
        }

        private void ServerDataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;

            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "status")
                    UpdateStatusData(mpdData.Content);
                if (mpdData.Command.Op == "currentsong")
                    UpdateSongData(mpdData.Content);
            }
        }

        private void UpdateStatusData(List<string> serverData)
        {
            _status.Update(serverData);
            PanelVisibility= (_status.State == "play") ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void UpdateSongData(List<string> serverData)
        {
            _song.Update(serverData);

            Title = _song.Title;
            Artist = _song.Artist;
            if (_song.File != null)
            {
                await AlbumArt.LoadImageData(_server.MusicCollectionFolder, _song.File, _server.AlbumCoverFileNames);
                await AlbumArt.UpdateImage();
            }
        }

        public void SetLayout(DataPanelLayout layout)
        {
          //  VisualStateManager.GoToState(this, layout.ToString(), true);
        }

        public void Dispose()
        {
            _server.DataReady -= ServerDataReady;
        }

        
    }
}
