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
using System.Globalization;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.Graphics.Display;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
[assembly: CLSCompliant(true)]

namespace Balboa
{

    // Change the current culture to the French culture,
    // and parsing the same string yields a different value.
    //  Thread.CurrentThread.CurrentCulture = new CultureInfo("Fr-fr", true);
    //  myDateTime = DateTime.Parse(dt);


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class MainPage : Page, IDisposable
    {
        private enum NewPlaylistNameRequestMode { SaveNewPlaylist, RenamePlaylist };

        ResourceLoader _resldr = new ResourceLoader();

        //private AppSettings _appSettings = new AppSettings();
        private ListViewItem    _listViewItemGotFocus;
        private GridViewItem    _gridViewItemGotFocus;
        private List<string>    _currentFilePath = new List<string>();
        private string          _parentfolder = "";
        private string          _currentPlaylistName = string.Empty;
        private string          _oldPlaylistName;
        private string          _newPlaylistName;

        NewPlaylistNameRequestMode _requestNewPlaylistNameMode;
  
        private Server _server;

        private enum DataPanelState {CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings} 

        public MainPage()
        {
            this.InitializeComponent();

            

            _currentPlaylistName = _resldr.GetString("NewPlaylist");

            this.SizeChanged += MainPage_SizeChanged;
           
            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;

            _server = new Server(this);

            _server.ConnectionStatusChanged += OnServerConnectionStatusChanged;
            _server.Error += OnServerError;
            _server.CriticalError += OnServerCriticalError;
            _server.StatusData.PropertyChanged += OnStatusDataPropertyChanged;
            _server.CurrentSongData.PropertyChanged += OnCurrentSongDataPropertyChanged;
            _server.CommandCompleted += OnCommandCompleted;
//            _server.OutputsData.CollectionChanged += OnOutputsCollectionChanged;
            _server.DirectoryData.CollectionChanged += OnFilelistChanged;

            //////
            p_Settings.Init(_server);

            p_Settings.PropertyChanged += (object obj, PropertyChangedEventArgs e ) => { _server.Restart(); };
            p_Settings.MessageReady += async (object obj, DisplayMessageEventArgs e) => { await DisplayMessage(e); };
                               
            //////


            // Установка контекстов для databinding
            stp_ServerStatus.DataContext = _server.StatusData;
            stp_MainMenu.DataContext = _server.StatusData;

            stackpanel_MainPanelHeader.DataContext = _server.CurrentSongData;

            tb_ConnectionStatus.DataContext = _server;

            gr_CurrentTrack.DataContext = _server.CurrentSongData;
            gr_Stats.DataContext            = _server.StatisticData;
//            gr_Settings.DataContext         = _appSettings;

            lv_PlayList.ItemsSource         = _server.PlaylistData;
            gr_FileSystemContent.ItemsSource = _server.DirectoryData;
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
            var displayinformation = DisplayInformation.GetForCurrentView();

            if (displayinformation.CurrentOrientation == DisplayOrientations.Landscape || displayinformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
            {
                if (e.NewSize.Width >= 1100)
                {
                    VisualStateManager.GoToState(this, "Default", true);
                }

                if (e.NewSize.Width < 1100)
                {
                    // Прячем горизонтальный регулятор громкости
                    // Показываем кнопку регулятора громкости
                    // Меняем размер колонки 1 в панели управления воспроизведения grid_PlayControls.ColumnDefinitions[1];
                    // (меняем 300 на 20*
                    VisualStateManager.GoToState(this, "Filled", true);
                }

                if (e.NewSize.Width < 900)
                {
                    VisualStateManager.GoToState(this, "Narrow", true);
                }

                if (e.NewSize.Width < 620)
                {
                    VisualStateManager.GoToState(this, "SuperNarrow", true);
                }
            }

            if (displayinformation.CurrentOrientation == DisplayOrientations.Portrait || displayinformation.CurrentOrientation == DisplayOrientations.PortraitFlipped)
            {
                VisualStateManager.GoToState(this, "Portrait", true); 
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            _server.Halt();
        }

        private void OnResuming(Object sender, Object e)
        {
            _server.Start();
        }

        #region Обработчики событий объекта Server

        private void OnStatusDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TimeElapsed":
                    if(!_seekBarIsBeingDragged)
                        pb_Progress.Value = _server.StatusData.TimeElapsed;
                    break;
                case "SongId": _server.CurrentSong();break;
                case "PlaylistId": _server.PlaylistInfo(); break;
                case "State": // "&#xE102;" - Play, "&#xE103;" - Pause
                    appbtn_PlayPause.Content = (_server.StatusData.State == "play") ? '\xE103' : '\xE102';
                    
                    if (_server.StatusData.State == "stop")
                    {
                        if (stackpanel_MainPanelHeader.Opacity != 0)
                            stackpanel_MainPanelHeaderHideStoryboard.Begin();
                    }
                    else
                    {
                        if (stackpanel_MainPanelHeader.Opacity == 0)
                            stackpanel_MainPanelHeaderShowStoryboard.Begin();
                    }
                    break;
                case "Volume":
                    _volumeChangedByStatus = true;
                    sl_Volume.Value = _server.StatusData.Volume;
                    sl_VerticalVolume.Value = _server.StatusData.Volume;
                    break;
            }
        }

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
                _server.StatusData.Duration = _server.CurrentSongData.Duration;
                pb_Progress.Maximum = _server.CurrentSongData.Duration;
                HightlightCurrentPlaingTrack();

                if (_server.Settings.MusicCollectionFolderToken.Length == 0)
                    return;
                
                string s = Utilities.ExtractFilePath(_server.CurrentSongData.File);
                try
                {
                    image_AlbumCover.Source = image_AlbumCoverSmall.Source = await Utilities.GetFolderImage(_server.Settings.MusicCollectionFolder, s, _server.Settings.AlbumCoverFileName);
                }
                catch (UnauthorizedAccessException )
                {
                    string message = string.Format(_resldr.GetString("CheckDirectoryAvailability"), _server.Settings.MusicCollectionFolder);
                    switch (await MessageBox(_resldr.GetString("UnauthorizedAccessError"), message, MsgBoxButtons.GoToSettings | MsgBoxButtons.Exit))
                    {
                        case MsgBoxButtons.Exit: Application.Current.Exit(); break;
                        case MsgBoxButtons.GoToSettings: SwitchDataPanelsTo(DataPanelState.Settings); ; break;
                    }
                }
                catch (Exception ee)
                {
                    MsgBoxButtons responce = await MessageBox(_resldr.GetString("Error"), string.Format(_resldr.GetString("Exception"),ee.GetType().ToString(),ee.Message), MsgBoxButtons.Continue);
                    switch (responce)
                    {
                        case MsgBoxButtons.Exit: Application.Current.Exit(); break;
                        case MsgBoxButtons.Continue: _server.Restart(); break;
                    }
                }
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
                    tbk_PlaylistName.Text = _currentPlaylistName;
                        NotifyUser(_resldr.GetString("PlaylistLoading"),
                           string.Format(CultureInfo.CurrentCulture, _resldr.GetString("PlaylistLoaded"), _currentPlaylistName));
                    }
                    else
                    {
                        NotifyUser(_resldr.GetString("Error"), 
                           string.Format(CultureInfo.CurrentCulture, _resldr.GetString("PlaylistError"), _currentPlaylistName, args.Message));
                    }
                    break; 
                case "search":
                    if (_server.Tracks.Count == 0)
                          textblock_SearchResult.Text = _resldr.GetString("SearchComplete");
                    else
                        textblock_SearchResult.Text = "";
                    break;

                case "playlistinfo":
                    if (_server.PlaylistData.Count == 0)
                        textblock_PlaylistContent.Text = _resldr.GetString("PlaylistIsEmpty");
                    else
                    { 
                        textblock_PlaylistContent.Text = string.Empty;
                        if (playlastaddedtrack)
                        {
                            var track = lv_PlayList.Items[lv_PlayList.Items.Count-1] as Track;
                            _server.PlayId(track.Id);
                            playlastaddedtrack = false;
                        }
                        HightlightCurrentPlaingTrack();
                    }
                    break;
                case "lsinfo":
                    textblock_FileSystemContent.Text = _server.DirectoryData.Count == 0? 
                        string.Format(CultureInfo.CurrentCulture, _resldr.GetString("NoAudioFilesInFolder"),
                                        Utilities.BuildFilePath(_currentFilePath))
                        :string.Empty;
                    break;
            }
        }

 
        private       void OnFilelistChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // При переходе на уровень вверх по файловой системе подсветим 
            //последний открывавшийся фолдер и прокрутим Grid так чтобы он был виден

            foreach (File item in _server.DirectoryData)
            {
                if (item.Name.ToLower() == _parentfolder.ToLower())
                {
                    item.JustClosed = true;
                    gr_FileSystemContent.ScrollIntoView(item, ScrollIntoViewAlignment.Default);
                    break;
                }
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

        private       void OnServerConnectionStatusChanged(object sender, EventArgs eventArgs)
        {
            if(((ServerEventArgs)eventArgs).ConnectionStatus)
                _server.PlaylistInfo();
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
        private bool _seekBarIsBeingDragged = false;
        private double _currentTrackPosition = 0;

        private void pb_Progress_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _seekBarIsBeingDragged = true;
            var sl = sender as Slider;
            _currentTrackPosition = sl.Value;
         }

        private void pb_Progress_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var sl = sender as Slider;
            double offset = sl.Value - _currentTrackPosition;

            string soffset = offset.ToString(CultureInfo.InvariantCulture);
            if (offset > 0)
                 soffset = "+"+soffset;
            _server.SeekCurrent(soffset);
            _seekBarIsBeingDragged = false;
        }
       
        #endregion

        #region Обработка событий интерфейса

        #region УПРАВЛЕНИЕ ВОСПРИЗВЕДЕНИЕМ

        private void btn_PrevTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Previous();
        }

        private void btn_PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // "&#xE102;" - Play
            // "&#xE103;" - Pause
            Button button = (Button)sender;
            switch(_server.StatusData.State)
            {
                case "stop": _server.Play();
                    // Устанавливаем символ "Pause"
                    button.Content = '\xE103';
                    break;
                case "pause": _server.Pause();    // Если статус сервера "pause" то команда Pause() запускает проигрывание
                    button.Content = '\xE103';   // Устанавливаем символ "Pause"
                    break;
                case "play":
                    _server.Pause();    // Если статус сервера "play" то команда Pause() прерывает проигрывание
                    button.Content = '\xE102';  //57602 // Устанавливаем символ "Play"
                    break;
            }
     
            bool playedtrackselected = false; // Устанавливаем пизнак того что проигрываемый трек не подсвечен
            // Ищем в Playlist трек с установленным признаком IsPlaying
            // и если такой трек находится то прокручиваем Playlist так чтобы трек быд виден
            foreach (Track item in _server.PlaylistData)
            {
                if (item.IsPlaying)
                {
                    playedtrackselected = true;
                    lv_PlayList.ScrollIntoView(item);
                    return;
                }
            }
            // Если трек с признаком IsPlaying не найден и Playlist не пуст устанавливаем признак проигрываемого трека
            // на первый элемент в Playlist
            if (!playedtrackselected && _server.PlaylistData.Count>0)
            {
                _server.PlaylistData[0].IsPlaying = true;
            }
         }

        private void btn_NextTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Next();
        }

        private void btn_Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Stop();
        }
    
        #endregion

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
       
        private async void btn_Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Прочитаем список доступных выходов и добавим их в список параметров
            if (_server.IsConnected)
                _server.Outputs();
            SwitchDataPanelsTo(DataPanelState.Settings);
        }
        /*
        private async void appbtn_SaveSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string notNumber = @"\D{1,}";

            if (Regex.IsMatch(_appSettings.Port, notNumber) || _appSettings.Port.Length == 0)
            {
                await MessageBox(_resldr.GetString("Eror"),
                    _resldr.GetString("PortValueMustBeNumber"), MsgBoxButtons.Continue);
                return;
             }
             else
             {
                int port = int.Parse(_appSettings.Port);
                if (port > 65535)
                {
                    await MessageBox(_resldr.GetString("Eror"),
                       _resldr.GetString("PortValueMustBeLess65536"), MsgBoxButtons.Continue);
                    return;
                }
             }

             if (Regex.IsMatch(_appSettings.ViewUpdateInterval, notNumber) || _appSettings.ViewUpdateInterval.Length==0)
             {
                await MessageBox(_resldr.GetString("Eror"), 
                    _resldr.GetString("ViewUpdateIntervalMustNumber"), MsgBoxButtons.Continue);
                return;
             }
             else
             {
                int updateinterval = int.Parse(_appSettings.ViewUpdateInterval);
                if (updateinterval < 100)
                {
                    await MessageBox(_resldr.GetString("Eror"), 
                        _resldr.GetString("ViewUpdateIntervalMustBe100"), MsgBoxButtons.Continue);
                    return;
                }
              }

              //if (!_appSettings.SettingsChanged)
              //      return;
               // Проверим возможность соединения с новыми параметрами перед их сохранением
              Connection connection = new Connection();
              bool connectionIsOk = await connection.Open(_appSettings.Server, _appSettings.Port, _appSettings.Password);
              connection.Close();

            if (!connectionIsOk)
            {
                await MessageBox(_resldr.GetString("ConnectionError"), connection.Error , MsgBoxButtons.OK);
                return;
            }
            if (_appSettings.ServerNameChanged && (!_appSettings.MusicCollectionFolderTokenChanged))
            {
                await MessageBox(_resldr.GetString("Warning"),
                    _resldr.GetString("ServerNameChanged"), MsgBoxButtons.Continue);
                tbx_MusicCollectionPath.Text = String.Empty;
                StorageApplicationPermissions.FutureAccessList.Clear();
                _appSettings.MusicCollectionFolderToken = String.Empty;
                _appSettings.MusicCollectionFolder = String.Empty;
            }
            _appSettings.Save();

            Server.Host = _appSettings.Server;
            Server.Port = _appSettings.Port;
            Server.Password = _appSettings.Password;
            Server.ViewUpdateInterval =  int.Parse(_appSettings.ViewUpdateInterval);
            Server.MusicCollectionFolder = _appSettings.MusicCollectionFolder;
            Server.AlbumCoverFileNames = _appSettings.AlbumCoverFileName;
            Server.DisplayFolderPictures = (bool)_appSettings.DisplayFolderPictures;

            Server.Restart();
        }

        private void ts_Output_Switched(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            int output = int.Parse(ts.Name, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (ts.IsOn)
                Server.EnableOutput(output);
            else
                Server.DisableOutput(output);
        }

        private async void appbtn_TestConnection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string message = string.Empty;

            Connection connection = new Connection();

            bool connectionresult = await connection.Open(tbx_Server.Text, tbx_Port.Text, tbx_Password.Text);
            if (connectionresult)
            {
                connection.Close();
                message = string.Format(_resldr.GetString("ConnectedSuccesfully"),connection.InitialResponse);
            }
            else
            {
                message = string.Format(_resldr.GetString("ConnectionErrorDescription"), connection.Error);
            }

            MsgBoxButtons b = await MessageBox(_resldr.GetString("ConnectionTest"), message, MsgBoxButtons.OK);
        }

        private async void btn_SelectMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                tbx_MusicCollectionPath.Text = folder.Path;
                if (_appSettings.MusicCollectionFolderToken != String.Empty )
                    StorageApplicationPermissions.FutureAccessList.Clear();
                _appSettings.MusicCollectionFolderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
            }
         }

        private void btn_ClearMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
            _appSettings.MusicCollectionFolderToken = String.Empty;
            _appSettings.MusicCollectionFolder = String.Empty;
            tbx_MusicCollectionPath.Text = String.Empty;
        }

        private void appbtn_StartSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Server.Restart();
            Server.Start();
        }

        private void appbtn_StopSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Halt();
        }
        */
        #endregion

        #region CURRENT PLAYLIST COMMANDS --------------------------------------------------

        private void btn_Playlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
        }

        private void lv_PlayList_GotFocus(object sender, RoutedEventArgs e)
        {
            _listViewItemGotFocus = e.OriginalSource as ListViewItem;
        }

        private void lv_PlayList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_listViewItemGotFocus != null)
            { 
                Track playlistitem = _listViewItemGotFocus.Content as Track;
                _server.PlayId(playlistitem.Id);
            }
            //int index = lv_PlayList.SelectedItems.IndexOf(playlistitem);
            //if (index>=0)
            //    lv_PlayList.SelectedItems.RemoveAt(index);
        }

        private void appbtn_Playlist_Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.SaveNewPlaylist;
            tbx_PlaylistName.Text = _currentPlaylistName;
            RequestNewPlaylistName();
        }

        private void appbtn_Playlist_DeleteSelected_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (Track track in lv_PlayList.SelectedItems)
            {
                _server.DeleteId(track.Id);
            }
        }

        private void appbtn_Playlist_Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Clear();
        }

        private bool EnteredFromPlaylistMode = false;
        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EnteredFromPlaylistMode = true;
            btn_FileSystem_Tapped(sender, e);
        }

        private void appbtn_Playlist_Shaffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Shuffle();
        }

        private void btn_PlaylistNameSave_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(_requestNewPlaylistNameMode == NewPlaylistNameRequestMode.SaveNewPlaylist)
            { 
                _currentPlaylistName = tbx_PlaylistName.Text;
                tbk_PlaylistName.Text = _currentPlaylistName;
            
                string str = tbk_PlaylistName.Text;
            
                Encoding encoding = Encoding.Unicode;
                byte[] encBytes = encoding.GetBytes(str);
                byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);

                str = Encoding.UTF8.GetString(utf8Bytes,0, utf8Bytes.Length);

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

        #endregion   ------------------------------------------------------------

        #region FILE SYSTEM COMMANDS  -----------------------------------------------
        /// <summary>
        /// Отображает содержимое закладки File system (Содержимое корневого каталога MPD )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

       
        private void btn_FileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            appbtn_Up.IsEnabled = false;
            _currentFilePath.Clear();

            _server.CurrentFolder = String.Empty;

            SwitchDataPanelsTo(DataPanelState.FileSystem);
            _server.LsInfo();
        }

        private void gr_FileSystemContent_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_gridViewItemGotFocus == null)
                return;

            File currentfileitem = _gridViewItemGotFocus.Content as File;

            if (currentfileitem == null)
                return;

            if (currentfileitem.Nature == FileNature.Directory)
            {
                _currentFilePath.Add(currentfileitem.Name.Trim());
                _server.CurrentFolder = Utilities.BuildFilePath(_currentFilePath);
                _server.LsInfo(_server.CurrentFolder);
                appbtn_Up.IsEnabled = true;
            }
        }

        private void gr_FileSystemContent_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e != null)
                _gridViewItemGotFocus = e.OriginalSource as GridViewItem;
        }

        private void appbtn_Up_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Если мы не на самом верхнем уровне в дереве каталогов то удалим последний элемент 
            // в списке каталогов
            if (_currentFilePath.Count > 0)
            {
                _parentfolder = _currentFilePath[_currentFilePath.Count - 1];
                _currentFilePath.RemoveAt(_currentFilePath.Count - 1);
            }

            _server.CurrentFolder = Utilities.BuildFilePath(_currentFilePath);
            _server.LsInfo(_server.CurrentFolder);

            // Eсли мы поднялись на самый верх по дереву каталогов отключим кнопку Up
            if (_currentFilePath.Count > 0)
                appbtn_Up.IsEnabled = true;
            else
                appbtn_Up.IsEnabled = false;

        }

        private void appbtn_RescanDatabase_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Update();
        }

        private async void appbtn_AddFromFileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string path = string.Empty;
            if (_currentFilePath.Count>0)
                path = Utilities.BuildFilePath(_currentFilePath)+ "/";
            if (gr_FileSystemContent.SelectedItems.Count > 0)
            {
                foreach (File item in gr_FileSystemContent.SelectedItems)
                {
                    string lp = path + item.Name;
                    _server.Add(path + item.Name);
                }
                if (EnteredFromPlaylistMode)
                {
                    // возвращаемся в Playlist
                    SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
                }
            }
            else
            {
               await MessageBox(_resldr.GetString("AddingTrackToPlaylist"), 
                   _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButtons.Continue);
            }
        }


        #endregion --------------------------------------------------------------

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
                _currentPlaylistName = fi.Name;
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
                _oldPlaylistName = fi.Name;
                tbx_PlaylistName.Text = _oldPlaylistName;
                _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.RenamePlaylist;
                RequestNewPlaylistName();
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
            _listViewItemGotFocus = e.OriginalSource as ListViewItem;
        }

        private bool playlastaddedtrack = false;
        private void listview_Search_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Track track = _listViewItemGotFocus.Content as Track;
            _server.Add(track.File);
            playlastaddedtrack = true;
         }
        #endregion

        #region UTILS
        private void HightlightCurrentPlaingTrack()
        {
            foreach (Track item in lv_PlayList.Items)
            {
                if (item.Id == _server.CurrentSongData.Id)
                {
                    item.IsPlaying = true;
                    lv_PlayList.ScrollIntoView(item);
                    if (lv_PlayList.SelectedItems.IndexOf(item) >= 0)
                        lv_PlayList.SelectedItems.Remove(item);
                }
                else
                {
                    item.IsPlaying = false;
                }
            }
        }
        
        private void SwitchDataPanelsTo(DataPanelState state)
        {
            if (popup_MainMenu.IsOpen)
                popup_MainMenu.IsOpen = false;

            SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);

            // Выключим все информационные панели
            foreach (UIElement uielement in grid_MainPanel.Children)
            {
                if (uielement is Grid || uielement is Settings)
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
                    gr_Playlist.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("PlaylistCurrent");
                    gr_CurrentPlaylistShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentTrack:
                    btn_CurrentTrack.Foreground = OrangeBrush;
                    gr_CurrentTrack.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("CurrentTrack");
                    gr_CurrentTrackShowStoryboard.Begin();
                    break;
                case DataPanelState.FileSystem:
                    btn_FileSystem.Foreground = OrangeBrush;
                    gr_FileSystem.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = _resldr.GetString("FilesAndFolders");
                    gr_FileSystemShowStoryboard.Begin();
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
                    btn_Settings.Foreground = OrangeBrush;
                    textbox_CurrentMode.Text = _resldr.GetString("Settings");
                    p_Settings.Visibility = Visibility.Visible;
                    //gr_Settings.Visibility = Visibility.Visible;

                    //gr_SettingsShowStoryboard.Begin();
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
                if (state != DataPanelState.CurrentTrack)
                {
                    if (stackpanel_MainPanelHeader.Opacity == 0)
                        stackpanel_MainPanelHeaderShowStoryboard.Begin();
                }
                else
                    stackpanel_MainPanelHeaderHideStoryboard.Begin();
            }

            if (state == DataPanelState.CurrentTrack)
                stackpanel_MainPanelHeader.Visibility = Visibility.Collapsed;
            else
                stackpanel_MainPanelHeader.Visibility = Visibility.Visible;
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
        
//        private void SetPlaybuttonPicture(string state)
//        {
//            // "&#xE102;" - Play
//            // "&#xE103;" - Pause
//
//            if (state == "play")
//            {
//                appbtn_PlayPause.Content = '\xE103'; // Устанавливаем символ "Pause"
//            }
//            else
//            {    
//                appbtn_PlayPause.Content = '\xE102'; // Устанавливаем символ "Play"
//            }
//        }

        private void RequestNewPlaylistName()
        { 
            CoreWindow currentWindow = Window.Current.CoreWindow;
            popup_GetPlaylistName.HorizontalOffset = (currentWindow.Bounds.Width / 2) - (400 / 2);
            popup_GetPlaylistName.VerticalOffset = (currentWindow.Bounds.Height / 2) - (150 / 2);

            Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition popuptransition = new Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition();
            popup_GetPlaylistName.ChildTransitions.Add(popuptransition);
            popup_GetPlaylistName.IsOpen = true;
        }
        #endregion

        #region VOLUME CONTROL
        private bool _volumeChangedByStatus = true;
        private void sl_Volume_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            var sl = sender as Slider;
            if (!_volumeChangedByStatus && _server.StatusData.State == "play")
                _server.SetVolume((int)sl.Value);
            _volumeChangedByStatus = false;
        }

        #endregion

        #endregion
  
        private void textblock_MainMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
             if (popup_MainMenu.IsOpen)
                popup_MainMenu.IsOpen = false;
            else
                popup_MainMenu.IsOpen = true;
        }

        private void appbtn_Volume_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var appbarbutton = sender as AppBarButton;
            var ttv = appbarbutton.TransformToVisual(this);
            Point screenCoords = ttv.TransformPoint(new Point(15, -340));

            popup_VolumeControl.HorizontalOffset = screenCoords.X;
            popup_VolumeControl.VerticalOffset = screenCoords.Y;

            if (popup_VolumeControl.IsOpen)
                popup_VolumeControl.IsOpen = false;
            else
                popup_VolumeControl.IsOpen = true;
        }

        private void pb_Progress_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp =  e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta/24; // считаем что каждый клик смещает позицию в треке на 5 сек

            _currentTrackPosition = pb_Progress.Value;
            if (((sl.Value + mouseweeldelta) > 0) && ((sl.Value + mouseweeldelta) < sl.Maximum))
            {
                string soffset = mouseweeldelta.ToString(CultureInfo.InvariantCulture);

                if (mouseweeldelta > 0)
                    soffset = "+" + mouseweeldelta.ToString(CultureInfo.InvariantCulture);
                _server.SeekCurrent(soffset);
            }

        }

        private void pb_Progress_Tapped(object sender, TappedRoutedEventArgs e)
        {

            var sl = sender as Slider;
            _currentTrackPosition = pb_Progress.Value;
            double offset = sl.Value - _currentTrackPosition;

            string soffset = offset.ToString(CultureInfo.InvariantCulture);
            if (offset > 0)
                soffset = "+" + soffset;
            _server.SeekCurrent(soffset);
            _seekBarIsBeingDragged = false;
        }

        private void sl_Volume_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp = e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta / 12; // считаем что каждый клик смещает позицию в треке на 5 сек

            int newvalue = (int)sl.Value + mouseweeldelta;
            if ((newvalue >= 0) && (newvalue<=100))
            {
                _server.SetVolume(newvalue);
            }
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

        private async Task<MsgBoxButton> DisplayMessage(DisplayMessageEventArgs messageArgs)
        {
            MsgBox.SetButtons(messageArgs.Buttons);
            MsgBox.Message = messageArgs.Message;
            MsgBox.BoxHeight = messageArgs.BoxHeight;
            return await MsgBox.Show();
        }
    }
}

  