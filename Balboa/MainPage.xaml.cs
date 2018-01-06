/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 *
 *
 * Главная страница приложения
 *
 --------------------------------------------------------------------------*/

using Balboa.Common;
using System;
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
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.Graphics.Display;
using System.Text.RegularExpressions;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Balboa
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private enum NewPlaylistNameRequestMode { SaveNewPlaylist, RenamePlaylist };

        private NavigationHelper navigationHelper;
  
         /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public AppSettings      AppSettings
        {
            get { return this._appSettings; }
        }

        private ListViewItem    _listViewItemGotFocus;
        private GridViewItem    _gridViewItemGotFocus;
        private List<string>    _currentFilePath = new List<string>();
        private string          _parentfolder = "";
        private string          _currentPlaylistName = "New playlist";
        private string          _oldPlaylistName;
        private string          _newPlaylistName;

        NewPlaylistNameRequestMode _requestNewPlaylistNameMode;

        private AppSettings     _appSettings = new AppSettings();
  
        private Server Server;

        private enum DataPanelState {CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings} 
        private enum ApplicationStates { Default, Filled, Narrow }

//        private ApplicationStates _applicationState = ApplicationStates.Default;
      
        // Конструктор
        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            this.SizeChanged += MainPage_SizeChanged;
           
            Application.Current.Suspending += new SuspendingEventHandler(OnSuspending);
            Application.Current.Resuming += new EventHandler<Object>(OnResuming);
                     
            Server = new Server(this);
            Server.ConnectionStatusChanged += OnServerConnectionStatusChanged;
            Server.Error += OnServerError;
            Server.CriticalError += OnServerCriticalError;
            Server.StatusData.PropertyChanged += OnStatusDataPropertyChanged;
            Server.CurrentSongData.PropertyChanged += OnCurrentSongDataPropertyChanged;
            Server.CommandCompleted += OnCommandCompleted;
            Server.OutputsData.CollectionChanged += OnOutputsCollectionChanged;

            Server.DirectoryData.CollectionChanged += OnFilelistChanged;

            // Установка контекстов для databinding

            stp_ServerStatus.DataContext = Server.StatusData;
            stackpanel_MainPanelHeader.DataContext = Server.CurrentSongData;
            tb_ServerStatus.DataContext = Server.StatusData;

            tb_ConnectionStatus.DataContext = Server;
            tb_Remain.DataContext = Server.StatusData;
            stp_MainMenu.DataContext = Server.StatusData;

            gr_Stats.DataContext            = Server.StatisticData;
            gr_CurrentTrack.DataContext     = Server.CurrentSongData;
            lv_PlayList.ItemsSource         = Server.PlaylistData;
            gr_FileSystemContent.ItemsSource = Server.DirectoryData;
            gr_SavedPlaylistsContent.ItemsSource = Server.SavedPlaylistsData;

            listview_Genres.ItemsSource = Server.Genres;

            listview_Arists.ItemsSource = Server.Artists;
            listview_Albums.ItemsSource = Server.Albums;
            listview_Tracks.ItemsSource = Server.Tracks;

            listview_GenreArists.ItemsSource = Server.Artists;
            listview_GenreAlbums.ItemsSource = Server.Albums;
            listview_GenreTracks.ItemsSource = Server.Tracks;
            listview_Search.ItemsSource = Server.Tracks;

            _appSettings.Restore();

            if (_appSettings.InitialSetupDone)
            {
                Server.Host                     = _appSettings.Server;
                Server.Port                     = _appSettings.Port;
                Server.Password                 = _appSettings.Password;
                Server.ViewUpdateInterval       = _appSettings.ViewUpdateInterval;
                Server.MusicCollectionFolder    = _appSettings.MusicCollectionFolder;
                Server.AlbumCoverFileNames      = _appSettings.AlbumCoverFileName;
                Server.DisplayFolderPictures    = _appSettings.DisplayFolderPictures;
       
              
       

        Server.Start();         // Запускаем сеанс взаимодействия с MPD

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
                //textbox_WindowWidth.Text = e.NewSize.Width.ToString();
                if (e.NewSize.Width >= 1100)
                {
//                    _applicationState = ApplicationStates.Default;
                    VisualStateManager.GoToState(this, "Default", true);
                }

                if (e.NewSize.Width < 1100)
                {
                    // Прячем горизонтальный регулятор громкости
                    // Показываем кнопку регулятора громкости
                    // Меняем размер колонки 1 в панели управления воспроизведения grid_PlayControls.ColumnDefinitions[1];
                    // (меняем 300 на 20*
//                    _applicationState = ApplicationStates.Filled;
                    VisualStateManager.GoToState(this, "Filled", true);
                }

                if (e.NewSize.Width < 900)
                {
//                    _applicationState = ApplicationStates.Narrow;
                    VisualStateManager.GoToState(this, "Narrow", true);
                }

                if (e.NewSize.Width < 620)
                {
//                    _applicationState = ApplicationStates.Narrow;
                    VisualStateManager.GoToState(this, "SuperNarrow", true);
                }
            }

            if (displayinformation.CurrentOrientation == DisplayOrientations.Portrait || displayinformation.CurrentOrientation == DisplayOrientations.PortraitFlipped)
            {
                VisualStateManager.GoToState(this, "Portrait", true); 
            }

      
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            await Server.Halt();
        }

        private void OnResuming(Object sender, Object e)
        {
           Server.Start();
        }

        #region Обработчики событий объекта Server

        private void OnStatusDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName=="TimeElapsed" && !_seekBarIsBeingDragged)
            { 
                pb_Progress.Value = Server.StatusData.TimeElapsed;
                
            }
            if (e.PropertyName == "SongId") // Произошёл переход к следующему треку
            { 
                // Запрашиваем у сервера данные проигрываемого трека
                Server.CurrentSong();
            }
            
            if (e.PropertyName == "PlaylistId")  // Изменился PlaylistId, читаем заново Playlist  
            {
                Server.PlaylistInfo();
            }

            if (e.PropertyName == "State")
            {
                SetPlaybuttonPicture(Server.StatusData.State);
                if (Server.StatusData.State == "stop")
                {
                    if (stackpanel_MainPanelHeader.Opacity != 0)
                        stackpanel_MainPanelHeaderHideStoryboard.Begin();
                }
                else
                {
                    if (stackpanel_MainPanelHeader.Opacity == 0)
                        stackpanel_MainPanelHeaderShowStoryboard.Begin();
                }
            }

            if (e.PropertyName == "Volume")
            {
                volumechangedbystatus = true;
                sl_Volume.Value = Server.StatusData.Volume;
                sl_VerticalVolume.Value = Server.StatusData.Volume;
            }

        }

        /// <summary>
        /// Устанавливает свойство IsPlaying для текущего проигрываемого трека равным true
        ///  и для предыдущего трка равным false
        /// В зависимости от значения свойства устанавливается стиль отображения трека в Playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCurrentSongDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Id")
            {
                Server.StatusData.Duration = Server.CurrentSongData.Duration;
                pb_Progress.Maximum = Server.CurrentSongData.Duration;
                HightlightCurrentPlaingTrack();

                if (_appSettings.MusicCollectionFolderToken.Length == 0)
                    return;
                
                 string s = Utils.ExtractFilePath(Server.CurrentSongData.File);

                image_AlbumCover.Source = image_AlbumCoverSmall.Source = await Utils.GetFolderImage(_appSettings.MusicCollectionFolder,s, _appSettings.AlbumCoverFileName);
            }
        }

        private       void OnCommandCompleted(object sender, ServerCommandCompletionEventArgs args)
        {
            switch (args.Command)
            {
                case "load":
                    if (args.Result == "OK")
                    { 
                    tbk_PlaylistName.Text = _currentPlaylistName;
                    NotifyUser("Load playlist", "Playlist " + _currentPlaylistName + " loaded succesfully");
                    }
                    else
                    {
                        NotifyUser("", "Error loading playlist" + _currentPlaylistName + "\n"+ args.Message);
                    }
                    break;
                case "search":
                    if (Server.Tracks.Count == 0)
                        textblock_SearchResult.Text = "Serching complete.\n\nNothing found.";
                    else
                        textblock_SearchResult.Text = "";
                    break;

                case "playlistinfo":
                    if (Server.PlaylistData.Count == 0)
                        textblock_PlaylistContent.Text = "Playlist is empty";
                    else
                    { 
                        textblock_PlaylistContent.Text = "";
                        if (playlastaddedtrack)
                        {
                            var track = lv_PlayList.Items[lv_PlayList.Items.Count-1] as Track;
                            Server.PlayId(track.Id);
                            playlastaddedtrack = false;
                        }
                        HightlightCurrentPlaingTrack();
                    }
                    
                    break;
                 default: break;
            }

        }

        private       void OnOutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            stp_Outputs.Children.Clear();

            foreach (Output output in Server.OutputsData)
            {
                ToggleSwitch ts = new ToggleSwitch();
                ts.Style = Resources["ToggleSwitchStyle"] as Style;
                
                ts.Name = output.ID.ToString();
                ts.Header = output.Name;
                ts.IsOn = output.Enabled;
                ts.Width = 300;
                ts.Toggled += ts_Output_Switched;

                stp_Outputs.Children.Add(ts);
            }
        }

        private       void OnFilelistChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string currentpath = Utils.BuildFilePath(_currentFilePath);

            StringBuilder foldername = new StringBuilder(Utils.BuildFilePath(_currentFilePath));
            if (foldername.Length > 0)
                foldername.Append("\\");
 
            string[] AlbumCoverFileNames = _appSettings.AlbumCoverFileName.Split(';');
            // При переходе на уровень вверх по файловой системе подсветим 
            //последний открывавшийся фолдер и прокрутим Grid так чтобы он был виден
            foreach (File item in Server.DirectoryData)
            {
                if (item.Name.ToLower() == _parentfolder.ToLower())
                {
                    item.JustClosed = true;
                    gr_FileSystemContent.ScrollIntoView(item, ScrollIntoViewAlignment.Default);
                    break;
                }
            }
        }

        private async void OnServerError(object sender, ServerEventArgs e)
        {
            await Server.Halt();

            MessageBoxButtons responce = await MessageBox("Error", e.Message, MessageBoxButtons.GoToSettings_Retry_Exit);
            if (responce == MessageBoxButtons.Exit)
            {
                 Application.Current.Exit();
            }
            if (responce == MessageBoxButtons.Continue)
            {
                 Server.Start();
            }
            if (responce == MessageBoxButtons.GoToSettings)
            {
                SwitchDataPanelsTo(DataPanelState.Settings);
             }
        }

        private async void OnServerCriticalError(object sender, ServerEventArgs e)
        {
            MessageBoxButtons responce = await MessageBox("Error", e.Message, MessageBoxButtons.GoToSettings_Retry_Exit);
                if (responce == MessageBoxButtons.Exit)
            {
                await Server.Halt();
                Application.Current.Exit();
            }
            if (responce == MessageBoxButtons.Continue)
            {
               await Server.Restart();
            }
            if (responce == MessageBoxButtons.GoToSettings)
            {
                SwitchDataPanelsTo(DataPanelState.Settings);
                //await Server.Restart();
            }

        }

        private       void OnServerConnectionStatusChanged(object sender, ServerConnectionStatusEventArgs e)
        {
            if(e.Status == ConnectionStatus.Connected)
                Server.PlaylistInfo();
            
        }

        #endregion

        private async void NotifyUser(string title, string message)
        {
            object cmdid= new object();
            MessageDialog md = new Windows.UI.Popups.MessageDialog(message, title);
            //md.Commands.Add(new UICommand("Continue (not recommended)",null, 1));
            md.Commands.Add(new UICommand("Close", null, 2));
            var selected = await md.ShowAsync();
            return;
        }

    
        #region Navigation helpers

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

            


        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        #endregion


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

            string soffset = offset.ToString();
            if (offset >0)
                 soffset = "+"+soffset;
            Server.SeekCurrent(soffset);
            _seekBarIsBeingDragged = false;
        }
       
        #endregion

        #region Обработка событий интерфейса

        #region УПРАВЛЕНИЕ ВОСПРИЗВЕДЕНИЕМ
        private void img_Volume_Tapped(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btn_PrevTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Previous();
        }

        private void btn_PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // "&#xE102;" - Play
            // "&#xE103;" - Pause
            Button button = (Button)sender;
            if (Server.StatusData.State == "stop")
            {
                Server.Play();
                // Устанавливаем символ "Pause"
                button.Content = '\xE103';
            }
            if (Server.StatusData.State == "pause")
            {
                Server.Pause();    // Если статус сервера "pause" то команда Pause() запускает проигрывание
                // Устанавливаем символ "Pause"
                button.Content = '\xE103';
            }
            if (Server.StatusData.State == "play")
            {
                Server.Pause();    // Если статус сервера "play" то команда Pause() прерывает проигрывание
                // Устанавливаем символ "Play"
                button.Content = '\xE102';  //57602
            }


            bool playedtrackselected = false; // Устанавливаем пизнак того что проигрываемый трек не подсвечен
            // Ищем в Playlist трек с установленным признаком IsPlaying
            // и если такой трек находится то прокручиваем Playlist так чтобы трек быд виден
            foreach (Track item in Server.PlaylistData)
            {
                if (item.IsPlaying)
                {
                    playedtrackselected = true;
                    lv_PlayList.ScrollIntoView(item);
                    return;
                }
            }
            // Если трек с признаком IsPlaying не найден и Playlist не пуст устанавливаем признак проигрываемого трека на первый элемент в Playlist
            if (!playedtrackselected && Server.PlaylistData.Count>0)
            {
                Server.PlaylistData[0].IsPlaying = true;
            }
         }

        private void btn_NextTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Next();
        }

        private void btn_Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Stop();
        }
    
        #endregion

        #region MAIN MENU

        private void ts_Random_Toggled(object sender, RoutedEventArgs e)
        {
            Server.Random(((ToggleSwitch)(sender)).IsOn);
        }

        private void ts_Repeat_Toggled(object sender, RoutedEventArgs e)
        {
            Server.Repeat(((ToggleSwitch)(sender)).IsOn);
        }

        private void ts_Consume_Toggled(object sender, RoutedEventArgs e)
        {
            Server.Consume(((ToggleSwitch)(sender)).IsOn);
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
    
            if (Server.Genres.Count == 0)
                Server.List("genre");

            Server.Artists.ClearAndNotify();
            Server.Albums.ClearAndNotify();
            Server.Tracks.ClearAndNotify();
         }

        private void listview_Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListviewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                Server.Artists.ClearAndNotify();
                Server.Albums.ClearAndNotify();
                Server.Tracks.ClearAndNotify();
                Server.List("artist", "genre", si.Name);
            }
        }

        private async void appbtn_Genre_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_GenreTracks.SelectedItems.Count > 0)
            {
                foreach (Track track in listview_GenreTracks.SelectedItems)
                    Server.Add(track.File);
            }
            else
            {
                if (listview_GenreAlbums.SelectedItems.Count > 0)
                {
                    var si = listview_GenreAlbums.SelectedItem as CommonGridItem;
                    if (si!=null)
                        Server.SearchAdd("album", si.Name);
                }
                else
                {
                    if (listview_GenreArists.SelectedItems.Count > 0)
                    {
                        var si = listview_GenreArists.SelectedItem as CommonGridItem;
                        if (si != null)
                            Server.SearchAdd("artist", si.Name);
                    }
                    else
                    {
                        if (listview_Genres.SelectedItems.Count > 0)
                        {
                            var si = listview_Genres.SelectedItem as CommonGridItem;
                            if (si != null)
                                Server.SearchAdd("genre", si.Name);
                        }
                        else
                        {
                            await MessageBox("Adding track to playlist", "No selected items to add.", MessageBoxButtons.Continue);
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
            textbox.Text = "Type genre name here";
        }

        private void textbox_GenreSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox_GenreSearch.Text.Length > 0)
            {
                int genrescount = listview_Genres.Items.Count;
                for (int i = 0; i < genrescount; i++)
                {
                    var genre = listview_Genres.Items[i] as CommonGridItem;
                    if (genre.Name.ToLower().StartsWith(textbox_GenreSearch.Text.ToLower()))
                    {
                        var lastgenre = listview_Genres.Items[genrescount - 1];// as CommonGridItem;
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
            Server.Albums.ClearAndNotify();
            Server.Tracks.ClearAndNotify();
            Server.List("artist");
         }
      

        private void listview_Arists_Tapped(object sender, TappedRoutedEventArgs e)
        {

            var lv = sender as ListviewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si!=null)
            {
                Server.Albums.ClearAndNotify();
                Server.Tracks.ClearAndNotify(); 
                Server.List("album", "artist", si.Name);
            }

        }

        private void listview_Albums_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var lv = sender as ListviewExtended;
            var si = lv.SelectedItem as CommonGridItem;
            if (si != null)
            {
                Server.Tracks.ClearAndNotify();
                Server.Search("album", si.Name);
            }

        }

        private async void appbtn_Artist_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_Tracks.SelectedItems.Count>0)
            {
                foreach (Track track in listview_Tracks.SelectedItems)
                    Server.Add(track.File);
            }
            else
            {
                if (listview_Albums.SelectedItems.Count > 0)
                {
                    var si = listview_Albums.SelectedItem as CommonGridItem;
                    Server.SearchAdd("album", si.Name);
                }
                else
                {
                    if (listview_Arists.SelectedItems.Count > 0)
                    {
                        var si = listview_Arists.SelectedItem as CommonGridItem;
                        Server.SearchAdd("artist", si.Name);
                    }
                    else
                    {
                        await MessageBox("Adding track to playlist", "No selected items to add.", MessageBoxButtons.Continue);
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
                    if (artist.Name.ToLower().StartsWith(textbox_ArtistSearch.Text.ToLower()))
                    {
                        var lastartist = (CommonGridItem)(listview_Arists.Items[listview_Arists.Items.Count-1]);
                        var firstartist = (CommonGridItem)(listview_Arists.Items[0]);
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
            textbox.Text = "Type artist name here";
        }
        #endregion

        #region SETTINGS
        private void btn_Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(Server.IsConnected)
            {
                // Прочитаем список доступных выходов и добавим их в список параметров
                Server.Outputs();
            }
            // Восстановим значения на экранной форме Settings
            tbx_Server.Text              = _appSettings.Server;
            tbx_Port.Text                = _appSettings.Port;
            tbx_Password.Text            = _appSettings.Password;
            tbx_ViewUpdateInterval.Text  = _appSettings.ViewUpdateInterval.ToString();
            tbx_MusicCollectionPath.Text = _appSettings.MusicCollectionFolder;
            tbx_AlbumCoverFileName.Text  = _appSettings.AlbumCoverFileName;
            checkbox_DisplayFolderPictures.IsChecked = _appSettings.DisplayFolderPictures;

            SwitchDataPanelsTo(DataPanelState.Settings);
 
        }

        private async void appbtn_SaveSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string notNumber = @"\D{1,}";

            _appSettings.Server = tbx_Server.Text;
            _appSettings.Port = tbx_Port.Text;
            _appSettings.Password = tbx_Password.Text;
            _appSettings.MusicCollectionFolder = tbx_MusicCollectionPath.Text;
            _appSettings.AlbumCoverFileName = tbx_AlbumCoverFileName.Text;
            _appSettings.DisplayFolderPictures = (bool)checkbox_DisplayFolderPictures.IsChecked;

            if (Regex.IsMatch(tbx_Port.Text, notNumber) || tbx_Port.Text.Length == 0)
            {
                await MessageBox("Eror", "'Port' value must be number", MessageBoxButtons.Continue);
                return;
             }
             else
             {
                int port = int.Parse(tbx_Port.Text);
                if (port > 65535)
                {
                    await MessageBox("Eror", "'Port' value must be less then 65536", MessageBoxButtons.Continue);
                    return;
                }
                _appSettings.Port = tbx_Port.Text;
             }

             if (Regex.IsMatch(tbx_ViewUpdateInterval.Text, notNumber) || tbx_ViewUpdateInterval.Text.Length==0)
             {
                await MessageBox("Eror", "'View update interval' value must be number", MessageBoxButtons.Continue);
                return;
             }
             else
             {
                int updateinterval = int.Parse(tbx_ViewUpdateInterval.Text);
                if (updateinterval < 100)
                {
                    await MessageBox("Eror", "View update interval must be 100 or more", MessageBoxButtons.Continue);
                    return;
                }
                _appSettings.ViewUpdateInterval = updateinterval;
              }


              if (!_appSettings.SettingsChanged)
                    return;

              Connection connection = new Connection();

              bool connectionresult = await connection.Open(tbx_Server.Text, tbx_Port.Text, tbx_Password.Text);
              connection.Close();

              if (connectionresult)
              {
                  if (_appSettings.ServerNameChanged && (!_appSettings.MusicCollectionFolderTokenChanged))
                  {
                      await MessageBox("Warning", "Server name changed without changing 'Path to music collection'\n  'Path to music collection' will be creared", MessageBoxButtons.Continue);
                      tbx_MusicCollectionPath.Text = String.Empty;
                      StorageApplicationPermissions.FutureAccessList.Clear();
                      _appSettings.MusicCollectionFolderToken = String.Empty;
                      _appSettings.MusicCollectionFolder = String.Empty;
                  }
                  _appSettings.Save();

                  Server.Host = _appSettings.Server;
                  Server.Port = _appSettings.Port;
                  Server.Password = _appSettings.Password;
                  Server.ViewUpdateInterval = _appSettings.ViewUpdateInterval;
                  Server.MusicCollectionFolder = _appSettings.MusicCollectionFolder;
                  Server.AlbumCoverFileNames = _appSettings.AlbumCoverFileName;
                  Server.DisplayFolderPictures = _appSettings.DisplayFolderPictures;

                await Server.Restart();
                }
                else
                {
                    MessageBoxButtons b = await MessageBox("Connection error", "\n" + connection.Error + "\n\nConnection parameters does not changed", MessageBoxButtons.OK);
                }
        }

        private void ts_Output_Switched(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            if (ts.IsOn)
                Server.EnableOutput(int.Parse(ts.Name));
            else
                Server.DisableOutput(int.Parse(ts.Name));
        }

        private async void appbtn_TestConnection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await TestConnection();
        }

        private async Task TestConnection()
        {
            string message = string.Empty;

            Connection connection = new Connection();

            bool connectionresult = await connection.Open(tbx_Server.Text, tbx_Port.Text, tbx_Password.Text);
            if (connectionresult)
            {
                connection.Close();
                message = "Connected succesfully\n" + connection.InitialResponce;
            }
            else
            {
                message = "Connection error\n" + connection.Error;
            }

            MessageBoxButtons b = await MessageBox("Connection test", message, MessageBoxButtons.OK);

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
        #endregion

        #region CURRENT PLAYLIST COMMANDS --------------------------------------------------

        private void btn_Playlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
            // Run the PopInThemeAnimation 
            //Windows.UI.Xaml.Media.Animation.Storyboard sb = this.FindName("PopInStoryboard") as Windows.UI.Xaml.Media.Animation.Storyboard;
            //if (sb != null) sb.Begin();

        }

        private void lv_PlayList_GotFocus(object sender, RoutedEventArgs e)
        {
            _listViewItemGotFocus = e.OriginalSource as ListViewItem;
            var lv = sender as ListviewExtended;
         }

        private void lv_PlayList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Track playlistitem = _listViewItemGotFocus.Content as Track;
            Server.PlayId(playlistitem.Id);


            int index = lv_PlayList.SelectedItems.IndexOf(playlistitem);
            if (index>=0)
                lv_PlayList.SelectedItems.RemoveAt(index);

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
                Server.DeleteId(track.Id);
            }
        }

        private void appbtn_Playlist_Clear_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Clear();
        }

        private bool EnteredFromPlaylistMode = false;
        private void appbtn_Playlist_Add_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EnteredFromPlaylistMode = true;
            btn_FileSystem_Tapped(sender, e);
        }

        private void appbtn_Playlist_Shaffle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Shuffle();
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

                Server.Save(str);
            }

            if (_requestNewPlaylistNameMode == NewPlaylistNameRequestMode.RenamePlaylist)
            {
                _newPlaylistName = tbx_PlaylistName.Text;
                Server.Rename(_oldPlaylistName, _newPlaylistName);
            }

            Server.ListPlaylists();
            popup_GetPlaylistName.IsOpen = false;
        }

        private void btn_PlaylistNameCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Close it all down
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

            Server.CurrentFolder = String.Empty;

            SwitchDataPanelsTo(DataPanelState.FileSystem);
            Server.LsInfo();
        }

        private void gr_FileSystemContent_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_gridViewItemGotFocus == null) return;

            File currentfileitem = _gridViewItemGotFocus.Content as File;
            if (currentfileitem.Type == FileType.Directory)
            {
                _currentFilePath.Add(currentfileitem.Name.Trim());
                Server.CurrentFolder = Utils.BuildFilePath(_currentFilePath);
                Server.LsInfo(Server.CurrentFolder);
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
          
            Server.CurrentFolder = Utils.BuildFilePath(_currentFilePath);
            Server.LsInfo(Server.CurrentFolder);

            // Eсли мы поднялись на самый верх по дереву каталогов отключим кнопку Up
            if (_currentFilePath.Count > 0)
                appbtn_Up.IsEnabled = true;
            else
                appbtn_Up.IsEnabled = false;

        }

        private void appbtn_RescanDatabase_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Server.Update();
        }

        private async void appbtn_AddFromFileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string path = string.Empty;
            if (_currentFilePath.Count>0)
                path = Utils.BuildFilePath(_currentFilePath)+ "/";
            if (gr_FileSystemContent.SelectedItems.Count > 0)
            {
                foreach (File item in gr_FileSystemContent.SelectedItems)
                {
                    string lp = path + item.Name;
                    Server.Add(path + item.Name);
                }
                if (EnteredFromPlaylistMode)
                {
                    // возвращаемся в Playlist
                    SwitchDataPanelsTo(DataPanelState.CurrentPlaylist);
                }
            }
            else
            {
               await MessageBox("Adding track to playlist", "No selected items to add.", MessageBoxButtons.Continue);
            }
        }


        #endregion --------------------------------------------------------------

        #region SAVED PLAYLISTS COMAND
        private void btn_SavedPlayLists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Playlists);
  
            Server.ListPlaylists();
        }
      
        private async void appbtn_SavedPlaylistLoad_Click(object sender, RoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi != null)
            {
                _currentPlaylistName = fi.Name;
                Server.Load(fi.Name);
            }
            else
            {
                await MessageBox("Playlist loading", "Playlist does not selected.", MessageBoxButtons.Continue);
            }
        }

        private async void appbtn_SavedPlaylistDelete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi != null)
            {
                Server.Rm(fi.Name);
                Server.ListPlaylists();
            }
            else
            {
                await MessageBox("Deleting playlist", "Playlist does not selected.", MessageBoxButtons.Continue);
            }

        }

        private async void appbtn_SavedPlaylistRename_Tapped(object sender, TappedRoutedEventArgs e)
        {
            File fi = gr_SavedPlaylistsContent.SelectedItem as File;
            if (fi != null)
            {
                _oldPlaylistName = fi.Name;
                tbx_PlaylistName.Text = _oldPlaylistName;
                _requestNewPlaylistNameMode = NewPlaylistNameRequestMode.RenamePlaylist;
                RequestNewPlaylistName();
            }
            else
            {
               await MessageBox("Renaming playlist", "Playlist does not selected.", MessageBoxButtons.Continue);
            }
        }
        #endregion

        #region STATS COMMANDS

        private void btn_Stats_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Statistic);
            Server.Stats();
        }
        #endregion

        #region SEARCH COMMANDS

        private void btn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(DataPanelState.Search);
            textblock_SearchResult.Text = "";
        }

        private void appbtn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (textbox_Search.Text.Length > 0)
            {
                textblock_SearchResult.Text = "Searching ...";
                Server.Tracks.ClearAndNotify();
                Server.Search("any", textbox_Search.Text);
            }
        }

        private async void appbtn_Search_AddToPaylist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listview_Search.SelectedItems.Count > 0)
            {
                foreach (Track track in listview_Search.SelectedItems)
                    Server.Add(track.File);
            }
           else
            {
                await MessageBox("Adding track to playlist", "No selected items to add.", MessageBoxButtons.Continue);
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
            Server.Add(track.File);
            playlastaddedtrack = true;
         }
        #endregion

        #region UTILS
        private void HightlightCurrentPlaingTrack()
        {
            foreach (Track item in lv_PlayList.Items)
            {
                if (item.Id == Server.CurrentSongData.Id)
                {
                    item.IsPlaying = true;
                    lv_PlayList.ScrollIntoView(item);
                    int index = lv_PlayList.SelectedItems.IndexOf(item);
                    if (index >= 0)
                        lv_PlayList.SelectedItems.RemoveAt(index);
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
                if (uielement is Grid)
                    uielement.Visibility = Visibility.Collapsed;
            }
            // Изменим цвет текста во всех кнопках главного меню на белый
            foreach (UIElement uielement in stp_MainMenu.Children)
            {
                if (uielement is Button)
                {
                    var btn = uielement as Button;
                    btn.Foreground = WhiteBrush;
                }
            }

            /*
            DoubleAnimation da = new DoubleAnimation();

            da.From = 0.0;
            da.To = 1.0;
            da.Duration = TimeSpan.FromSeconds(2);

            Storyboard sb = new Storyboard();
            sb.Children.Add(da);
            */
            // Включим нужную панель
            // Изменим цвет текста в соответствующей кнопке на оранжевый
            // Установим режим в заголовке окна

            switch (state)
            {
                case DataPanelState.Artists:
                    btn_Artists.Foreground = OrangeBrush;
                    gr_Artists.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Artists";
                    gr_ArtistsShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentPlaylist:
                    btn_Playlist.Foreground = OrangeBrush;
                    gr_Playlist.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Current playlist";
                    gr_CurrentPlaylistShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentTrack:
                    btn_CurrentTrack.Foreground = OrangeBrush;
                    gr_CurrentTrack.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Current track";
                    gr_CurrentTrackShowStoryboard.Begin();
                    break;
                case DataPanelState.FileSystem:
                    btn_FileSystem.Foreground = OrangeBrush;
                    gr_FileSystem.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Files & folders";
                    gr_FileSystemShowStoryboard.Begin();
                    break;
                case DataPanelState.Genres:
                    btn_Genres.Foreground = OrangeBrush;
                    gr_Genres.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Genres";
                    gr_GenresShowStoryboard.Begin();
                    break;
                case DataPanelState.Playlists:
                    btn_SavedPlayLists.Foreground = OrangeBrush;
                    gr_SavedPlayLists.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Playlist";
                    gr_SavedPlayListsShowStoryboard.Begin();
                    break;
                case DataPanelState.Search:
                    btn_Search.Foreground = OrangeBrush;
                    gr_Search.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Search";
                    gr_SearchShowStoryboard.Begin();
                    break;
                case DataPanelState.Settings:
                    btn_Settings.Foreground = OrangeBrush;
                    gr_Settings.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Settings";
                    gr_SettingsShowStoryboard.Begin();
                    break;
                case DataPanelState.Statistic:
                    btn_Stats.Foreground = OrangeBrush;
                    gr_Stats.Visibility = Visibility.Visible;
                    textbox_CurrentMode.Text = "Statistic";
                    gr_StatsShowStoryboard.Begin();
                         break;
                    //            sb.SetValue(Storyboard.TargetPropertyProperty, "Opacity");
                    //            sb.SetValue(Storyboard.TargetNameProperty, "gr_Genres");
                    //            sb.Begin();
             }
            if (Server.StatusData.State != "stop")
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
            //
            //sb.SetValue(Opacity, 0.0);
            //Storyboard.TargetName = "gr_CurrentTrack"
            //                              Storyboard.TargetProperty = "Opacity"
            //                              From = "0.0" To = "1.0" Duration = "0:0:2" />

                // весь код в switch можно упростить используя механизм из следующей строки
                //Windows.UI.Xaml.Media.Animation.Storyboard sb = this.FindName("PopInStoryboard") as Windows.UI.Xaml.Media.Animation.Storyboard;

        }

        private enum MessageBoxButtons { OK,Cancel, Continue, Retry, OK_Cancel, Retry_Cancel, Continue_Exit, Exit, GoToSettings_Retry_Exit, GoToSettings }

        private async Task<MessageBoxButtons> MessageBox(string title, string message, MessageBoxButtons buttons)
        {
            
            object cmdid = new object();
            MessageDialog md = new MessageDialog(message, title);
            
            switch(buttons)
            {
                case MessageBoxButtons.OK:
                        md.Commands.Add(new UICommand("OK", null, MessageBoxButtons.OK));
                        break;
                case MessageBoxButtons.Cancel:
                        md.Commands.Add(new UICommand("Cancel", null, MessageBoxButtons.Cancel));
                        break;
                case MessageBoxButtons.Continue:
                        md.Commands.Add(new UICommand("Continue", null, MessageBoxButtons.Continue));
                        break;
                case MessageBoxButtons.Retry:
                        md.Commands.Add(new UICommand("Retry", null, MessageBoxButtons.Retry));
                        break;
                case MessageBoxButtons.OK_Cancel:
                        md.Commands.Add(new UICommand("OK", null, MessageBoxButtons.OK));
                        md.Commands.Add(new UICommand("Cancel", null, MessageBoxButtons.Cancel));
                        break;
                case MessageBoxButtons.Retry_Cancel:
                        md.Commands.Add(new UICommand("Retry", null, MessageBoxButtons.Retry));
                        md.Commands.Add(new UICommand("Cancel", null, MessageBoxButtons.Cancel));
                        break;
                case MessageBoxButtons.Continue_Exit:
                        md.Commands.Add(new UICommand("Continue", null, MessageBoxButtons.Continue));
                        md.Commands.Add(new UICommand("Close application", null, MessageBoxButtons.Exit));
                        break;
                case MessageBoxButtons.GoToSettings_Retry_Exit:
                        md.Commands.Add(new UICommand("Go to Settins", null, MessageBoxButtons.GoToSettings));
                        md.Commands.Add(new UICommand("Retry", null, MessageBoxButtons.Continue));
                        md.Commands.Add(new UICommand("Close application", null, MessageBoxButtons.Exit));
                        break;
            }
            UICommand selected = (UICommand) await md.ShowAsync();
            return (MessageBoxButtons) selected.Id;
        }
        
        private void SetPlaybuttonPicture(string state)
        {
            // "&#xE102;" - Play
            // "&#xE103;" - Pause

            if (state == "play")
            {
                appbtn_PlayPause.Content = '\xE103'; // Устанавливаем символ "Pause"
            }
            else
            {    
                appbtn_PlayPause.Content = '\xE102'; // Устанавливаем символ "Play"
            }
        }

        private void RequestNewPlaylistName()
        { 
        //First we need to find out how big our window is, so we can center to it.
        CoreWindow currentWindow = Window.Current.CoreWindow;
        //Set our background rectangle to fill the entire window
            //rectBackgroundHide.Height = currentWindow.Bounds.Height;
            //rectBackgroundHide.Width = currentWindow.Bounds.Width;
            //rectBackgroundHide.Margin = new Thickness(0, 0, 0, 0);
        //Make sure the background is visible
            //rectBackgroundHide.Visibility = Visibility.Visible;
            //Now we figure out where the center of the screen is, and we 
            //move the popup to that location.
            popup_GetPlaylistName.HorizontalOffset = (currentWindow.Bounds.Width / 2) - (400 / 2);
            popup_GetPlaylistName.VerticalOffset = (currentWindow.Bounds.Height / 2) - (150 / 2);

            //AddDeleteThemeTransition
            Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition popuptransition = new Windows.UI.Xaml.Media.Animation.AddDeleteThemeTransition();
            //popuptransition..Edge = Windows.UI.Xaml.Controls.Primitives.EdgeTransitionLocation.Top;
            popup_GetPlaylistName.ChildTransitions.Add(popuptransition);

            popup_GetPlaylistName.IsOpen = true;
            
           // tbx_PlaylistName.Text = _currentPlaylistName;
            //Presto!  We have a centered popup.
        }
        #endregion

        #region VOLUME CONTROL
        private bool volumechangedbystatus = true;
        private void sl_Volume_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            var sl = sender as Slider;
            if (!volumechangedbystatus && Server.StatusData.State == "play")
                Server.SetVol((int)sl.Value);
            volumechangedbystatus = false;
        }

        #endregion

        #endregion
  
        private void textblock_Title_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //myStoryboard.Begin();
        }

 
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

            //var ttv = appbarbutton.TransformToVisual(Window.Current.Content);
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
                string soffset = mouseweeldelta.ToString();

                if (mouseweeldelta > 0)
                    soffset = "+" + mouseweeldelta.ToString();
                Server.SeekCurrent(soffset);
            }

        }

        private void pb_Progress_Tapped(object sender, TappedRoutedEventArgs e)
        {

            var sl = sender as Slider;
            _currentTrackPosition = pb_Progress.Value;
            double offset = sl.Value - _currentTrackPosition;

            string soffset = offset.ToString();
            if (offset > 0)
                soffset = "+" + soffset;
            Server.SeekCurrent(soffset);
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
                Server.SetVol(newvalue);
            }
        }

       
    }
}

  