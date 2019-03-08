using Balboa.Common;
using SiroccoControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class ControlPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SolidColorBrush WhiteBrush = new SolidColorBrush(Colors.White);
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Colors.Orange);
        private Server _server;
        private Status _status = new Status();
        private Song _song = new Song();
        private int _currentSongID = -1;

        private double _timeLeft;
        public double TimeLeft
        {
            get { return _timeLeft; }
            private set
            {
                if (_timeLeft != value)
                {
                    _timeLeft = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeLeft)));
                }
            }
        }

        private double _timeElapsed;
        public double TimeElapsed
        {
            get { return _timeElapsed; }
            private set
            {
                if (_timeElapsed != value)
                {
                    _timeElapsed = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeElapsed)));
                }
            }
        }

        private double _duration;
        public double Duration
        {
            get { return _duration; }
            private set
            {
                if (_duration != value)
                {
                    _duration = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
                }
            }
        }

        private char _PlayPauseButtonContent = (char)0xE103;
        public char PlayPauseButtonContent
        {
            get { return _PlayPauseButtonContent; }
            private set
            {
                if (_PlayPauseButtonContent != value)
                {
                    _PlayPauseButtonContent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayPauseButtonContent)));
                }
            }
        }

        private double _volume = 0;
        public double Volume
        {
            get { return _volume; }
            private set
            {
                if (_volume != value)
                {
                    _volume = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                }
            }
        }

        private SolidColorBrush _randomColor;
        public SolidColorBrush RandomColor
        {
            get { return _randomColor; }
            set
            {
                if (_randomColor != value)
                {
                    _randomColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RandomColor)));
                }
            }
        }
        private SolidColorBrush _consumeColor;
        public SolidColorBrush ConsumeColor
        {
            get { return _consumeColor; }
            set
            {
                if (_consumeColor != value)
                {
                    _consumeColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConsumeColor)));
                }
            }
        }
        private SolidColorBrush _repeatColor;
        public SolidColorBrush RepeatColor
        {
            get { return _repeatColor; }
            set
            {
                if (_repeatColor != value)
                {
                    _repeatColor = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RepeatColor)));
                }
            }
        }

        public string DataPanelInfo
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
        public string DataPanelElementsCount
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        Orientation IDataPanel.Orientation { get; set; }

      

        public ControlPanel()
        {
            this.InitializeComponent();
        }

        public ControlPanel(Server server):this()
        {
            Init(server);
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
            SizeChanged += (object sender, SizeChangedEventArgs e) => 
            {
                StopButton.Width = (e.NewSize.Width < 700) ? 0 : 100;
                PlayModeButtons.Width = (e.NewSize.Width < 600) ? 0 : 100;
                VolumeControl.Width = (e.NewSize.Width < 510) ? 0 : 160;
             };
        }

        public void Update()
        {
            _server.CurrentSong();
            _server.Status();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;

            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (mpdData.Command.Op == "status")
                    UpdateControlData(mpdData.Content);
                if (mpdData.Command.Op == "currentsong")
                    UpdateSongData(mpdData.Content);
            }
        }

        private void UpdateControlData(List<string> serverData)
        {
            _status.Update(serverData);

            TimeLeft    = _status.TimeLeft;
            PlayPauseButtonContent = (_status.State == "play") ? '\xE103' : '\xE102';
            Volume = _status.Volume;
            VolumeSlider.Value = _status.Volume;
            RandomColor = _status.Random ? OrangeBrush : WhiteBrush;
            ConsumeColor = _status.Consume ? OrangeBrush : WhiteBrush;
            RepeatColor = _status.Repeat ? OrangeBrush : WhiteBrush;

            if (_currentSongID != _status.SongId)
            {
                Duration = _status.Duration;
                TimeElapsed = 0;
                _currentSongID = _status.SongId;
                _server.CurrentSong();
            }
            if (Duration > 0)
            {
                if (!_seekBarIsBeingDragged)
                {
                    TimeLeft = _status.Duration - _status.TimeElapsed;
                    TimeElapsed = _status.TimeElapsed;
                }
            }
        }

        private void UpdateSongData(List<string> serverData)
        {
            _song.Update(serverData);
            Duration = _song.Duration;
            _server.UpdateAlbumArt(_song.File);
        }

        private void btn_PrevTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Previous();
        }

        private void btn_PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (_status.State)
            {
                case "stop": _server.Play();  break;
                case "pause":_server.Pause(); break;
                case "play": _server.Pause(); break;
            }
        }

        private void btn_NextTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Next();
        }

        private void btn_Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Stop();
            TimeElapsed = 0;
        }

        private void btn_Restart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Restart();
        }

        private void VolumeSlider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                int volume = Convert.ToInt32((sender as RoundSlider).Value);
                volume = volume < 0 ? 0 : volume;
                _server?.SetVolume(volume);
            }
        }


        #region Seek bar
        private bool _seekBarIsBeingDragged = false;
        private double _currentTrackPosition = 0;

        private void pb_Progress_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _seekBarIsBeingDragged = true;
            var sl = sender as Slider;
            _currentTrackPosition = sl.Value;
        }

        private void pb_Progress_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var sl = sender as Slider;
            double offset = sl.Value - _currentTrackPosition;

            string soffset = offset.ToString(CultureInfo.InvariantCulture);
            if (offset > 0)
                soffset = "+" + soffset;
            _seekBarIsBeingDragged = false;
            _server.SeekCurrent(soffset);
        }

        private void pb_Progress_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp = e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta / 24; // считаем что каждый клик смещает позицию в треке на 5 сек

            _currentTrackPosition = TimeElapsed;
            if (((sl.Value + mouseweeldelta) > 0) && ((sl.Value + mouseweeldelta) < sl.Maximum))
            {
                string soffset = mouseweeldelta.ToString(CultureInfo.InvariantCulture);

                if (mouseweeldelta > 0)
                    soffset = "+" + mouseweeldelta.ToString(CultureInfo.InvariantCulture);
                _server.SeekCurrent(soffset);
            }

        }
        #endregion


        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
        }

        private void Random_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Random(!_status.Random);
        }

        private void Repeat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Repeat(!_status.Repeat);
        }

        private void Consume_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Consume(!_status.Consume);
        }

 
    }  // class ControlPanel
}
