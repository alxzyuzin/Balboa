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
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
[assembly: CLSCompliant(false)]

namespace Balboa
{
    // Change the current culture to the French culture,
    // and parsing the same string yields a different value.
    //  Thread.CurrentThread.CurrentCulture = new CultureInfo("Fr-fr", true);
    //  myDateTime = DateTime.Parse(dt);

    public enum ControlAction {NoAction, RestartServer, SwitchToTrackDirectory, SwitchToPlaylist }

    public partial class MainPage : Page
    {

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;

        public enum DataPanelState {CurrentTrack, CurrentPlaylist, FileSystem, Playlists, Statistic, Artists, Genres, Search, Settings} 

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
            this.SizeChanged += MainPage_SizeChanged;

            DataContext = this;
            _server = new Server(this);

            _server.Error += OnServerError;
            _server.CriticalError += OnServerCriticalError;

            //////
            p_MainMenu.Init(_server);
            p_MainMenu.ActionRequested += OnDataPanelActionRequested;

            //p_Settings.Init(_server);
            //p_TrackDirectory.Init(_server);
            //p_ControlPanel.Init(_server);
            //p_CurrentPlaylist.Init(_server);
            //p_PageHeader.Init(_server);
            //p_CurrentTrack.Init(_server);
            //p_PlayMode.Init(_server);
            //p_Stats.Init(_server);
            //p_Search.Init(_server);
            //p_GenresPanel.Init(_server);

            //////

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

        private void OnDataPanelActionRequested(Object sender, ActionParams actionParams)
        {
            if (actionParams.ActionType.HasFlag(ActionType.ActivateDataPanel))
            {
                ActivatePanel(actionParams.PanelClassName);
            }
            if (actionParams.ActionType.HasFlag(ActionType.DisplayMessage))
            {
                MsgBoxButton pressedButton = MsgBoxButton.Close;// = await DisplayMessage(actionParams.Message);
                ((IDataPanel)sender).HandleUserResponse(pressedButton);
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

  
        #region Обработчики событий объекта Server

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
        private void ActivatePanel(string panelClassName)
        {
            Type t = Type.GetType("Balboa."+panelClassName);
            UserControl panel = Activator.CreateInstance(t) as UserControl;
            if (DataPanel.Child != null)
            {
                ((IDataPanel)panel).ActionRequested -= OnDataPanelActionRequested;
            }
            DataPanel.Child = panel;
            ((IDataPanel)panel).Init(_server);
            ((IDataPanel)panel).Update();
            ((IDataPanel)panel).ActionRequested += OnDataPanelActionRequested;
        }



        private void SwitchDataPanelsTo(DataPanelState state)
        {
            // Выключим все информационные панели
            //p_PageHeader.Visibility = Visibility.Collapsed;
            //p_CurrentTrack.Visibility = Visibility.Collapsed;
            //p_CurrentPlaylist.Visibility = Visibility.Collapsed;
            //p_TrackDirectory.Visibility = Visibility.Collapsed;
            //p_Settings.Visibility = Visibility.Collapsed;
            //p_Stats.Visibility = Visibility.Collapsed;
            //p_Search.Visibility = Visibility.Collapsed;
            //p_GenresPanel.Visibility = Visibility.Collapsed;

            // Включим нужную панель
            // Изменим цвет текста в соответствующей кнопке на оранжевый
            // Установим режим в заголовке окна

            switch (state)
            {
                case DataPanelState.Artists:
//                    btn_Artists.Foreground = OrangeBrush;
//                    gr_Artists.Visibility = Visibility.Visible;
//                    textbox_CurrentMode.Text = _resldr.GetString("Artists");
//                    gr_ArtistsShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentPlaylist:
//                    btn_Playlist.Foreground = OrangeBrush;
//                    textbox_CurrentMode.Text = _resldr.GetString("PlaylistCurrent");
//                    p_CurrentPlaylist.Visibility = Visibility.Visible;
//                    p_CurrentPlaylist.Update();
                    //gr_CurrentPlaylistShowStoryboard.Begin();
                    break;
                case DataPanelState.CurrentTrack:
//                    btn_CurrentTrack.Foreground = OrangeBrush;
//                    textbox_CurrentMode.Text = _resldr.GetString("CurrentTrack");
//                    p_CurrentTrack.Visibility = Visibility.Visible;
//                    p_CurrentTrack.Update();
                    //gr_CurrentTrack.Visibility = Visibility.Visible;
                    //gr_CurrentTrackShowStoryboard.Begin();
                    break;
                case DataPanelState.FileSystem:
//                    btn_FileSystem.Foreground = OrangeBrush;
//                    textbox_CurrentMode.Text = _resldr.GetString("FilesAndFolders");
//                    p_TrackDirectory.Visibility = Visibility.Visible;
//                    p_TrackDirectory.Update();
                    break;
                case DataPanelState.Genres:
//                    btn_Genres.Foreground = OrangeBrush;
//                    p_GenresPanel.Visibility = Visibility.Visible;
//                    textbox_CurrentMode.Text = _resldr.GetString("Genres");
                    //gr_GenresShowStoryboard.Begin();
                    break;
                case DataPanelState.Playlists:
//                    btn_SavedPlayLists.Foreground = OrangeBrush;
//                    p_SavedPlaylistsPanel.Visibility = Visibility.Visible;
//                    textbox_CurrentMode.Text = _resldr.GetString("Playlist");
                    //gr_SavedPlayListsShowStoryboard.Begin();
                    break;
                case DataPanelState.Search:
//                    btn_Search.Foreground = OrangeBrush;
//                    p_Search.Visibility = Visibility.Visible;
//                    textbox_CurrentMode.Text = _resldr.GetString("Search");
                    //gr_SearchShowStoryboard.Begin();
                    break;
                case DataPanelState.Settings:
                    //              tbx_MusicCollectionPath.Text = _appSettings.MusicCollectionFolder;
                    //              checkbox_DisplayFolderPictures.IsChecked = _appSettings.DisplayFolderPictures;
                    //gr_Settings.Visibility = Visibility.Visible;
                    //gr_SettingsShowStoryboard.Begin();

//                    btn_Settings.Foreground = OrangeBrush;
//                    textbox_CurrentMode.Text = _resldr.GetString("Settings");
//                    p_Settings.Visibility = Visibility.Visible;
                    break;
                case DataPanelState.Statistic:
//                    btn_Stats.Foreground = OrangeBrush;
//                    p_Stats.Visibility = Visibility.Visible;
//                    textbox_CurrentMode.Text = _resldr.GetString("Statistic");
                    //gr_StatsShowStoryboard.Begin();
                    break;
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
        

        //private async Task<MsgBoxButton> DisplayMessage(string message, MsgBoxButton button, int height)
        //{
        //    MsgBox.SetButtons(button);
        //    MsgBox.Message = message;
        //    MsgBox.BoxHeight = height;
        //    return await MsgBox.Show();
        //}

        private async Task<MsgBoxButton> DisplayMessage(Message messageArgs)
        {
            MsgBox.SetButtons(messageArgs.Buttons);
            MsgBox.Message = messageArgs.Text;
            MsgBox.BoxHeight = messageArgs.BoxHeight;
            return await MsgBox.Show();
        }
    }
}

  