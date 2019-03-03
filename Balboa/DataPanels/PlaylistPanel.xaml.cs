using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Linq;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class PlaylistPanel : UserControl, INotifyPropertyChanged,
                                                IDataPanel, IDataPanelInfo, IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private Server _server;
        private TrackCollection<Track> _playlist = new TrackCollection<Track>();
        private ListViewItem _listViewItemInFocus;
        private int _currentSongID = -1;

        public ObservableCollection<Track> Playlist => _playlist;

        private Visibility _emptyContentMessageVisibility = Visibility.Collapsed;
        public Visibility EmptyContentMessageVisibility
        {
            get { return _emptyContentMessageVisibility; }
            set
            {
                if (_emptyContentMessageVisibility != value)
                {
                    _emptyContentMessageVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmptyContentMessageVisibility)));
                }
                
            }
        }

        private string _loadedPlaylistName = string.Empty;
        public string LoadedPlaylistName
        {
            get { return _loadedPlaylistName; }
            set
            {
                if (_loadedPlaylistName != value)
                {
                    _loadedPlaylistName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadedPlaylistName)));
                }
            }
        }

        private string _dataPanelInfo;
        public string DataPanelInfo
        {
            get { return _dataPanelInfo; }
            set
            {
                if (_dataPanelInfo != value)
                {
                    _dataPanelInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataPanelInfo)));
                }
            }
        }

        private string _dataPanelElementsCount;
        public string DataPanelElementsCount
        {
            get { return _dataPanelElementsCount; }
            set
            {
                if (_dataPanelElementsCount != value)
                {
                    _dataPanelElementsCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataPanelElementsCount)));
                }
            }
        }

        public double TotalButtonWidth => AppBarButtons.ActualWidth;

        public Orientation Orientation { get; set; }

        public PlaylistPanel()
        {
            InitializeComponent();
        }

        public PlaylistPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            Init(server);

        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;

            Update();

            LoadedPlaylistName = _server.PlaylistName;
            if (_loadedPlaylistName != null)
                DataPanelInfo = "Playlist name: " + _server.PlaylistName;
        }

        public void Update()
        {
            _server?.PlaylistInfo();
            _server?.CurrentSong();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "playlistinfo")
                {
                    UpdateControlData(mpdData.Content);

                }
                if (mpdData.Command.Op == "currentsong")
                {
                    Song song = new Song();
                    song.Update(mpdData.Content);
                    if (_currentSongID != song.Id)
                    {
                        _currentSongID = song.Id;
                        HightlightCurrentPlaingTrack();
                    }
                }
                if (mpdData.Command.Op == "save")
                {
                    ;
                }
            }

            if (mpdData.Keyword != ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "save")
                {
                    ;
                }
            }
        }

        private void UpdateControlData(List<string> serverData)
        {
            _playlist.Clear();
            while (serverData.Count > 0)
            {
                var track = new Track();
                track.Update(serverData);
                _playlist.Add(track);
            }
            _playlist.NotifyCollectionChanged();
            EmptyContentMessageVisibility = _playlist.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            DataPanelElementsCount = $"{_playlist.Count.ToString()} items.";
            MakeOpaque.Begin();
        }

        private void lv_PlayList_GotFocus(object sender, RoutedEventArgs e)
        {
            _listViewItemInFocus = e.OriginalSource as ListViewItem;
        }

        private void lv_PlayList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_listViewItemInFocus != null)
            {
                Track playlistitem = _listViewItemInFocus.Content as Track;
                _server.PlayId(playlistitem.Id);
            }
        }
       
        private void appbtn_Playlist_DeleteSelected_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (Track track in lv_PlayList.SelectedItems)
                _server.DeleteId(track.Id);
            _currentSongID = -1;
            Update();
        }

        private void appbtn_Playlist_Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _playlist.Clear();
            _playlist.NotifyCollectionChanged();
             _server.Clear();
            LoadedPlaylistName = _server.PlaylistName = string.Empty;
            EmptyContentMessageVisibility = Visibility.Visible;
        }

        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel).SetPanel<TrackDirectoryPanel>(new TrackDirectoryPanel(_server)));
        }

        private void appbtn_Playlist_Shaffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Shuffle();
            _server.PlaylistInfo();
            _currentSongID = -1;
            _server.CurrentSong();
        }

        private void HightlightCurrentPlaingTrack()
        {
            Track track;
            track =  lv_PlayList.Items.FirstOrDefault(item => (item as Track).IsPlaying) as Track;
            if (track!= null)
                track.IsPlaying = false;
            track = lv_PlayList.Items.FirstOrDefault(item => (item as Track).Id == _currentSongID) as Track;
            if (track != null)
                track.IsPlaying = true;
        
            _playlist.NotifyCollectionChanged();
            lv_PlayList.ScrollIntoView(track);
        }

        private void appbtn_Playlist_Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (popup_GetPlaylistName.IsOpen)
            {
                popup_GetPlaylistName.IsOpen = false;
                return;
            }
            var playlistNameInput = new PlaylistNameInput(LoadedPlaylistName);

            
            playlistNameInput.PropertyChanged += (object snd, PropertyChangedEventArgs args) =>
            {
                if (args.PropertyName == "PlaylistName")
                {
                    if (playlistNameInput.PlaylistName.Length > 0)
                    {
                        if (LoadedPlaylistName == playlistNameInput.PlaylistName)
                            _server.Rm(LoadedPlaylistName);
                        _server.PlaylistName = LoadedPlaylistName = playlistNameInput.PlaylistName;
                        _server.Save(playlistNameInput.PlaylistNameUTF8);
                        DataPanelInfo = "Playlist name: " + _server.PlaylistName;
                    }
                }
            };
            popup_GetPlaylistName.Child = playlistNameInput;
            popup_GetPlaylistName.IsOpen = true;
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }
    } // class PlaylistPanel
}
