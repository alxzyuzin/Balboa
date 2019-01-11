﻿using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class PageHeader : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler ActionRequested;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private Song _song = new Song();
        
        //public ControlAction Action
        //{
        //    get
        //    {
        //        return ControlAction.NoAction;
        //    }
        //}

        //private Message _message;
        //public Message Message
        //{
        //    get
        //    {
        //        return _message;
        //    }
        //    set
        //    {
        //        if (_message!=value)
        //        {
        //            _message = value;
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
        //        }
        //    }
        //}

        private AlbumArt _albumArt = new AlbumArt();
        public AlbumArt AlbumArt
        {
            get { return _albumArt; }
            private set
            {
                if (_albumArt != value)
                {
                    _albumArt = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumArt)));
                }
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            private set
            {
                if (_title != value)
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }

            }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            private set
            {
                if (_artist != value)
                {
                    _artist = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Artist)));
                }

            }
        }

        private string _album;
        public string Album
        {
            get { return _album; }
            private set
            {
                if (_album != value)
                {
                    _album = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Album)));
                }

            }
        }
        
        public PageHeader()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        private void _server_DataReady(object sender, EventArgs e)
        {
            var mpdData = e as MpdResponse;
            if (mpdData.Command.Op != "currentsong")
                return;

            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                UpdateControlData(mpdData.Content);
            }
        }

        private async void  UpdateControlData(List<string> serverData)
        {
            _song.Update(serverData);
            Title = _song.Title;
            Artist = _song.Artist;
            Album = _song.Album;
            if (_song.File != null)
            {
                await AlbumArt.LoadImageData(_server.MusicCollectionFolder, _song.File, _server.AlbumCoverFileNames);
                await AlbumArt.UpdateImage();
            }
        }


        // TO DO Гасить заголовок если проигрывание трека остановлено
        //if (_server.StatusData.State != "stop")
        //{
        //    //if (state != DataPanelState.CurrentTrack)
        //    //{
        //    //    if (p_PageHeader.Opacity == 0)
        //    //        stackpanel_MainPanelHeaderShowStoryboard.Begin();
        //    //}
        //    //else
        //    //    stackpanel_MainPanelHeaderHideStoryboard.Begin();
        //}

        private void textblock_MainMenu_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // if (popup_MainMenu.IsOpen)
            //    popup_MainMenu.IsOpen = false;
            //else
            //    popup_MainMenu.IsOpen = true;
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            throw new NotImplementedException();
        }
    } // Class PageHeader

}
