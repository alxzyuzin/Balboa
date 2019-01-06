/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Главная страница приложения
 *
 --------------------------------------------------------------------------*/

using Balboa.Common;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
[assembly: CLSCompliant(false)]

namespace Balboa
{

    // Change the current culture to the French culture,
    // and parsing the same string yields a different value.
    //  Thread.CurrentThread.CurrentCulture = new CultureInfo("Fr-fr", true);
    //  myDateTime = DateTime.Parse(dt);

    public enum ControlAction {NoAction, RestartServer, SwitchToTrackDirectory, SwitchToPlaylist }

    public partial class MainPage : Page, INotifyPropertyChanged, IDisposable
    {
        //private enum NewPlaylistNameRequestMode { SaveNewPlaylist, RenamePlaylist };

        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader _resldr = new ResourceLoader();

        //private string _connectionStatus;
        //public string ConnectionStatus
        //{
        //    get { return _connectionStatus; }
        //    set
        //    {
        //        if (_connectionStatus!=value)
        //        {
        //            _connectionStatus = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionStatus)));
        //        }
        //    }
        //}

        //private ListViewItem    _listViewItemGotFocus;
        //private string          _currentPlaylistName = string.Empty;
        //private string          _oldPlaylistName;
        //private string          _newPlaylistName;

        //NewPlaylistNameRequestMode _requestNewPlaylistNameMode;
  
        private Server _server;

        private enum DataPanelState {CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings} 

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
            this.SizeChanged += MainPage_SizeChanged;

            DataContext = this;
            _server = new Server(this);


            //_currentPlaylistName = _resldr.GetString("NewPlaylist");

            
            _server.Error += OnServerError;
            _server.CriticalError += OnServerCriticalError;
//            _server.CurrentSongData.PropertyChanged += OnCurrentSongDataPropertyChanged;
            _server.CommandCompleted += OnCommandCompleted;

            //////
            p_Settings.Init(_server);
            p_TrackDirectory.Init(_server);
            p_ControlPanel.Init(_server);
            p_CurrentPlaylist.Init(_server);
            p_PageHeader.Init(_server);


            p_Settings.PropertyChanged += OnDataPanelPropertyChanged;
            p_TrackDirectory.PropertyChanged += OnDataPanelPropertyChanged;
            p_CurrentPlaylist.PropertyChanged += OnDataPanelPropertyChanged;


            //////


            // Установка контекстов для databinding
            stp_MainMenu.DataContext = _server.StatusData;
//            stackpanel_MainPanelHeader.DataContext = _server.CurrentSongData;
//            gr_CurrentTrack.DataContext = _server.CurrentSongData;
            gr_Stats.DataContext            = _server.StatisticData;
            gr_SavedPlaylistsContent.ItemsSource = _server.SavedPlaylistsData;

            listview_Genres.ItemsSource = _server.Genres;
            listview_Arists.ItemsSource = _server.Artists;
            listview_Albums.ItemsSource = _server.Albums;
            listview_Tracks.ItemsSource = _server.Tracks;
            listview_GenreArists.ItemsSource = _server.Artists;
            listview_GenreAlbums.ItemsSource = _server.Albums;
            listview_GenreTracks.ItemsSource = _server.Tracks;
            listview_Search.ItemsSource = _server.Tracks;

            if (_server.Initialized)
            {
                _server.Start();         // Запускаем сеанс взаимодействия с MPD
                SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
             }
            else
            {
                SwitchDataPanelsTo(DataPanelState.Settings);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var displayinformation = DisplayInformation.GetForCurrentView();

            //if (displayinformation.CurrentOrientation == DisplayOrientations.Landscape || displayinformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
            //{
            //    if (e.NewSize.Width >= 1100)
            //    {
            //        VisualStateManager.GoToState(this, "Default", true);
            //    }

            //    if (e.NewSize.Width < 1100)
            //    {
            //        // Прячем горизонтальный регулятор громкости
            //        // Показываем кнопку регулятора громкости
            //        // Меняем размер колонки 1 в панели управления воспроизведения grid_PlayControls.ColumnDefinitions[1];
            //        // (меняем 300 на 20*
            //        VisualStateManager.GoToState(this, "Filled", true);
            //    }

            //    if (e.NewSize.Width < 900)
            //    {
            //        VisualStateManager.GoToState(this, "Narrow", true);
            //    }

            //    if (e.NewSize.Width < 620)
            //    {
            //        VisualStateManager.GoToState(this, "SuperNarrow", true);
            //    }
            //}

            //if (displayinformation.CurrentOrientation == DisplayOrientations.Portrait || displayinformation.CurrentOrientation == DisplayOrientations.PortraitFlipped)
            //{
            //    VisualStateManager.GoToState(this, "Portrait", true); 
            //}
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            _server.Halt();
        }

        private void OnResuming(Object sender, Object e)
        {
            _server.Start();
        }

        private async void OnDataPanelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {

            var panel = sender as IDataPanel;
                switch (eventArgs.PropertyName)
                {
                    case "Message":
                        await DisplayMessage(panel.Message);
                        break;
                case "Action":
                    switch(panel.Action)
                    {
                        case ControlAction.RestartServer:
                            _server.Restart();
                            break;
                        case ControlAction.SwitchToTrackDirectory:
                            SwitchDataPanelsTo(DataPanelState.FileSystem);
                            break;
                        case ControlAction.SwitchToPlaylist:
                            SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
                            break;

                    }
                    break;
                }
                
            
        }


        #region Обработчики событий объекта Server

        //private void OnStatusDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case "TimeElapsed":
        //            if(!_seekBarIsBeingDragged)
        //                pb_Progress.Value = _server.StatusData.TimeElapsed;
        //            break;
        //        case "SongId": _server.CurrentSong();break;
        //        case "PlaylistId": _server.PlaylistInfo(); break;
        //        case "State": // "&#xE102;" - Play, "&#xE103;" - Pause
        //            appbtn_PlayPause.Content = (_server.StatusData.State == "play") ? '\xE103' : '\xE102';
                    
        //            if (_server.StatusData.State == "stop")
        //            {
        //                if (stackpanel_MainPanelHeader.Opacity != 0)
        //                    stackpanel_MainPanelHeaderHideStoryboard.Begin();
        //            }
        //            else
        //            {
        //                if (stackpanel_MainPanelHeader.Opacity == 0)
        //                    stackpanel_MainPanelHeaderShowStoryboard.Begin();
        //            }
        //            break;
        //        case "Volume":
        //            _volumeChangedByStatus = true;
        //            sl_Volume.Value = _server.StatusData.Volume;
        //            sl_VerticalVolume.Value = _server.StatusData.Volume;
        //            break;
        //    }
        //}

        /// <summary>
        /// Устанавливает свойство IsPlaying для текущего проигрываемого трека равным true
        /// и для предыдущего трека равным false
        /// В зависимости от значения свойства устанавливается стиль отображения трека в Playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCurrentSongDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
            {
 //               _server.StatusData.Duration = _server.CurrentSongData.Duration;
//                pb_Progress.Maximum = _server.CurrentSongData.Duration;
                //HightlightCurrentPlaingTrack();

                if (_server.Settings.MusicCollectionFolderToken.Length == 0)
                    return;
                
                //string s = Utilities.ExtractFilePath(_server.CurrentSongData.File);
                //try
                //{
                //  //  image_AlbumCover.Source = image_AlbumCoverSmall.Source = await GetFolderImage(_server.Settings.MusicCollectionFolder, s, _server.Settings.AlbumCoverFileName);
                //}
                //catch (UnauthorizedAccessException )
                //{
                //    string message = string.Format(_resldr.GetString("CheckDirectoryAvailability"), _server.Settings.MusicCollectionFolder);
                //    switch (await MessageBox(_resldr.GetString("UnauthorizedAccessError"), message, MsgBoxButtons.GoToSettings | MsgBoxButtons.Exit))
                //    {
                //        case MsgBoxButtons.Exit: Application.Current.Exit(); break;
                //        case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings); ; break;
                //    }
                //}
                //catch (Exception ee)
                //{
                //    MsgBoxButtons responce = await MessageBox(_resldr.GetString("Error"), string.Format(_resldr.GetString("Exception"),ee.GetType().ToString(),ee.Message), MsgBoxButtons.Continue);
                //    switch (responce)
                //    {
                //        case MsgBoxButtons.Exit: Application.Current.Exit(); break;
                //        case MsgBoxButtons.Continue: _server.Restart(); break;
                //    }
                //}
            }
        }

        private       void OnCommandCompleted(object sender, EventArgs eventArgs)
        {
            ServerEventArgs args = eventArgs as ServerEventArgs;
            switch (args.Command)
            {
                case "load":
                    if (args.Result == "OK")
                    { 
                        //tbk_PlaylistName.Text = _currentPlaylistName;
                        //NotifyUser(_resldr.GetString("PlaylistLoading"),
                        //   string.Format(CultureInfo.CurrentCulture, _resldr.GetString("PlaylistLoaded"), _currentPlaylistName));
                    }
                    else
                    {
                        //NotifyUser(_resldr.GetString("Error"), 
                        //   string.Format(CultureInfo.CurrentCulture, _resldr.GetString("PlaylistError"), _currentPlaylistName, args.Message));
                    }
                    break; 
                case "search":
                    if (_server.Tracks.Count == 0)
                          textblock_SearchResult.Text = _resldr.GetString("SearchComplete");
                    else
                        textblock_SearchResult.Text = "";
                    break;

                case "playlistinfo":
 //                   if (_server.PlaylistData.Count == 0)
                        playlastaddedtrack = false;
                    //textblock_PlaylistContent.Text = _resldr.GetString("PlaylistIsEmpty");
 //                   else
 //                   { 
                        //textblock_PlaylistContent.Text = string.Empty;
                        if (playlastaddedtrack)
                        {
                          //  var track = lv_PlayList.Items[lv_PlayList.Items.Count-1] as Track;
                          //  _server.PlayId(track.Id);
                            playlastaddedtrack = false;
                        }
                        //HightlightCurrentPlaingTrack();
//                    }
                    break;
 
            }
        }

        private async void OnServerError(object sender, EventArgs eventArgs)
        {
            _server.Halt();
            switch(await MessageBox(_resldr.GetString("Error"),((ServerEventArgs) eventArgs).Message, MsgBoxButtons.GoToSettings | MsgBoxButtons.Retry | MsgBoxButtons.Exit))
            {
                case MsgBoxButtons.Exit: Application.Current.Exit(); break;
                case MsgBoxButtons.Retry: _server.Start(); break;
                case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings);break;
            }
        }

        private async void OnServerCriticalError(object sender, EventArgs eventArgs)
        {
            _server.Halt();
            MsgBoxButtons responce = await MessageBox(_resldr.GetString("CriticalError"), ((ServerEventArgs)eventArgs).Message, MsgBoxButtons.GoToSettings | MsgBoxButtons.CloseApplication);
            switch (responce)
            {
                case MsgBoxButtons.CloseApplication: Application.Current.Exit();break;
                case MsgBoxButtons.Retry: _server.Restart();break;
                case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings);break;
            }
        }

         #endregion

        private async void NotifyUser(string title, string message)
        {
            object cmdid= new object();
            MessageDialog md = new MessageDialog(message, title);
            md.Commands.Add(new UICommand(_resldr.GetString("Close"), null, 2));
            var selected = await md.ShowAsync();
            return;
        }
 
        #region Seek bar
        //private bool _seekBarIsBeingDragged = false;
        //private double _currentTrackPosition = 0;

        //private void pb_Progress_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        //{
        //    _seekBarIsBeingDragged = true;
        //    var sl = sender as Slider;
        //    _currentTrackPosition = sl.Value;
        // }

        //private void pb_Progress_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        //{
        //    var sl = sender as Slider;
        //    double offset = sl.Value - _currentTrackPosition;

        //    string soffset = offset.ToString(CultureInfo.InvariantCulture);
        //    if (offset > 0)
        //         soffset = "+"+soffset;
        //    _server.SeekCurrent(soffset);
        //    _seekBarIsBeingDragged = false;
        //}
       
        #endregion

        #region Обработка событий интерфейса

       
        #region MAIN MENU

        private void ts_Random_Toggled(object sender, RoutedEventArgs e)
        {
            _server.Random(((ToggleSwitch)(sender)).IsOn);
        }

        private void ts_Repeat_Toggled(object sender, RoutedEventArgs e)
        {
            _server.Repeat(((ToggleSwitch)(sender)).IsOn);
        }

        private void ts_Consume_Toggled(object sender, RoutedEventArgs e)
        {
            _server.Consume(((ToggleSwitch)(sender)).IsOn);
        }

        private void btn_Playlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
        }

        /// <summary>
        /// Отображает содержимое закладки File system (Содержимое корневого каталога MPD )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_FileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.FileSystem);
        }



        
        /// ///////////////////////////////////////////////////////////////////////////////////
        
        #endregion

        #region  CURRENT TRACK
        private void btn_CurrentTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.CurrentTrack);
        }

        #endregion

        #region GENRES

        private void btn_Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Genres);
    
            if (_server.Genres.Count == 0)
                _server.List("genre");

            _server.Artists.ClearAndNotify();
            _server.Albums.ClearAndNotify();
            _server.Tracks.ClearAndNotify();
         }

        private void listview_Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                _server.Artists.ClearAndNotify();
                _server.Albums.ClearAndNotify();
                _server.Tracks.ClearAndNotify();
                _server.List("artist", "genre", si.Name);
            }
        }

        private async void appbtn_Genre_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_GenreTracks.SelectedItems.Count > 0)
            {
                foreach (Track track in listview_GenreTracks.SelectedItems)
                    _server.Add(track.File);
            }
            else
            {
                if (listview_GenreAlbums.SelectedItems.Count > 0)
                {
                    var si = listview_GenreAlbums.SelectedItem as CommonGridItem;
                    if (si!=null)
                        _server.SearchAdd("album", si.Name);
                }
                else
                {
                    if (listview_GenreArists.SelectedItems.Count > 0)
                    {
                        var si = listview_GenreArists.SelectedItem as CommonGridItem;
                        if (si != null)
                            _server.SearchAdd("artist", si.Name);
                    }
                    else
                    {
                        if (listview_Genres.SelectedItems.Count > 0)
                        {
                            var si = listview_Genres.SelectedItem as CommonGridItem;
                            if (si != null)
                                _server.SearchAdd("genre", si.Name);
                        }
                        else
                        {
                            await MessageBox(_resldr.GetString("AddingTrackToPlaylist"), 
                                _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
                        }
                    }
                }
            }
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
        #endregion

        #region ARTISTS

        private void btn_Artists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Artists);
            _server.Albums.ClearAndNotify();
            _server.Tracks.ClearAndNotify();
            _server.List("artist");
         }

        private void listview_Arists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si!=null)
            {
                _server.Albums.ClearAndNotify();
                _server.Tracks.ClearAndNotify();
                _server.List("album", "artist", si.Name);
            }
        }

        private void listview_Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListViewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                _server.Tracks.ClearAndNotify();
                _server.Search("album", si.Name);
            }
        }

        private async void appbtn_Artist_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_Tracks.SelectedItems.Count>0)
            {
                foreach (Track track in listview_Tracks.SelectedItems)
                    _server.Add(track.File);
            }
            else
            {
                if (listview_Albums.SelectedItems.Count > 0)
                {
                    var si = listview_Albums.SelectedItem as CommonGridItem;
                    _server.SearchAdd("album", si.Name);
                }
                else
                {
                    if (listview_Arists.SelectedItems.Count > 0)
                    {
                        var si = listview_Arists.SelectedItem as CommonGridItem;
                        _server.SearchAdd("artist", si.Name);
                    }
                    else
                    {
                        await MessageBox(_resldr.GetString("AddingTrackToPlaylist"), 
                            _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
                    }
                }
            }
        }

        private void textbox_ArtistSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox_ArtistSearch.Text.Length > 0)
            {
                for (int i = 0; i < listview_Arists.Items.Count; i++)
                {
                    var artist = (CommonGridItem)(listview_Arists.Items[i]);
                    if (artist.Name.StartsWith(textbox_ArtistSearch.Text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var lastartist = (CommonGridItem)(listview_Arists.Items[listview_Arists.Items.Count-1]);
//                        var firstartist = (CommonGridItem)(listview_Arists.Items[0]);
                        listview_Arists.ScrollIntoView(lastartist);
                        listview_Arists.ScrollIntoView(artist);
                        break;
                     }
                }
            }
        }

        private void textbox_ArtistSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Text = string.Empty;
        }

        private void textbox_ArtistSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Text = _resldr.GetString("TypeArtistNameHere");
        }
        #endregion

        #region SETTINGS
       
        private void btn_Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Прочитаем список доступных выходов и добавим их в список параметров
            if (_server.IsConnected)
                _server.Outputs();
            SwitchDataPanelsTo(DataPanelState.Settings);
        }
       
        #endregion

         #endregion   ------------------------------------------------------------

   

        #region SAVED PLAYLISTS COMAND
        private void btn_SavedPlayLists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Playlists);
            _server.ListPlaylists();
        }
      
        private async void appbtn_SavedPlaylistLoad_Click(object sender, RoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi == null)
            {
                await MessageBox(_resldr.GetString("PlaylistLoading"),
                                 _resldr.GetString("PlaylisNotSelectedForLoad"), MsgBoxButtons.Continue);
            }
            else
            {
                //_currentPlaylistName = fi.Name;
                _server.Load(fi.Name);
            }
        }

        private async void appbtn_SavedPlaylistDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi == null)
            {
                await MessageBox(_resldr.GetString("PlaylistDeleting"),
                                 _resldr.GetString("PlaylistNotSelectedForDelete"), MsgBoxButtons.Continue);
            }
            else
            {
                _server.Rm(fi.Name);
                _server.ListPlaylists();
            }
        }

        private async void appbtn_SavedPlaylistRename_Tapped(object sender, TappedRoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi == null)
            {
                await MessageBox(_resldr.GetString("PlaylistRename"),
                                 _resldr.GetString("PlaylistNotSelectedForRename"), MsgBoxButtons.Continue);
            }
            else
            {
            //    _oldPlaylistName = fi.Name;
            //    tbx_PlaylistName.Text = _oldPlaylistName;
            //    _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.RenamePlaylist;
            //    RequestNewPlaylistName();
            }
        }
        #endregion

        #region STATS COMMANDS

        private void btn_Stats_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Statistic);
            _server.Stats();
        }
        #endregion

        #region SEARCH COMMANDS

        private void btn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Search);
            textblock_SearchResult.Text = string.Empty;
        }

        private void appbtn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (textbox_Search.Text.Length > 0)
            {
                textblock_SearchResult.Text = _resldr.GetString("Searching");
                _server.Tracks.ClearAndNotify();
                _server.Search("any", textbox_Search.Text);
            }
        }

        private async void appbtn_Search_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_Search.SelectedItems.Count > 0)
            {
                foreach (Track track in listview_Search.SelectedItems)
                    _server.Add(track.File);
            }
           else
            {
                await MessageBox(_resldr.GetString("AddingTrackToPlaylist"), 
                    _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
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
        #endregion

        #region UTILS
        
        
        private void SwitchDataPanelsTo(DataPanelState state)
        {
            if (popup_MainMenu.IsOpen)
                popup_MainMenu.IsOpen = false;

            SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);

            // Выключим все информационные панели
            foreach (UIElement uielement in grid_MainPanel.Children)
            {
                if (uielement is Grid || 
                    uielement is Settings || 
                    uielement is TrackDirectory ||
                    uielement is CurrentPlaylist)
                    uielement.Visibility = Visibility.Collapsed;
            }
            // Изменим цвет текста во всех кнопках главного меню на белый
            foreach (UIElement uielement in stp_MainMenu.Children)
            {
                var button = uielement as Button;
                if (button!=null)
                    button.Foreground = WhiteBrush;
            }

            // Включим нужную панель
            // Изменим цвет текста в соответствующей кнопке на оранжевый
            // Установим режим в заголовке окна

            switch (state)
            {
                case DataPanelState.Artists:
                    btn_Artists.Foreground = OrangeBrush;
                    gr_Artists.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("Artists");
                    gr_ArtistsShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentPlaylist:
                    btn_Playlist.Foreground = OrangeBrush;
                    textbox_CurrentMode.Text = _resldr.GetString("PlaylistCurrent");
                    p_CurrentPlaylist.Visibility = Visibility.Visible;
                    p_CurrentPlaylist.Update();
                    //gr_CurrentPlaylistShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentTrack:
                    btn_CurrentTrack.Foreground = OrangeBrush;
                    gr_CurrentTrack.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("CurrentTrack");
                    gr_CurrentTrackShowStoryboard.Begin();
                    break;
                case DataPanelState.FileSystem:
                    btn_FileSystem.Foreground = OrangeBrush;
                    textbox_CurrentMode.Text = _resldr.GetString("FilesAndFolders");
                    p_TrackDirectory.Visibility = Visibility.Visible;
                    p_TrackDirectory.Update();
                    break;
                case DataPanelState.Genres:
                    btn_Genres.Foreground = OrangeBrush;
                    gr_Genres.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("Genres");
                    gr_GenresShowStoryboard.Begin();
                    break;
                case DataPanelState.Playlists:
                    btn_SavedPlayLists.Foreground = OrangeBrush;
                    gr_SavedPlayLists.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("Playlist");
                    gr_SavedPlayListsShowStoryboard.Begin();
                    break;
                case DataPanelState.Search:
                    btn_Search.Foreground = OrangeBrush;
                    gr_Search.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("Search");
                    gr_SearchShowStoryboard.Begin();
                    break;
                case DataPanelState.Settings:
                    //              tbx_MusicCollectionPath.Text = _appSettings.MusicCollectionFolder;
                    //              checkbox_DisplayFolderPictures.IsChecked = _appSettings.DisplayFolderPictures;
                    //gr_Settings.Visibility = Visibility.Visible;
                    //gr_SettingsShowStoryboard.Begin();

                    btn_Settings.Foreground = OrangeBrush;
                    textbox_CurrentMode.Text = _resldr.GetString("Settings");
                    p_Settings.Visibility = Visibility.Visible;
                    break;
                case DataPanelState.Statistic:
                    btn_Stats.Foreground = OrangeBrush;
                    gr_Stats.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("Statistic");
                    gr_StatsShowStoryboard.Begin();
                         break;
             }
            if (_server.StatusData.State != "stop")
            {
                //if (state != DataPanelState.CurrentTrack)
                //{
                //    if (p_PageHeader.Opacity == 0)
                //        stackpanel_MainPanelHeaderShowStoryboard.Begin();
                //}
                //else
                //    stackpanel_MainPanelHeaderHideStoryboard.Begin();
            }

            if (state == DataPanelState.CurrentTrack)
                p_PageHeader.Visibility = Visibility.Collapsed;
            else
                p_PageHeader.Visibility = Visibility.Visible;
        }

        [Flags]
        private enum MsgBoxButtons { OK = 1,Cancel = 2, Continue = 4, Retry = 8,  Exit = 16, GoToSettings = 32, CloseApplication = 64 }

        private async Task<MsgBoxButtons> MessageBox(string title, string message, MsgBoxButtons buttons)
        {
            object cmdid = new object();
            MessageDialog md = new MessageDialog(message, title);
            
            if (buttons.HasFlag(MsgBoxButtons.OK))
                md.Commands.Add(new UICommand(_resldr.GetString("Ok"), null, MsgBoxButtons.OK));
            if (buttons.HasFlag(MsgBoxButtons.Cancel))
                md.Commands.Add(new UICommand(_resldr.GetString("Cancel"), null, MsgBoxButtons.Cancel));
            if (buttons.HasFlag(MsgBoxButtons.Continue))
                md.Commands.Add(new UICommand(_resldr.GetString("Continue"), null, MsgBoxButtons.Continue));
            if (buttons.HasFlag(MsgBoxButtons.Retry))
                md.Commands.Add(new UICommand(_resldr.GetString("Retry"), null, MsgBoxButtons.Retry));
            if (buttons.HasFlag(MsgBoxButtons.Exit))
                md.Commands.Add(new UICommand(_resldr.GetString("Exit"), null, MsgBoxButtons.Exit));
            if (buttons.HasFlag(MsgBoxButtons.GoToSettings))
                md.Commands.Add(new UICommand(_resldr.GetString("GoToSettings"), null, MsgBoxButtons.GoToSettings));
            if (buttons.HasFlag(MsgBoxButtons.CloseApplication))
                md.Commands.Add(new UICommand(_resldr.GetString("CloseApplication"), null, MsgBoxButtons.CloseApplication));
            UICommand selected = (UICommand) await md.ShowAsync();
            return (MsgBoxButtons) selected.Id;
        }
        

        #endregion

       
  
        private void textblock_MainMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
             if (popup_MainMenu.IsOpen)
                popup_MainMenu.IsOpen = false;
            else
                popup_MainMenu.IsOpen = true;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources

            }
            // free native resources
            _server.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task<MsgBoxButton> DisplayMessage(string message, MsgBoxButton button, int height)
        {
            MsgBox.SetButtons(button);
            MsgBox.Message = message;
            MsgBox.BoxHeight = height;
            return await MsgBox.Show();
        }

        private async Task<MsgBoxButton> DisplayMessage(Message messageArgs)
        {
            MsgBox.SetButtons(messageArgs.Buttons);
            MsgBox.Message = messageArgs.Text;
            MsgBox.BoxHeight = messageArgs.BoxHeight;
            return await MsgBox.Show();
        }
    }
}

  