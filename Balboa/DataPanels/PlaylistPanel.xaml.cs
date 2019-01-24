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
                                                IDataPanel, IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private Server _server;
        private TrackCollection<Track> _playlist = new TrackCollection<Track>();
        private ListViewItem _listViewItemInFocus;
        private int _currentSongID = -1;

        public ObservableCollection<Track> Playlist => _playlist;

        private Visibility _playListContentVisibility = Visibility.Collapsed;
        public Visibility PlaylistContentVisibility
        {
            get { return _playListContentVisibility; }
            set
            {
                if (_playListContentVisibility != value)
                {
                    _playListContentVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistContentVisibility)));
                }
                
            }
        }

        public Visibility _playlistNameVisibility = Visibility.Collapsed;
        public Visibility PlaylistNameVisibility
        {
            get { return _playlistNameVisibility; }
            set
            {
                if (_playlistNameVisibility != value)
                {
                    _playlistNameVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistNameVisibility)));
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

                    PlaylistNameVisibility = _loadedPlaylistName?.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public PlaylistPanel()
        {
            InitializeComponent();
        }

        public PlaylistPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;

            _server.PlaylistInfo();
            _server.CurrentSong();

            LoadedPlaylistName = _server.PlaylistName;
            if (_loadedPlaylistName!=null)
                PlaylistNameVisibility = _loadedPlaylistName.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
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

            PlaylistContentVisibility = _playlist.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
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
            Update();
        }

        private void appbtn_Playlist_Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _playlist.Clear();
            _playlist.NotifyCollectionChanged();
             _server.Clear();
            LoadedPlaylistName = _server.PlaylistName = string.Empty;
        }

        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel, Panels.TrackDirectoryPanel));
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
        
//            lv_PlayList.SelectedItems.Clear();
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
                        _server.PlaylistName = LoadedPlaylistName = playlistNameInput.PlaylistName;
                        _server.Save(playlistNameInput.PlaylistNameUTF8);
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
