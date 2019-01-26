using Balboa.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class SearchPanel : UserControl, INotifyPropertyChanged, IDataPanel,
                                                            IRequestAction, IDisposable

    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private TrackCollection<Track> _foundTracks = new TrackCollection<Track>();
        public ObservableCollection<Track> Tracks => _foundTracks;

        private string _searchResult;
        public string SearchResult
        {
            get { return _searchResult; }
            set
            {
                if(_searchResult!=value)
                {
                    _searchResult = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResult)));
                }
            }
        }

        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchString)));
                }
            }
        }

        public SearchPanel()
        {
            this.InitializeComponent();
        }

        public SearchPanel(Server server):this()
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            //throw new NotImplementedException();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "search")
                {
                    _foundTracks.Clear();
                    while (mpdData.Content.Count > 0)
                    {
                        var track = new Track();
                        track.Update(mpdData.Content);
                        _foundTracks.Add(track);
                    }
                    _foundTracks.NotifyCollectionChanged();

                    SearchResult = _foundTracks.Count == 0 ? _resldr.GetString("SearchComplete") : "";
                }
            }
        }

        private void appbtn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SearchString.Length > 0)
            {
                SearchResult = _resldr.GetString("Searching");
                _server.Search("any", SearchString);
            }
        }

        private void appbtn_Search_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_Search.SelectedItems.Count > 0)
            {
                foreach (Track track in listview_Search.SelectedItems)
                    _server.Add(track.File);
            }
            else
            {
                //await MessageBox(_resldr.GetString("AddingTrackToPlaylist"),
                //    _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
            }
        }

        private void appbtn_Search_SelectAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            listview_Search.SelectAll();
        }

        private void appbtn_Search_DeSelectAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            listview_Search.SelectedItems.Clear();
        }

        private void listview_Search_GotFocus(object sender, RoutedEventArgs e)
        {
            // _listViewItemGotFocus = e.OriginalSource as ListViewItem;
        }

        private bool playlastaddedtrack = false;
        private void listview_Search_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            //Track track = _listViewItemGotFocus.Content as Track;
            //_server.Add(track.File);
            playlastaddedtrack = true;
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
          //  throw new NotImplementedException();
        }

        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }
    }
}
