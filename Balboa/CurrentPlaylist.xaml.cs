using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Linq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class CurrentPlaylist : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Server _server;
        private TrackCollection<Track> _playlist = new TrackCollection<Track>();
        private ListViewItem _listViewItemInFocus;
        private int _currentSongID = -1;

        public ObservableCollection<Track> Playlist => _playlist;


        

        private enum NewPlaylistNameRequestMode { SaveNewPlaylist, RenamePlaylist };
        NewPlaylistNameRequestMode _requestNewPlaylistNameMode;
        private string _currentPlaylistName = string.Empty;
        private string _oldPlaylistName;
        private string _newPlaylistName;

        private Message _message;
        public Message Message
        {
            get { return _message; }
            private set
            {
                if (_message != value)
                {
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }

        private ControlAction _action;
        public ControlAction Action
        {
            get { return _action; }
            private set
            {
//                if (_action != value)
//                {
                    _action = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Action)));
//                }
            }
        }
        public CurrentPlaylist()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server.PlaylistInfo();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            { 
                if (mpdData.Command.Op == "playlistinfo")
                    UpdateControlData(mpdData.Content);
                if (mpdData.Command.Op == "currentsong")
                    HightlightCurrentPlaingTrack(mpdData.Content);
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
            _server.Clear();
            _playlist.Clear();
            Update();
        }

        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Action = ControlAction.SwitchToTrackDirectory;
        }

        private void appbtn_Playlist_Shaffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Shuffle();
            Update();
        }

        private void HightlightCurrentPlaingTrack(List<string> serverData)
        {
            Song song = new Song();
            song.Update(serverData);

            if (_currentSongID != song.Id)
            {
                Track track;
                track =  lv_PlayList.Items.FirstOrDefault(item => (item as Track).IsPlaying) as Track;
                if (track!= null)
                    track.IsPlaying = false;
                track = lv_PlayList.Items.FirstOrDefault(item => (item as Track).Id == song.Id) as Track;
                if (track != null)
                    track.IsPlaying = true;
                lv_PlayList.ScrollIntoView(track);
                _playlist.NotifyCollectionChanged();
                //{
                //    //if (item.Id == _server.CurrentSongData.Id)
                //    //{
                //    //    item.IsPlaying = true;
                //    //    lv_PlayList.ScrollIntoView(item);
                //    //    if (lv_PlayList.SelectedItems.IndexOf(item) >= 0)
                //    //        lv_PlayList.SelectedItems.Remove(item);
                //    //}
                //    //else
                //    //{
                //    //    item.IsPlaying = false;
                //    //}
                //}
            }
        }


        private void appbtn_Playlist_Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.SaveNewPlaylist;
            tbx_PlaylistName.Text = _currentPlaylistName;
            RequestNewPlaylistName();
        }

        private void btn_PlaylistNameSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_requestNewPlaylistNameMode == NewPlaylistNameRequestMode.SaveNewPlaylist)
            {
                _currentPlaylistName = tbx_PlaylistName.Text;
                tbk_PlaylistName.Text = _currentPlaylistName;

                string str = tbk_PlaylistName.Text;

                Encoding encoding = Encoding.Unicode;
                byte[] encBytes = encoding.GetBytes(str);
                byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);

                str = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length);

                _server.Save(str);
            }

            if (_requestNewPlaylistNameMode == NewPlaylistNameRequestMode.RenamePlaylist)
            {
                _newPlaylistName = tbx_PlaylistName.Text;
                _server.Rename(_oldPlaylistName, _newPlaylistName);
            }

            _server.ListPlaylists();
            popup_GetPlaylistName.IsOpen = false;
        }

        private void btn_PlaylistNameCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            popup_GetPlaylistName.IsOpen = false;
        }


 

        private void RequestNewPlaylistName()
        {
            CoreWindow currentWindow = Window.Current.CoreWindow;
            popup_GetPlaylistName.HorizontalOffset = (currentWindow.Bounds.Width / 2) - (400 / 2);
            popup_GetPlaylistName.VerticalOffset = (currentWindow.Bounds.Height / 2) - (150 / 2);

            Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition popuptransition = new Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition();
            popup_GetPlaylistName.ChildTransitions.Add(popuptransition);
            popup_GetPlaylistName.IsOpen = true;
        }
    }
}
