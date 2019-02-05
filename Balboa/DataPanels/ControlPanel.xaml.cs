using Balboa.Common;
using SiroccoControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class ControlPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Server _server;
        private Status _status = new Status();
        private Song _song = new Song();
        private int _currentSongID = -1;

        private string _extendedStatus;
        public string ExtendedStatus
        {

            get { return _extendedStatus; }
            private set
            {
                if (_extendedStatus!=value)
                {
                    _extendedStatus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExtendedStatus)));
                }
            }
        }

        private string _connectionStatus = string.Empty;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            private set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionStatus)));
                }
            }
        }

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

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            private set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressValue)));
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





        public ControlPanel()
        {
            this.InitializeComponent();
           
        }

        public ControlPanel(Server server):this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
            _server.ConnectionStatusChanged += (Object obj, string status) => { ConnectionStatus = status; };
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
            _server.ConnectionStatusChanged += (Object obj, string status) => { ConnectionStatus = status; };
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

            ExtendedStatus = _status.ExtendedStatus;
            TimeLeft    = _status.TimeLeft;
            TimeElapsed = _status.TimeElapsed;
            PlayPauseButtonContent = (_status.State == "play") ? '\xE103' : '\xE102';
            Volume = _status.Volume;
            VolumeSlider.Value = _status.Volume;

            if (_currentSongID != _status.SongId)
            {
                Duration = _status.Duration;
                _currentSongID = _status.SongId;
                _server.CurrentSong();
            }
            if (Duration > 0)
            {
                TimeLeft = Duration - TimeElapsed;
                ProgressValue = _status.TimeElapsed / Duration;
                pb_Progress.Value = _status.TimeElapsed / Duration;
            }

        }

        private void UpdateSongData(List<string> serverData)
        {
            _song.Update(serverData);
            Duration = _song.Duration;
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

            //bool playedtrackselected = false; // Устанавливаем пизнак того что проигрываемый трек не подсвечен
            //// Ищем в Playlist трек с установленным признаком IsPlaying
            //// и если такой трек находится то прокручиваем Playlist так чтобы трек быд виден
            //foreach (Track item in _server.PlaylistData)
            //{
            //    if (item.IsPlaying)
            //    {
            //        playedtrackselected = true;
            //        //                    lv_PlayList.ScrollIntoView(item);
            //        return;
            //    }
            //}
            //// Если трек с признаком IsPlaying не найден и Playlist не пуст устанавливаем признак проигрываемого трека
            //// на первый элемент в Playlist
            //if (!playedtrackselected && _server.PlaylistData.Count > 0)
            //{
            //    _server.PlaylistData[0].IsPlaying = true;
            //}
        }

        private void btn_NextTrack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Next();
        }

        private void btn_Stop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Stop();
        }

        private void btn_Restart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Restart();
        }



        #region VOLUME CONTROL

        private void VolumeSlider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
                _server?.SetVolume(Convert.ToInt32((sender as RoundSlider).Value));
        }

        ////=======================================================================
        private void appbtn_Volume_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var appbarbutton = sender as AppBarButton;
            var ttv = appbarbutton.TransformToVisual(this);
            Point screenCoords = ttv.TransformPoint(new Point(15, -340));

            popup_VolumeControl.HorizontalOffset = screenCoords.X;
            popup_VolumeControl.VerticalOffset = screenCoords.Y;

            if (popup_VolumeControl.IsOpen)
                popup_VolumeControl.IsOpen = false;
            else
                popup_VolumeControl.IsOpen = true;
        }

        private void sl_Volume_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp = e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta / 12;

            int newvalue = (int)sl.Value + mouseweeldelta;
            if ((newvalue >= 0) && (newvalue <= 100) && Math.Abs(mouseweeldelta)>1)
            {
             //   _server.SetVolume(newvalue);
            }
        }


        private bool _volumeChangedByStatus = true;
        private void sl_Volume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var sl = sender as Slider;
            if (!_volumeChangedByStatus && _status.State == "play")
                _server.SetVolume((int)sl.Value);
            _volumeChangedByStatus = false;
        }

        #endregion

//        private void OnStatusDataPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            switch (e.PropertyName)
//            {
//                case "TimeElapsed":
//                    if (!_seekBarIsBeingDragged)
//                        pb_Progress.Value = _server.StatusData.TimeElapsed;
//                    break;
//                case "SongId": _server.CurrentSong(); break;
//                case "PlaylistId": _server.PlaylistInfo(); break;
//                case "State": // "&#xE102;" - Play, "&#xE103;" - Pause
//                    appbtn_PlayPause.Content = (_server.StatusData.State == "play") ? '\xE103' : '\xE102';

//                    if (_server.StatusData.State == "stop")
//                    {
////                        if (stackpanel_MainPanelHeader.Opacity != 0)
////                            stackpanel_MainPanelHeaderHideStoryboard.Begin();
//                    }
//                    else
//                    {
////                        if (stackpanel_MainPanelHeader.Opacity == 0)
////                            stackpanel_MainPanelHeaderShowStoryboard.Begin();
//                    }
//                    break;
//                case "Volume":
////                    _volumeChangedByStatus = true;
//                    sl_Volume.Value = _server.StatusData.Volume;
////                    sl_VerticalVolume.Value = _server.StatusData.Volume;
//                    break;
//            }
//        }



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
            _server.SeekCurrent(soffset);
            _seekBarIsBeingDragged = false;
        }

        private void pb_Progress_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var sl = sender as Slider;
            var cp = e.GetCurrentPoint((UIElement)sender) as PointerPoint;
            int mouseweeldelta = cp.Properties.MouseWheelDelta / 24; // считаем что каждый клик смещает позицию в треке на 5 сек

            _currentTrackPosition = pb_Progress.Value;
            if (((sl.Value + mouseweeldelta) > 0) && ((sl.Value + mouseweeldelta) < sl.Maximum))
            {
                string soffset = mouseweeldelta.ToString(CultureInfo.InvariantCulture);

                if (mouseweeldelta > 0)
                    soffset = "+" + mouseweeldelta.ToString(CultureInfo.InvariantCulture);
                _server.SeekCurrent(soffset);
            }

        }

        private void pb_Progress_Tapped(object sender, TappedRoutedEventArgs e)
        {

            var sl = sender as Slider;
            _currentTrackPosition = pb_Progress.Value;
            double offset = sl.Value - _currentTrackPosition;

            string soffset = offset.ToString(CultureInfo.InvariantCulture);
            if (offset > 0)
                soffset = "+" + soffset;
            _server.SeekCurrent(soffset);
            _seekBarIsBeingDragged = false;
        }

       
        
        #endregion


        public void Dispose()
        {
            _server.DataReady -= _server_DataReady;
            _server.ConnectionStatusChanged -= (Object obj, string status) => { ConnectionStatus = status; };
        }

 
    }
}
