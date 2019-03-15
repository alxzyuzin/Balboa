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
using System.Collections.Generic;

namespace Balboa.Common
{

    public sealed class Status
    {
        public enum ServerState
        {
            Play, Stop, Pause
        }

        #region Properties
        public int    Volume { get; private set; }      // volume: 0-100 
        public bool   Repeat { get; private set; }      // repeat: 0 or 1 
        public bool   Random { get; private set; }          // random: 0 or 1 
        public bool   Single { get; private set; }           // single:  0 or 1
        public bool   Consume { get; private set; }          // consume: 0 or 1
        public int    PlaylistId { get; private set; }      // playlist: 31-bit unsigned integer, the playlist version number
        public int    PlaylistLength { get; private set; }    // playlistlength: integer, the length of the playlist
        public string State { get; private set; }        // state: play, stop, or pause


        public int    Song { get; private set; }              // song: playlist song number of the current song stopped on or playing
        public int    SongId { get; private set; }            // songid: playlist songid of the current song stopped on or playing
        public int    NextSong { get; private set; }       // nextsong:   playlist song number of the next song to be played
        public int    NextSongId { get; private set; }     // nextsongid: playlist songid of the next song to be played
        public float  Time { get; private set; }        // time: total time elapsed(of current playing/paused song)

        public double Duration { get; private set; }  // duration: Duration of the current song in seconds.
        public double TimeLeft { get; private set; }        
        public double TimeElapsed { get; private set; }

        public string AudioParameters { get; private set; }  
        public string ExtendedStatus { get; private set; } 
        public int    Bitrate { get; private set; }        // bitrate: instantaneous bitrate in kbps
        public int    XFade { get; private set; }          // xfade: crossfade in seconds
        public float  MixRampDB { get; private set; }      // mixrampdb: mixramp threshold in dB
        public string MixRampDelay { get; private set; }   // mixrampdelay: mixrampdelay in seconds
        public int    SampleRate { get; private set; }     // audio: sampleRate:bits:channels
        public int    Bits { get; private set; }
        public int    Channels { get; private set; }// audio: sampleRate:bits:channels
        public int    UpdatingDbJobId { get; set; }
        public string Error { get; set; }

        #endregion

        public Status()
        {
        }


        public void Update(List<string> statusInfo)
        {
            if (statusInfo == null) throw new ArgumentNullException(nameof(statusInfo));

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
                    case "duration": Duration = float.Parse(statusvalue, CultureInfo.InvariantCulture); break;          // duration: 
                    case "xfade": XFade = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;                  // xfade: crossfade in seconds
                    case "mixrampdb": MixRampDB = float.Parse(statusvalue, CultureInfo.InvariantCulture); break;        // mixrampdb: mixramp threshold in dB
                    case "mixrampdelay": MixRampDelay = statusvalue; break;               // mixrampdelay: mixrampdelay in seconds
                    case "audio":
                        //int.TryParse()
                        SampleRate = Regex.IsMatch(statusvalue, notNumber) ? 0 : int.Parse(statusvalue, CultureInfo.InvariantCulture);         // audio: sampleRate:bits:channels
                        Bits = Regex.IsMatch(statusitem[2], notNumber) ? 0 : int.Parse(statusitem[2], CultureInfo.InvariantCulture);
                        Channels = Regex.IsMatch(statusitem[3], notNumber) ? 0 : int.Parse(statusitem[3], CultureInfo.InvariantCulture);
                        AudioParameters = $"Sample rate: { SampleRate.ToString(CultureInfo.InvariantCulture)} kHz, { Bits.ToString(CultureInfo.InvariantCulture)} bits per sample, channels: { Channels.ToString(CultureInfo.InvariantCulture)}";
                        break;
                    case "updating_db": UpdatingDbJobId = int.Parse(statusvalue, CultureInfo.InvariantCulture); break;  //updating_db: job id
                    case "error": Error = statusvalue; break;                             // error
                }
            }
            ExtendedStatus = GetExtendedStatusValue(State);
            TimeLeft = Duration - TimeElapsed;
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
