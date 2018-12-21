/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс содержит данные текущего статуса сервера.
 *
  --------------------------------------------------------------------------*/

using System;
using System.Globalization;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using System.Text.RegularExpressions;

namespace Balboa.Common
{

    public sealed class Status: INotifyPropertyChanged
    {
        private const string _modName = "Status.cs";

        private Page   _mainPage;
        
        public enum ServerState
        {
            Play, Stop, Pause
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { PropertyChanged(this, new PropertyChangedEventArgs(propertyName) ); });
            }
        }


        #region Properties
        private int _volume;       
        public int Volume       // volume: 0-100 
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    NotifyPropertyChanged("Volume");
                }
            }
        }

        private bool _repeat;        
        public bool Repeat      // repeat: 0 or 1 
        {
            get { return _repeat; }
            set
            {
                if (_repeat!= value)
                {
                    _repeat = value;
                    NotifyPropertyChanged("Repeat");
                }
            }
        }

        private bool _random;       
        public bool Random          // random: 0 or 1 
        {
            get { return _random; }
            set
            {
                if (_random != value)
                {
                    _random = value;
                    NotifyPropertyChanged("Random");
                }
            }
        }


        private bool _single;   
        public bool Single          // single:  0 or 1
        {
            get { return _single; }
            set
            {
                if (_single != value)
                {
                    _single = value;
                    NotifyPropertyChanged("Single");
                }
            }
        }

        private bool _consume;    
        public bool Consume         // consume: 0 or 1
        {
            get { return _consume; }
            set
            {
                if (_consume != value)
                {
                    _consume = value;
                    NotifyPropertyChanged("Consume");
                }
            }
        }

        private int _playlistid;   
        public int PlaylistId     // playlist: 31-bit unsigned integer, the playlist version number
        {
            get { return _playlistid; }
            set
            {
                if (_playlistid != value)
                {
                    _playlistid = value;
                    NotifyPropertyChanged("PlaylistId");
                }
            }
        }

        private int _playlistLength;
        public int PlaylistLength   // playlistlength: integer, the length of the playlist
        {
            get { return _playlistLength; }
            set { _playlistLength = value; }
        }

        private string _state;    
        public string State         // state: play, stop, or pause
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    NotifyPropertyChanged("State");
                    ExtendedStatus = GetExtendedStatusValue(_state);
                }
            }
        }

        private int _song;          
        public int Song             // song: playlist song number of the current song stopped on or playing
        {
            get { return _song; }
            set
            {
                if (_song != value)
                {
                    _song = value;
                    NotifyPropertyChanged("Song");
                }
            }

        }

        private int _songid;        
        public int SongId           // songid: playlist songid of the current song stopped on or playing
        {
            get { return _songid; }
            set
            {
                if (_songid != value)
                {
                    _songid = value;
                    NotifyPropertyChanged("SongId");
                }
            }
        }

        //        private int     _nextSong;      // nextsong:   playlist song number of the next song to be played
        public int      NextSong { get; set; }       // nextsong:   playlist song number of the next song to be played

        //        private int     _nextSongID;    // nextsongid: playlist songid of the next song to be playedS
        public int      NextSongId { get; set; }     // nextsongid: playlist songid of the next song to be played
        //        private float   _time;          // time: total time elapsed(of current playing/paused song)
        public float    Time { get; set; }        // time: total time elapsed(of current playing/paused song)

        private float _duration;      // duration: Duration of the current song in seconds.
        public float    Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                { 
                    _duration = value;
                    NotifyPropertyChanged("Duration");
                }
            }
        }

        private float _timeLeft;
        public float    TimeLeft
        {
            get { return (_duration>0)?_duration - _timeElapsed:0; }
            set
            {
                if (_timeLeft!=value)
                {
                    _timeLeft = value;
                    NotifyPropertyChanged("TimeLeft");
                } 
            }
        }

        private float _timeElapsed;   // elapsed:  Total time elapsed within the current song, but with higher resolution.
        public float    TimeElapsed                // elapsed:  Total time elapsed within the current song, but with higher resolution.
        {
            get { return _timeElapsed; }
            set
            {
                if (_timeElapsed != value)
                {
                    _timeElapsed = value;
                    TimeLeft = (_duration > 0) ? _duration - _timeElapsed : 0;
                    NotifyPropertyChanged("TimeElapsed");
                }
            }
        }

        private string _audioParameters;
        public string   AudioParameters
        {
            get { return _audioParameters; }
            set
            {
               if(_audioParameters != value)
                { 
                    _audioParameters = value;
                    NotifyPropertyChanged("AudioParameters");
//                    ExtendedStatus = "Playing.   " + AudioParameters;
                }
            }

        }

        string _extendedStatus = string.Empty;
        public string   ExtendedStatus
        {
            get
            {
                return _extendedStatus;
            }
            set
            {
                if (_extendedStatus!= value)
                {
                    _extendedStatus = value;
                    NotifyPropertyChanged("ExtendedStatus");
                }
            }
        }

        //        private int     _bitrate;       // bitrate: instantaneous bitrate in kbps
        public int      Bitrate { get; set; }        // bitrate: instantaneous bitrate in kbps
        //        private int     _xFade;         // xfade: crossfade in seconds
        public int      XFade { get; set; }          // xfade: crossfade in seconds
        //        private float   _mixRampdB;     // mixrampdb: mixramp threshold in dB
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rampd")]
        public float    MixRampdB { get; set; }      // mixrampdb: mixramp threshold in dB
        //        private string  _mixRampDelay;  // mixrampdelay: mixrampdelay in seconds
        public string   MixRampDelay { get; set; }   // mixrampdelay: mixrampdelay in seconds

        //        private int     _sampleRate;    // audio: sampleRate:bits:channels
        public int      SampleRate { get; set; }     // audio: sampleRate:bits:channels

        //        private int     _bits;
        public int      Bits { get; set; }

        private int _channels;      // audio: sampleRate:bits:channels
        public int      Channels
        {
            get { return _channels; }
            set
            {
                if (_channels != value)
                {
                    _channels = value;
                    NotifyPropertyChanged("Channels");
                }
            }       // audio: sampleRate:bits:channels
        }

        private string _playButtonState;
        public string   PlayButtonState
        {
            get { return _playButtonState; }
            set
            { if (_playButtonState!=value)
                {
                    _playButtonState = value;
                    NotifyPropertyChanged("PlayButtonState");
                }
            }
        }

        //        private int     _updatingDbJobID;//updating_db: job id
        public int      UpdatingDbJobId { get; set; }
       
        //        private string  _error;
        public string   Error { get; set; }

        #endregion

        public Status(Page mainPage)
        {
            _mainPage = mainPage;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Update(MpdResponseCollection statusInfo)
        {
            if (statusInfo == null)
                throw new BalboaNullValueException(_modName, "Update", "323", "statusInfo");

            string notNumber = @"\D{1,}";

            foreach (string item in statusInfo)
            {
                string[] statusitem = item.Split(':');
                string statusvalue = statusitem[1].TrimStart(' ');
                switch (statusitem[0])
                {
                    case "volume": Volume = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;                // volume: 0-100 
                    case "repeat": Repeat = (statusvalue == "1") ? true : false; break;   // repeat: 0 or 1 
                    case "random": Random = (statusvalue == "1") ? true : false; break;   // random: 0 or 1 
                    case "single": Single = (statusvalue == "1") ? true : false; break;   // single:  0 or 1 
                    case "consume": Consume = (statusvalue == "1") ? true : false; break;  // consume: 0 or 1 
                    case "playlist": PlaylistId = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;          // playlist: 31-bit unsigned integer, the playlist version number
                    case "playlistlength": PlaylistLength = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;// playlistlength: integer, the length of the playlist
                    case "state": State = statusvalue; break;                             // state: play, stop, or pause
                    case "song": Song = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;                    // song: playlist song number of the current song stopped on or playing
                    case "songid": SongId = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;                // songid: playlist songid of the current song stopped on or playing
                    case "nextsong": NextSong = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;            // nextsong:   playlist song number of the next song to be played
                    case "nextsongid": NextSongId = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;        // nextsongid: playlist songid of the next song to be played
                    case "time": Time = float.Parse(statusvalue, CultureInfo.InvariantCulture); break;                  // time: total time elapsed(of current playing/paused song)
                    case "elapsed": TimeElapsed = float.Parse(statusvalue, CultureInfo.InvariantCulture); break;        // elapsed:  Total time elapsed within the current song, but with higher resolution.
                    case "bitrate": Bitrate = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;              // bitrate: instantaneous bitrate in kbps
                    case "xfade": XFade = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;                  // xfade: crossfade in seconds
                    case "mixrampdb": MixRampdB = float.Parse(statusvalue, CultureInfo.InvariantCulture); break;        // mixrampdb: mixramp threshold in dB
                    case "mixrampdelay": MixRampDelay = statusvalue; break;               // mixrampdelay: mixrampdelay in seconds
                    case "audio":
                        SampleRate = Regex.IsMatch(statusvalue, notNumber) ? 0 : int.Parse(statusvalue, CultureInfo.InvariantCulture);         // audio: sampleRate:bits:channels
                        Bits       = Regex.IsMatch(statusitem[2], notNumber) ? 0:int.Parse(statusitem[2], CultureInfo.InvariantCulture);
                        Channels   = Regex.IsMatch(statusitem[3], notNumber) ? 0:int.Parse(statusitem[3], CultureInfo.InvariantCulture);
                        
                        AudioParameters = "Sample rate: " + 
                            SampleRate.ToString(CultureInfo.InvariantCulture) + " kHz, " + 
                            Bits.ToString(CultureInfo.InvariantCulture) + " per sample, channels: " + 
                            Channels.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "updating_db": UpdatingDbJobId = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;  //updating_db: job id
                    case "error": Error = statusvalue; break;                             // error
                }
            }
            ExtendedStatus = GetExtendedStatusValue(_state);
        }

        private string GetExtendedStatusValue(string state)
        { 
            switch (state)
            {
                case "pause":   return "Paused.   " + AudioParameters; 
                case "play": return "Playing.   " + AudioParameters; 
                case "stop": return "Stopped."; 
                case "restart": return "Restarting ...";
                default: return string.Empty;
             }
        }
    }
}
