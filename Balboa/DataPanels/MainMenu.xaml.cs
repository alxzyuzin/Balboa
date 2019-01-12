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
    public sealed partial class MainMenu : UserControl, IDataPanel
    {
        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;

        private SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);

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
            var pressedButton = sender as Button;
            ActionRequested?.Invoke(this, new ActionParams(pressedButton.Name));
            
            // Изменим цвет текста во всех кнопках главного меню на белый
            foreach (Button button in stp_MenuButtons.Children)
                button.Foreground = WhiteBrush;

            pressedButton.Foreground = OrangeBrush;
        }
    } //  Class MainMenu
}   // Namespace Balboa
