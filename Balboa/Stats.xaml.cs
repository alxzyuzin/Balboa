using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Balboa.Common;
using Windows.ApplicationModel.Resources;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class Stats : UserControl, INotifyPropertyChanged, IDataPanel
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler ActionRequested;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;

        private Statistic _statistic = new Statistic();

        public int Artists => _statistic.Artists;
        public int Albums => _statistic.Albums;
        public int Songs => _statistic.Songs;
        public int Uptime => _statistic.Uptime;
        public int DbPlaytime => _statistic.DbPlaytime;
        public DateTime DbUpdateDT => _statistic.DbUpdateDT;
        public int Playtime => _statistic.Playtime;

        //public ControlAction Action
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public Message Message
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public Stats()
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
            _server.Stats();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {

            var mpdData = e as MpdResponse;
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "stats")
                {
                    _statistic.Update(mpdData.Content);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                }

            }
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }
    }
}
