using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class PlayMode : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private Status _status = new Status();

        private bool _random;
        public bool Random
        {
            get { return _random; }
            private set
            {
                if (_random != value)
                {
                    _random = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Random)));
                }
            }
        }

        private bool _repeat;
        public bool Repeat
        {
            get { return _repeat; }
            private set
            {
                if (_repeat != value)
                {
                    _repeat = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Repeat)));
                }
            }
        }

        private bool _consume;
        public bool Consume
        {
            get { return _consume; }
            private set
            {
                if (_consume != value)
                {
                    _consume = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Consume)));
                }
            }
        }


        public PlayMode()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server.CurrentSong();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "status")
                    UpdateControlData(mpdData.Content);
            }
        }

        private void UpdateControlData(List<string> serverData)
        {
            _status.Update(serverData);

            Random = _status.Random;
            Repeat = _status.Repeat;
            Consume = _status.Consume;
            
        }



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
    }
}
