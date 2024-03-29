﻿using Balboa.Common;
using System;
using System.Collections;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{

    public sealed partial class SettingsPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDataPanelInfo,
                                                             IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private AppSettings    _appSettings = new AppSettings();
        private Server         _server;
        private ResourceLoader _resldr = new ResourceLoader();
        private string         _musicCollectionFolderToken;
        #region Properties
        private string         _serverName;
        public string ServerName
        {
            get { return _serverName; }
            set
            {
                if (_serverName != value)
                {
                    _serverName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ServerName)));
                }
            }
        }
        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                if (_port != value)
                {
                    _port = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Port)));
                }
            }
        }
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
                }
            }
        }
        private string _viewUpdateInterval;
        public string ViewUpdateInterval
        {
            get { return _viewUpdateInterval; }
            set
            {
                if (_viewUpdateInterval != value)
                {
                    _viewUpdateInterval = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ViewUpdateInterval)));
                }
            }
        }
        private string _musicCollectionFolder;
        public string MusicCollectionFolder
        {
            get { return _musicCollectionFolder; }
            set
            {
                if (_musicCollectionFolder != value)
                {
                    _musicCollectionFolder = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MusicCollectionFolder)));
                }
            }
        }
        private string _albumCoverFileNames;
        public string AlbumCoverFileNames
        {
            get { return _albumCoverFileNames; }
            set
            {
                if (_albumCoverFileNames != value)
                {
                    _albumCoverFileNames = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlbumCoverFileNames)));
                }
            }
        }
        private bool _displayFolderPictures;
        public bool DisplayFolderPictures
        {
            get { return _displayFolderPictures; }
            private set
            {
                if (_displayFolderPictures!=value)
                {
                    _displayFolderPictures = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayFolderPictures)));
                }
            }
        }

        
        public double TotalButtonWidth => AppBarButtons.ActualWidth;
        public string DataPanelInfo { get; set; }
        public string DataPanelElementsCount { get; set; }
        private Orientation _orientation;
        public Orientation Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    if (value == Orientation.Vertical)
                    {
                        ServerParams.Height = 700;
                        ServerParams.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                        
                    }
                    else
                    {
                        ServerParams.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                        ServerParams.Height = this.ActualHeight - AppBarButtons.Height;
                    }
                }
            }
        }
        #endregion Properties

        #region Constuctors
        public SettingsPanel()
        {
            this.InitializeComponent();
        }

        public SettingsPanel(Server server):this()
        {
            Init(server);

            ServerName =_appSettings.ServerName;
            Port = _appSettings.Port;
            Password = _appSettings.Password;
            ViewUpdateInterval =_appSettings.ViewUpdateInterval;
            MusicCollectionFolder = _appSettings.MusicCollectionFolder;
            AlbumCoverFileNames = _appSettings.AlbumCoverFileNames;
            DisplayFolderPictures  = _appSettings.DisplayFolderPictures;

            _musicCollectionFolderToken = _appSettings.MusicCollectionFolderToken;
        }

        #endregion

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            _server = server;
            _appSettings.Restore();
        }

        public void Update() { }

        private async void appbtn_SaveSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int ParseResult;

            if (!int.TryParse(Port, out ParseResult))
            {
                var message = new Message(MsgBoxType.Error, _resldr.GetString("PortValueMustBeNumber"));
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }
            else
            {
                if (ParseResult > 65535 || ParseResult == 0)
                {
                    var message = new Message(MsgBoxType.Error, _resldr.GetString("PortValueMustBeLess65536"));
                    RequestAction?.Invoke(this, new ActionParams(message));
                    return;
                }
            }

            if (!int.TryParse(ViewUpdateInterval, out ParseResult))
            {
                var message = new Message(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustNumber"));
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }
            else
            {
                int updateinterval = int.Parse(ViewUpdateInterval);
                if (ParseResult < 100)
                {
                    var message = new Message(MsgBoxType.Error, _resldr.GetString("ViewUpdateIntervalMustBe100"));
                    RequestAction?.Invoke(this, new ActionParams(message));
                    return;
                }
            }

            if ( (MusicCollectionFolder.Length==0) && (bool)(cbx_DisplayFolderPictures.IsChecked))
            {
                var message = new Message(MsgBoxType.Error, _resldr.GetString("MusicCollectionFolderNotDefined"));
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }
            

            // Проверим возможность соединения с новыми параметрами перед их сохранением
            Connection connection = new Connection();
            await connection.Open(ServerName, Port, Password);
            
            if (!connection.IsActive)
            {
                var message = new Message(MsgBoxType.Info, $"Connection to '{ServerName}' failed.\n {connection.Error}.",
                    MsgBoxButton.Close, 300);
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }
            if ((_appSettings.ServerName != ServerName) && (_appSettings.MusicCollectionFolderToken == _musicCollectionFolderToken))
            {
                var message = new Message(MsgBoxType.Warning, _resldr.GetString("ServerNameChanged"));
                MusicCollectionFolder = string.Empty;            
                StorageApplicationPermissions.FutureAccessList.Clear();
                _musicCollectionFolderToken = String.Empty;
            }
            connection.Close();


            _appSettings.ServerName = ServerName;
            _appSettings.Port = Port;
            _appSettings.Password = Password;
            _appSettings.ViewUpdateInterval = ViewUpdateInterval;
            _appSettings.MusicCollectionFolder = MusicCollectionFolder;
            _appSettings.AlbumCoverFileNames = AlbumCoverFileNames;
            _appSettings.DisplayFolderPictures = DisplayFolderPictures;
            _appSettings.DisplayFolderPictures = cbx_DisplayFolderPictures.IsChecked ?? false;
            _appSettings.MusicCollectionFolderToken = _musicCollectionFolderToken;

            _appSettings.Save();
            _server.Init(_appSettings);
            _server.Restart();
 
        }

        private async void appbtn_TestConnection_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string msg;
            
            Connection connection = new Connection();

            await connection.Open(ServerName, Port,Password);
            if (connection.IsActive)
                msg = $"Succesfully connected to {ServerName}. \n{connection.InitialResponse}";
            else
                msg = connection.Error;
            var message =  new Message(MsgBoxType.Info, msg);
            connection.Close();
            RequestAction?.Invoke(this, new ActionParams(message));

        }

        private async void btn_SelectMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                MusicCollectionFolder = folder.Path;
                if (_musicCollectionFolderToken != String.Empty)
                    StorageApplicationPermissions.FutureAccessList.Clear();
                _musicCollectionFolderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
            }
        }

        private void btn_ClearMusicCollectionPath_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
            _musicCollectionFolderToken = String.Empty;
            MusicCollectionFolder = String.Empty;
        }

        private void appbtn_StartSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
//            _server.Init(_appSettings);
            _server.Restart();
        }

        private void appbtn_StopSession_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Halt();
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
           // throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
