/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс данные текущего статуса сервера.
 *
  --------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using System.Text.RegularExpressions;

namespace Balboa.Common
{

    public sealed class Status: INotifyPropertyChanged
    {

        private Page   _mainPage;
        
        public enum Serverstate
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

        #region fields
        private int     _volume;        // volume: 0-100 
        private bool    _repeat;        // repeat: 0 or 1 
        private bool    _random;        // random: 0 or 1 
        private bool    _single;        // single:  0 or 1 
        private bool    _consume;       // consume: 0 or 1 
        private Int32   _playlistid;      // playlist: 31-bit unsigned integer, the playlist version number
        private int     _playlistlength;// playlistlength: integer, the length of the playlist
        private string  _state;     // state: play, stop, or pause
        private int     _song;          // song: playlist song number of the current song stopped on or playing
        private int     _songid;        // songid: playlist songid of the current song stopped on or playing
//        private int     _nextSong;      // nextsong:   playlist song number of the next song to be played
//        private int     _nextSongID;    // nextsongid: playlist songid of the next song to be played
//        private float   _time;          // time: total time elapsed(of current playing/paused song)
        private float   _timeElapsed;   // elapsed:  Total time elapsed within the current song, but with higher resolution.
        private float   _timeLeft;
        private float   _duration;      // duration: Duration of the current song in seconds.
//        private int     _bitrate;       // bitrate: instantaneous bitrate in kbps
//        private int     _xFade;         // xfade: crossfade in seconds
//        private float   _mixRampdB;     // mixrampdb: mixramp threshold in dB
//        private string  _mixRampDelay;  // mixrampdelay: mixrampdelay in seconds
//        private int     _sampleRate;    // audio: sampleRate:bits:channels
//        private int     _bits;
        private int     _channels;      // audio: sampleRate:bits:channels
//        private int     _updatingDbJobID;//updating_db: job id
//        private string  _error;
        private string  _audioParams;
        private string _playButtonState;

        #endregion

        #region Properties
        public int Volume     // volume: 0-100 
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

        public bool Repeat // repeat: 0 or 1 
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

        public bool Random // random: 0 or 1 
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

        public bool Single // single:  0 or 1
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

        public bool Consume // consume: 0 or 1
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

        public Int32 PlaylistId     // playlist: 31-bit unsigned integer, the playlist version number
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
        public int Playlistlength   // playlistlength: integer, the length of the playlist
        {
            get { return _playlistlength; }
            set { _playlistlength = value; }
        }
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    NotifyPropertyChanged("State");
                    ExtendedStatus = _state;
                }
            }
        }  // state: play, stop, or pause
        public int Song                         // song: playlist song number of the current song stopped on or playing
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
        public int SongId                       // songid: playlist songid of the current song stopped on or playing
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

        public int      NextSong { get; set; }       // nextsong:   playlist song number of the next song to be played
        public int      NextSongID { get; set; }     // nextsongid: playlist songid of the next song to be played
        public float    Time { get; set; }        // time: total time elapsed(of current playing/paused song)
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
        public string   AudioParams
        {
            get { return _audioParams; }
            set
            {
                _audioParams = value;
                NotifyPropertyChanged("AudioParams");
                ExtendedStatus = "";

            }

        }

        public string   ExtendedStatus
        {
            get
            {
                string extendedstatus = string.Empty;
                switch (State)
                {
                    case "pause":extendedstatus = "Paused.   " + AudioParams; break;
                    case "play": extendedstatus = "Playing.   " + AudioParams; break;
                    case "stop": extendedstatus = "Stopped."; break;
                    case "restart": extendedstatus = "Restarting ..."; break;
                }
                return extendedstatus;
            }
            set
            {
                NotifyPropertyChanged("ExtendedStatus");
            }
        }
        public int      Bitrate { get; set; }        // bitrate: instantaneous bitrate in kbps
        public int      XFade { get; set; }          // xfade: crossfade in seconds
        public float    MixRampdB { get; set; }    // mixrampdb: mixramp threshold in dB
        public string   MixRampDelay { get; set; }// mixrampdelay: mixrampdelay in seconds
        public int      SampleRate { get; set; }     // audio: sampleRate:bits:channels
        public int      Bits { get; set; }
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

        public int      UpdatingDbJobID { get; set; }
        public string   Error { get; set; }

        #endregion

        public Status(Page mainpage)
        {
            _mainPage = mainpage;
        }

        public void Update(Common.MPDResponce statusinfo)
        {
            string notNumber = @"\D{1,}";

            foreach (string item in statusinfo)
            {
                string[] statusitem = item.Split(':');
                string statusvalue = statusitem[1].TrimStart(' ');
                switch (statusitem[0])
                {
                    case "volume": Volume = int.Parse(statusvalue); break;                // volume: 0-100 
                    case "repeat": Repeat = (statusvalue == "1") ? true : false; break;   // repeat: 0 or 1 
                    case "random": Random = (statusvalue == "1") ? true : false; break;   // random: 0 or 1 
                    case "single": Single = (statusvalue == "1") ? true : false; break;   // single:  0 or 1 
                    case "consume": Consume = (statusvalue == "1") ? true : false; break;  // consume: 0 or 1 
                    case "playlist": PlaylistId = int.Parse(statusvalue); break;          // playlist: 31-bit unsigned integer, the playlist version number
                    case "playlistlength": Playlistlength = int.Parse(statusvalue); break;// playlistlength: integer, the length of the playlist
                    case "state": State = statusvalue; break;                             // state: play, stop, or pause
                    case "song": Song = int.Parse(statusvalue); break;                    // song: playlist song number of the current song stopped on or playing
                    case "songid": SongId = int.Parse(statusvalue); break;                // songid: playlist songid of the current song stopped on or playing
                    case "nextsong": NextSong = int.Parse(statusvalue); break;            // nextsong:   playlist song number of the next song to be played
                    case "nextsongid": NextSongID = int.Parse(statusvalue); break;        // nextsongid: playlist songid of the next song to be played
                    case "time": Time = float.Parse(statusvalue); break;                  // time: total time elapsed(of current playing/paused song)
                    case "elapsed": TimeElapsed = float.Parse(statusvalue); break;        // elapsed:  Total time elapsed within the current song, but with higher resolution.
                    case "bitrate": Bitrate = int.Parse(statusvalue); break;              // bitrate: instantaneous bitrate in kbps
                    case "xfade": XFade = int.Parse(statusvalue); break;                  // xfade: crossfade in seconds
                    case "mixrampdb": MixRampdB = float.Parse(statusvalue); break;        // mixrampdb: mixramp threshold in dB
                    case "mixrampdelay": MixRampDelay = statusvalue; break;               // mixrampdelay: mixrampdelay in seconds
                    case "audio":
                        
                        //bool r= Regex.IsMatch(statusvalue, @"\A\d*");
                        SampleRate = Regex.IsMatch(statusvalue, notNumber) ? 0 : int.Parse(statusvalue);         // audio: sampleRate:bits:channels
                        Bits       = Regex.IsMatch(statusitem[2], notNumber) ? 0:int.Parse(statusitem[2]);
                        Channels   = Regex.IsMatch(statusitem[3], notNumber) ? 0:int.Parse(statusitem[3]);
                        
                        /*
                        SampleRate = int.Parse(statusvalue);         // audio: sampleRate:bits:channels
                        bool r = Regex.IsMatch(statusitem[2], @"\A\D*");
                        Bits =  int.Parse(statusitem[2]);
                        Channels = int.Parse(statusitem[3]);
                        */
                        AudioParams = "Sample rate: " + SampleRate.ToString() + " kHz, " + Bits.ToString() + " per sample, channels: " + Channels.ToString();
                        break;
                    case "updating_db": UpdatingDbJobID = int.Parse(statusvalue); break;  //updating_db: job id
                    case "error": Error = statusvalue; break;                             // error
                }
            }
        }
    }
}
