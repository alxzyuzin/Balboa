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
    public enum Panels
    { 
        CurrentTrackPanel,
        PlaylistPanel,
        TrackDirectoryPanel,
        SavedPlaylistsPanel,
        DatabaseExplorerPanel,
        SearchPanel,
        StatisticPanel,
        OutputsPanel,
        SettingsPanel
    }

    public sealed partial class MainMenu : UserControl, IDataPanel, IRequestAction
    {
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
            Grid.SetRow(this, row);
            Grid.SetColumn(this, column);
            Grid.SetRowSpan(this, 2);
            Margin =  new Windows.UI.Xaml.Thickness(0, 50, 0, 0);
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

        private void MenuButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            var pressedButton = sender as Button;
           
            RequestAction?.Invoke(this, 
                new ActionParams(ActionType.ActivateDataPanel, (Panels)Enum.Parse(typeof(Panels), pressedButton.Name)));

            // Изменим цвет текста во всех кнопках главного меню на белый
            foreach (Button button in stp_MenuButtons.Children)
                button.Foreground = WhiteBrush;

            pressedButton.Foreground = OrangeBrush;
        }

        public void SelectItem(Panels panel)
        {
            foreach (Button button in stp_MenuButtons.Children)
                button.Foreground = WhiteBrush;

            var b = stp_MenuButtons.Children.FirstOrDefault(item => (item as Button).Name == panel.ToString()) as Button;
            if (b!=null)
                b.Foreground = OrangeBrush;
            
        }

  
    } //  Class MainMenu
}   // Namespace Balboa
