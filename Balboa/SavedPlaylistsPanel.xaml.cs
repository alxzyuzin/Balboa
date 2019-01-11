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
    public sealed partial class SavedPlaylistsPanel : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler ActionRequested;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private ObservableCollection<CommonGridItem> _items = new ObservableCollection<CommonGridItem>();
        public ObservableCollection<CommonGridItem> Items => _items;

        //_currentPlaylistName = _resldr.GetString("NewPlaylist");

        public SavedPlaylistsPanel()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public void Init(Server server)
        {
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
                    //_foundTracks.NotifyCollectionChanged();
                }
            }
        }

        private async void appbtn_SavedPlaylistLoad_Click(object sender, RoutedEventArgs e)
        {
            //File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            //if (fi == null)
            //{
            //    await MessageBox(_resldr.GetString("PlaylistLoading"),
            //                     _resldr.GetString("PlaylisNotSelectedForLoad"), MsgBoxButtons.Continue);
            //}
            //else
            //{
            //    //_currentPlaylistName = fi.Name;
            //    _server.Load(fi.Name);
            //}
        }

        private async void appbtn_SavedPlaylistDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            //if (fi == null)
            //{
            //    await MessageBox(_resldr.GetString("PlaylistDeleting"),
            //                     _resldr.GetString("PlaylistNotSelectedForDelete"), MsgBoxButtons.Continue);
            //}
            //else
            //{
            //    _server.Rm(fi.Name);
            //    _server.ListPlaylists();
            //}
        }

        private async void appbtn_SavedPlaylistRename_Tapped(object sender, TappedRoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
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
            throw new NotImplementedException();
        }
    }
}
