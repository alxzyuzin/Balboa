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
    public sealed partial class DatabaseExplorerPanel : UserControl, IDataPanel, INotifyPropertyChanged,
                                                                     IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private ObservableCollection<CommonGridItem> _foundItems = new ObservableCollection<CommonGridItem>();
        public  ObservableCollection<CommonGridItem> Items => _foundItems;

        public string DataPanelInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string DataPanelElementsCount
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DatabaseExplorerPanel()
        {
            this.InitializeComponent();
        }

        public DatabaseExplorerPanel(Server server):this()
        {
            _server = server;
            _server.DataReady += _server_DataReady;
            _server.List("Album");
        }


        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server.List("Album");
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "list")
                {
                    _foundItems.Clear();
                    while (mpdData.Content.Count > 0)
                    {
                        var commonGridItem = new CommonGridItem();
                        commonGridItem.Update(mpdData.Content);
                        _foundItems.Add(commonGridItem);
                    }
                    //_foundTracks.NotifyCollectionChanged();
                }
            }
        }


        private void listview_Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
//                _server.Artists.ClearAndNotify();
//                _server.Albums.ClearAndNotify();
//                _server.Tracks.ClearAndNotify();
//                _server.List("artist", "genre", si.Name);
            }
        }

        private async void appbtn_Genre_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //if (listview_GenreTracks.SelectedItems.Count > 0)
            //{
            //    foreach (Track track in listview_GenreTracks.SelectedItems)
            //        _server.Add(track.File);
            //}
            //else
            //{
            //    if (listview_GenreAlbums.SelectedItems.Count > 0)
            //    {
            //        var si = listview_GenreAlbums.SelectedItem as CommonGridItem;
            //        if (si != null)
            //            _server.SearchAdd("album", si.Name);
            //    }
            //    else
            //    {
            //        if (listview_GenreArists.SelectedItems.Count > 0)
            //        {
            //            var si = listview_GenreArists.SelectedItem as CommonGridItem;
            //            if (si != null)
            //                _server.SearchAdd("artist", si.Name);
            //        }
            //        else
            //        {
            //            if (listview_Genres.SelectedItems.Count > 0)
            //            {
            //                var si = listview_Genres.SelectedItem as CommonGridItem;
            //                if (si != null)
            //                    _server.SearchAdd("genre", si.Name);
            //            }
            //            else
            //            {
            //                await MessageBox(_resldr.GetString("AddingTrackToPlaylist"),
            //                    _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
            //            }
            //        }
            //    }
            //}
        }

        private void textbox_GenreSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Text = string.Empty;
        }

        private void textbox_GenreSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Text = _resldr.GetString("TypeGenreNameHere");
        }

        private void textbox_GenreSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox_GenreSearch.Text.Length > 0)
            {
                int genrescount = listview_Genres.Items.Count;
                for (int i = 0; i < genrescount; i++)
                {
                    var genre = listview_Genres.Items[i] as CommonGridItem;
                    if (genre.Name.StartsWith(textbox_GenreSearch.Text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var lastgenre = listview_Genres.Items[genrescount - 1];
                        listview_Genres.ScrollIntoView(lastgenre);
                        listview_Genres.ScrollIntoView(genre);
                        break;
                    }
                }
            }
        }

        private void listview_Arists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                //                _server.Albums.ClearAndNotify();
                //_server.Tracks.ClearAndNotify();
                _server.List("album", "artist", si.Name);
            }
        }

        private void listview_Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                //_server.Tracks.ClearAndNotify();
                _server.Search("album", si.Name);
            }
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
           // throw new NotImplementedException();
        }

        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }
    }
}
