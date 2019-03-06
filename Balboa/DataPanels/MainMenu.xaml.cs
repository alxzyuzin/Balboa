using System;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    

    public sealed partial class MainMenu : UserControl,  IRequestAction
    {
        private enum Panel
        {
            CurrentTrackPanel = 0,
            PlaylistPanel = 1,
            TrackDirectoryPanel = 2,
            SavedPlaylistsPanel = 3,
            SearchPanel = 4,
            StatisticPanel = 5,
            OutputsPanel = 6, 
            SettingsPanel = 7
        }

        public bool _displayButtonsOnly;
        public bool DisplayButtonsOnly
        {
            get { return _displayButtonsOnly; }
            set
            {
                if (_displayButtonsOnly != value)
                {
                    _displayButtonsOnly = value;
                    if (_displayButtonsOnly)
                        VisualStateManager.GoToState(this, "Narrow", true);
                    else
                        VisualStateManager.GoToState(this, "Wide", true);
                }
            }
        }

        private Server _server;
        private ResourceLoader _resldr = new ResourceLoader();
        private SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);

        public event ActionRequestedEventHandler RequestAction;

        public MainMenu()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, "Wide", true);
        }

        public MainMenu(Server server, int row = 0, int column = 0):this()
        {
            _server = server;
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
            Grid.SetRowSpan(this, 2);
         }

        public void Init(Server server)
        {
            _server = server;
        }


        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
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
