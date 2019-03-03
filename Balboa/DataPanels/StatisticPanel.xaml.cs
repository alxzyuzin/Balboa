using Balboa.Common;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class StatisticPanel : UserControl, INotifyPropertyChanged, IDataPanel,
                                                 IRequestAction, IDisposable
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;

        private Statistic _statistic = new Statistic();

        private int _artists;
        public int Artists
        {
            get { return _artists; }
            set
            {
                if (_artists!=value)
                {
                    _artists = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Artists)));
                }
            }
        }

        private int _albums;
        public int Albums
        {
            get { return _albums; }
            set
            {
                if (_albums != value)
                {
                    _albums = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Albums)));
                }
            }
        }
        
        private int _songs;
        public int Songs
        {
            get { return _songs; }
            set
            {
                if (_songs != value)
                {
                    _songs = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Songs)));
                }
            }
        }

        private int _uptime;
        public int Uptime
        {
            get { return _uptime; }
            set
            {
                if (_uptime != value)
                {
                    _uptime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Uptime)));
                }
            }
        }

        public int _dbPlaytime;
        public int DbPlaytime
        {
            get { return _dbPlaytime; }
            set
            {
                if (_dbPlaytime != value)
                {
                    _dbPlaytime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DbPlaytime)));
                }
            }
        }

        private DateTime _dbUpdateDT;
        public DateTime DbUpdateDT
        {
            get { return _dbUpdateDT; }
            set
            {
                if (_dbUpdateDT != value)
                {
                    _dbUpdateDT = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DbUpdateDT)));
                }
            }
        }

        private int _playtime;
        public int Playtime
        {
            get { return _playtime; }
            set
            {
                if (_playtime != value)
                {
                    _playtime = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playtime)));
                }
            }
        }

        public Orientation Orientation { get; set; }

        public StatisticPanel()
        {
            this.InitializeComponent();
        
        }

        public StatisticPanel(Server server):this()
        {
            _server = server;
            _server.DataReady += _server_DataReady;
            _server.Stats();
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

                    Artists = _statistic.Artists;
                    Albums = _statistic.Albums;
                    Songs = _statistic.Songs;
                    Uptime = _statistic.Uptime;
                    DbPlaytime = _statistic.DbPlaytime;
                    DbUpdateDT = _statistic.DbUpdateDT;
                    Playtime = _statistic.Playtime;
                }
            }
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }
    }
}
