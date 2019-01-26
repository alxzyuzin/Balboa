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
    //public enum Panels
    //{ 
    //    CurrentTrackPanel,
    //    PlaylistPanel,
    //    TrackDirectoryPanel,
    //    SavedPlaylistsPanel,
    //    DatabaseExplorerPanel,
    //    SearchPanel,
    //    StatisticPanel,
    //    OutputsPanel,
    //    SettingsPanel
    //}

    public sealed partial class MainMenu : UserControl, IDataPanel, IRequestAction
    {
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
            throw new NotImplementedException();
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
            HighLiteSelectedItem(sender as Button);
        }
        private void SwitchToPlaylistPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new PlaylistPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToTrackDirectoryPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new TrackDirectoryPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToSavedPlaylistsPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SavedPlaylistsPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToSearchPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SearchPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToStatisticPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new StatisticPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToOutputsPanel(object sender, TappedRoutedEventArgs e)
        {
            var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new OutputsPanel(_server));
            RequestAction.Invoke(this, actionParams);
            HighLiteSelectedItem(sender as Button);
        }

        private void SwitchToSettingsPanel(object sender, TappedRoutedEventArgs e)
        {
           var actionParams = new ActionParams(ActionType.ActivateDataPanel).SetPanel(new SettingsPanel(_server));
           RequestAction.Invoke(this, actionParams);
           HighLiteSelectedItem(sender as Button);
        }

        public void HighLiteSelectedItem(Button pressedButton)
        {
            var buttons1 = (pressedButton.Parent as StackPanel).Children as UIElementCollection;
            var buttons2 = (pressedButton.Parent as StackPanel).Name == "stp_MenuButtons" ?
                                                stp_SmallMenuButtons.Children as UIElementCollection:
                                                stp_MenuButtons.Children as UIElementCollection;

            int i = (buttons1.IndexOf(pressedButton));
            for (int j=0; j < buttons1.Count; j++)
            {
                if (i == j)
                {
                    var b1 = (buttons1[j] as Button).Foreground = OrangeBrush; ;
                    var b2 = (buttons2[j] as Button).Foreground = OrangeBrush; ;
                }
                else
                {
                    var b1 = (buttons1[j] as Button).Foreground = WhiteBrush;
                    var b2 = (buttons2[j] as Button).Foreground = WhiteBrush;
                }
            }
         }

    } //  Class MainMenu
}   // Namespace Balboa
