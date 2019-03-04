using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    [Flags]
    public enum PanelVisualState
    {
        Normal = 0,
        VerticalCollapsed = 1,
        HorizontalCollapsed = 2,
        Transparent = 4
    }

    public sealed partial class TrackInfoPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Server _server;
        private int _songId = -1;
        private Song _song = new Song();

        private Orientation _orientation; 
        public Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Orientation)));
                }
            }
        }
        public PanelVisualState VisualState { get; set; }     

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

        private double _imageWidth = 90;
        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (_imageWidth != value)
                {
                    _imageWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageWidth)));
                }
            }
        }

        public TrackInfoPanel()
        {
            this.InitializeComponent();
        }

        public TrackInfoPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            Init(server);
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
                if (mpdData.Command.Op == "currentsong")
                    UpdateSongData(mpdData.Content);
            }
        }

        private async void UpdateSongData(List<string> serverData)
        {
            _song.Update(serverData);
            if (_songId != _song.Id)
            {
                _songId = _song.Id;
                Title = _song.Title;
                Artist = _song.Artist;
                if (_song.File == null || _song.File == "")
                    return;
                await AlbumArt.LoadImageData(_server.MusicCollectionFolder, _song.File, _server.AlbumCoverFileNames);
                await AlbumArt.UpdateImage();
            }
        }
        public void Collapse()
        {
            if (Orientation == Orientation.Vertical && !VisualState.HasFlag(PanelVisualState.VerticalCollapsed))
            {
                VisualState |= PanelVisualState.VerticalCollapsed;
                VerticalCollapse.Begin();
            }
            if (Orientation == Orientation.Horizontal && !VisualState.HasFlag(PanelVisualState.HorizontalCollapsed))
            {
                VisualState |= PanelVisualState.HorizontalCollapsed;
                HorizontalCollapse.Begin();
            }
        }

        public void Expand()
        {
            if (Orientation == Orientation.Vertical && VisualState.HasFlag(PanelVisualState.VerticalCollapsed))
            {
                VisualState ^= PanelVisualState.VerticalCollapsed;
                VerticalExpand.Begin();
            }
            if (Orientation == Orientation.Horizontal && VisualState.HasFlag(PanelVisualState.HorizontalCollapsed))
            {
                VisualState ^= PanelVisualState.HorizontalCollapsed;
                HorizontalExpand.Begin();
            }
        }

        public void Hide()
        {
            if (VisualState.HasFlag(PanelVisualState.Transparent))
                return;
            VisualState |= PanelVisualState.Transparent;
            MakeTransparent.Begin();
        }

        public void Show()
        {
            if (!VisualState.HasFlag(PanelVisualState.Transparent))
                return;
            VisualState ^= PanelVisualState.Transparent;
            MakeOpaque.Begin();
        }

        public void Dispose()
        {
            _server.DataReady -= ServerDataReady;
        }
    }
}
