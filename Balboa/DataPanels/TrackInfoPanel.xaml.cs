using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    [Flags]
    public enum PanelVisualState
    {
        VerticalCollapsed = 1,
        HorizontalCollapsed = 2,
        Transparent = 4
    }

    public sealed partial class TrackInfoPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Server _server;
        private Status _status = new Status();
        private Song _song = new Song();
 
        public Orientation Orientation { get; set; }
        public PanelVisualState VisualState { get; set; }     

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
            if (Orientation == Orientation.Vertical)
            {
                AlbumArtImage.Width = 190;
                VerticalExpand.Begin();
            }
            else
            {
                AlbumArtImage.Width = 90;
                HorizontalExpand.Begin();
            }
                
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
