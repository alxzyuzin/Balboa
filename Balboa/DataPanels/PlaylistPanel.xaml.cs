using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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

        private string _temporaryPlaylistName = string.Empty;
        public string TemporaryPlaylistName
        {
            get { return _temporaryPlaylistName; }
            set
            {
                if (_temporaryPlaylistName != value)
                {
                    _temporaryPlaylistName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TemporaryPlaylistName)));
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
            {
                _server.DeleteId(track.Id);
            }
            Update();
        }

        private void appbtn_Playlist_Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _playlist.Clear();
            _playlist.NotifyCollectionChanged();
            _server.Clear();
          
            //Update();
        }

        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel, Panels.TrackDirectoryPanel));
        }

        private void appbtn_Playlist_Shaffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Shuffle();
            Update();
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
            lv_PlayList.ScrollIntoView(track);
            lv_PlayList.SelectedItems.Clear();
            _playlist.NotifyCollectionChanged();
        }

        private void appbtn_Playlist_Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TemporaryPlaylistName = LoadedPlaylistName;
            RequestNewPlaylistName();
        }

        private popupLastPressedButton _popupLastPressedBatton;
        private void btn_PlaylistNameSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _popupLastPressedBatton = popupLastPressedButton.Save;
            popup_GetPlaylistName.IsOpen = false;
        }

        private void btn_PlaylistNameCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _popupLastPressedBatton = popupLastPressedButton.Cancel;
            popup_GetPlaylistName.IsOpen = false;
        }

        private void RequestNewPlaylistName()
        {
//            CoreWindow currentWindow = Window.Current.CoreWindow;
            popup_GetPlaylistName.VerticalOffset = -155;
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

        private enum popupLastPressedButton
        {
            Save,
            Cancel
        }
       
        private void popup_GetPlaylistName_Closed(object sender, object e)
        {
            if (_popupLastPressedBatton == popupLastPressedButton.Save)
            {
                LoadedPlaylistName = TemporaryPlaylistName;
                string str = _temporaryPlaylistName;
                Encoding encoding = Encoding.Unicode;
                byte[] encBytes = encoding.GetBytes(str);
                byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
                str = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length);
                _server.Save(str);
                PlaylistNameVisibility = _loadedPlaylistName.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
