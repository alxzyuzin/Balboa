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
    public sealed partial class SearchPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDataPanelInfo,
                                                            IRequestAction, IDisposable

    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private TrackCollection<Track> _foundTracks = new TrackCollection<Track>();
        public ObservableCollection<Track> Tracks => _foundTracks;

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

        private ComboBoxItem _whereToSearch;
        public ComboBoxItem WhereToSearch
        {
            get { return _whereToSearch; }
            set
            {
                if (_whereToSearch != value)
                {
                    _whereToSearch = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WhereToSearch)));
                }
            }
        }

        private string _whatToSearch = string.Empty;
        public string WhatToSearch
        {
            get { return _whatToSearch; }
            set
            {
                if (_whatToSearch != value)
                {
                    _whatToSearch = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WhatToSearch)));
                }
            }
        }

        public Orientation Orientation { get; set; }


        public SearchPanel()
        {
            this.InitializeComponent();
            _whereToSearch = new ComboBoxItem();
            _whereToSearch.Content = "Any";
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
                    ;
                    while (mpdData.Content.Count > 0)
                    {
                        var track = new Track();
                        track.Update(mpdData.Content);
                        _foundTracks.Add(track);
                    }
                    _foundTracks.NotifyCollectionChanged();
                    string searchresult = _foundTracks.Count == 0 ? "Nothing found." : $"{ _foundTracks.Count} tracks found.";
                    DataPanelElementsCount = $"SearchComplete. {searchresult}";

                }
            }


        }

        private void appbtn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _foundTracks.Clear();
            _foundTracks.NotifyCollectionChanged();
            DataPanelElementsCount = "Searching ...";
            string wts = WhereToSearch.Content.ToString();
            wts = wts == "Year" ? "Date" : wts;

            if (WhatToSearch.Length > 0)
                _server.Search(wts, WhatToSearch);
            else
                _server.Search(wts, "");
        }

        private void appbtn_Search_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (gridview_Search.SelectedItems.Count == 0)
            {
                var message = new Message(MsgBoxType.Info, _resldr.GetString("NoSelectedItemsToAdd"),
                                           MsgBoxButton.Continue, 150);
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }

            foreach (Track track in gridview_Search.SelectedItems)
                    _server.Add(track.File);

            gridview_Search.SelectedItems.Clear();
            RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel).SetPanel(new PlaylistPanel(_server)));

        }

        private void appbtn_Search_SelectAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            gridview_Search.SelectAll();
        }

        private void appbtn_Search_DeSelectAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            gridview_Search.SelectedItems.Clear();
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

        private void SearchItemMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {

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
