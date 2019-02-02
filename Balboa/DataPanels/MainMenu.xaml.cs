using System;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Linq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    

    public sealed partial class MainMenu : UserControl, IDataPanel, IRequestAction
    {

        private enum Panel
        {
            CurrentTrackPanel = 1,
            PlaylistPanel = 2,
            TrackDirectoryPanel = 3,
            SavedPlaylistsPanel = 4,
            SearchPanel = 5,
            StatisticPanel = 6,
            OutputsPanel = 7, 
            SettingsPanel = 8
        }


        private Server _server;
        private ResourceLoader _resldr = new ResourceLoader();
        private SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);

        public event ActionRequestedEventHandler RequestAction;

        public MainMenu()
        {
            InitializeComponent();
        }

        public MainMenu(Server server, int row = 0, int column = 0):this()
        {
            _server = server;
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
            Grid.SetRowSpan(this, 2);
            Margin =  new Windows.UI.Xaml.Thickness(0, 10, 0, 0);
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

        private void SwitchMenuState(object sender, TappedRoutedEventArgs e)
        {
            //var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SettingsPanel(_server));
            //RequestAction.Invoke(this, actionParams);
            //HighLiteSelectedItem(sender as Button);
        }
        private void SwitchToCurrentTrackPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new CurrentTrackPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }
        private void SwitchToPlaylistPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new PlaylistPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToTrackDirectoryPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new TrackDirectoryPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToSavedPlaylistsPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SavedPlaylistsPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToSearchPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SearchPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToStatisticPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new StatisticPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToOutputsPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new OutputsPanel(_server));
            RequestAction.Invoke(this, actionParams);
        }

        private void SwitchToSettingsPanel(object sender, TappedRoutedEventArgs e)
        {
           var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SettingsPanel(_server));
           RequestAction.Invoke(this, actionParams);
        }

        public void HighLiteSelectedItem(string className)
        {
            var buttons = stp_MenuButtons.Children as UIElementCollection;
            for (int i = 0; i < buttons.Count; i++)
                SetMenuItemcolor(i, WhiteBrush);

            var p = Enum.Parse(typeof(Panel), className) ?? 0;
            SetMenuItemcolor((int)p, OrangeBrush);
        }

        private void SetMenuItemcolor(int itemNumber, SolidColorBrush color)
        {
            ((Button)(stp_SmallMenuButtons.Children as UIElementCollection)[itemNumber]).Foreground = color;
            ((Button)(stp_MenuButtons.Children as UIElementCollection)[itemNumber]).Foreground = color;
        }
           
        
    } //  Class MainMenu
}   // Namespace Balboa
