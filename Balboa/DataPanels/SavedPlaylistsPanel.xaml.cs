using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Balboa.Common;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;

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
        private ObservableCollection<CommonGridItem> _items = new ObservableCollection<CommonGridItem>();
        public ObservableCollection<CommonGridItem> Items => _items;

        //_currentPlaylistName = _resldr.GetString("NewPlaylist");

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
                        var commonGridItem = new CommonGridItem();
                        commonGridItem.Update(mpdData.Content);
                        _items.Add(commonGridItem);
                    }
                }
            }
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
                _server.PlaylistName = (gr_SavedPlaylists.SelectedItem as CommonGridItem).Name;
                _server.Load(_server.PlaylistName);
                RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel, Panels.PlaylistPanel));
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
            File fi = gr_SavedPlaylists.SelectedItem as File;
            //if (fi == null)
            //{
            //    await MessageBox(_resldr.GetString("PlaylistRename"),
            //                     _resldr.GetString("PlaylistNotSelectedForRename"), MsgBoxButtons.Continue);
            //}
            //else
            //{
            //    //    _oldPlaylistName = fi.Name;
            //    //    tbx_PlaylistName.Text = _oldPlaylistName;
            //    //    _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.RenamePlaylist;
            //    //    RequestNewPlaylistName();
            //}
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
