using System;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class MainMenu : UserControl, IDataPanel
    {
        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;

        public event ActionRequestedEventHandler ActionRequested;

        public MainMenu()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Init(Server server)
        {
            _server = server;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }

        private void MenuButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var button = sender as Button;
            ActionRequested?.Invoke(this, new ActionParams((sender as Button).Name));
        }


        private void btn_Playlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.CurrentPlaylist);
        }

        /// <summary>
        /// Отображает содержимое закладки File system (Содержимое корневого каталога MPD )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_FileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.FileSystem);
        }

        private void btn_CurrentTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.CurrentTrack);
        }

        private void btn_Stats_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.Statistic);
            _server.Stats();
        }

        private void btn_Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.Search);

        }

        private void btn_SavedPlayLists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.Playlists);
            _server.ListPlaylists();
        }

        private void btn_Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Прочитаем список доступных выходов и добавим их в список параметров
            if (_server.IsConnected)
                _server.Outputs();
            SwitchDataPanelsTo(MainPage.DataPanelState.Settings);
        }

        private void btn_Genres_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.Genres);

            //            if (_server.Genres.Count == 0)
            //                _server.List("genre");

            //            _server.Artists.ClearAndNotify();
            //            _server.Albums.ClearAndNotify();
            //            _server.Tracks.ClearAndNotify();
        }

        private void btn_Artists_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SwitchDataPanelsTo(MainPage.DataPanelState.Artists);
            //            _server.Albums.ClearAndNotify();
            _server.Tracks.ClearAndNotify();
            _server.List("artist");
        }

        private void SwitchDataPanelsTo(MainPage.DataPanelState state)
        {

        }

        private void ResetMenuButtonsColor()
        {
            SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);
         // Изменим цвет текста во всех кнопках главного меню на белый
            foreach (UIElement uielement in stp_MenuButtons.Children)
            {
                var button = uielement as Button;
                if (button!=null)
                    button.Foreground = WhiteBrush;
            }
        }

 
    }
}
