using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Balboa.Common;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class SavedPlaylistsPanel : UserControl, INotifyPropertyChanged, IDataPanel,
                                                                   IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private ObservableCollection<SavedPlaylistItem> _items = new ObservableCollection<SavedPlaylistItem>();
        public ObservableCollection<SavedPlaylistItem> Items => _items;

        private Visibility _savedPlaylistsContentVisibility = Visibility.Collapsed;
        public Visibility SavedPlaylistsContentVisibility
        {
            get{ return _savedPlaylistsContentVisibility; }
            set
            {
                if (_savedPlaylistsContentVisibility != value)
                {
                    _savedPlaylistsContentVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SavedPlaylistsContentVisibility)));
                }
            }
        }

        public SavedPlaylistsPanel()
        {
            this.InitializeComponent();
        }


        public SavedPlaylistsPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;

            _server.ListPlaylists();
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server.ListPlaylists();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "listplaylists")
                {
                    _items.Clear();
                    while (mpdData.Content.Count > 0)
                    {
                        var savedPlaylistItem = new SavedPlaylistItem();
                        savedPlaylistItem.Update(mpdData.Content);
                        _items.Add(savedPlaylistItem);
                    }
                }
            }
            SavedPlaylistsContentVisibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void appbtn_SavedPlaylistLoad_Click(object sender, RoutedEventArgs e)
        {
            if (gr_SavedPlaylists.SelectedItems.Count == 0)
            {
                var message = new Message(MsgBoxType.Info, _resldr.GetString("PlaylisNotSelectedForLoad"), 
                                          MsgBoxButton.Continue, 150);
                RequestAction?.Invoke(this, new ActionParams(message));
            }
            else
            {
                _server.Clear();
                _server.PlaylistName = (gr_SavedPlaylists.SelectedItem as SavedPlaylistItem).FileName;
                _server.Load(_server.PlaylistName);
                RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel).SetPanel<PlaylistPanel>(new PlaylistPanel(_server)));
            }
        }

        private void appbtn_SavedPlaylistDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {

            if (gr_SavedPlaylists.SelectedItems.Count == 0)
            {
                var message = new Message(MsgBoxType.Info, _resldr.GetString("PlaylistNotSelectedForDelete"),
                                          MsgBoxButton.Continue, 150);
                RequestAction?.Invoke(this, new ActionParams(message));
            }
            else
            {
                _server.Rm((gr_SavedPlaylists.SelectedItem as CommonGridItem).Name);
                _server.ListPlaylists();
            }
        }

        private void appbtn_SavedPlaylistRename_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (popup_GetPlaylistName.IsOpen)
                return;

            if (gr_SavedPlaylists.SelectedItems.Count == 0)
            {
                var message = new Message(MsgBoxType.Info, _resldr.GetString("PlaylistNotSelectedForRename"),
                                          MsgBoxButton.Continue, 150);
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }

            var playlistNameInput = new PlaylistNameInput((gr_SavedPlaylists.SelectedItem as CommonGridItem).Name);
            playlistNameInput.PropertyChanged += (object snd, PropertyChangedEventArgs args) =>
            {
                if (args.PropertyName == "PlaylistName")
                {
                    _server.Rename((gr_SavedPlaylists.SelectedItem as CommonGridItem).Name, playlistNameInput.PlaylistNameUTF8);
                    _server.ListPlaylists();
                }
            };
            popup_GetPlaylistName.Child = playlistNameInput;
            popup_GetPlaylistName.IsOpen = true;
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }
    }

 
}
