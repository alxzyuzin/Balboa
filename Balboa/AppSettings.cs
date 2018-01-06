using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace Balboa
{
    public class AppSettings
    {
        private bool   _initialSetupDone = false;
        private string _server = "";
        private string _port = "";
        private int    _viewUpdateInterval = 500;
        private string _password = "";
      
        private string _musicCollectionFolder = "";
        private string _musicCollectionFolderToken = "";
        private string _albumCoverFileName = "";
        private bool   _displayFolderPictures = false;

        private bool _settingsChanged = false;
        private bool _serverNameChanged = false;
        private bool _musicCollectionFolderTokenChanged = false;

        public bool   InitialSetupDone
        {
            get
            {
                return _initialSetupDone;
            }
            set
            {
                if(_initialSetupDone!=value)
                {
                    _initialSetupDone = value;
                }
            }
        }

        public string Server
        {
            get
            {
                return _server;
            }
            set
            {
                if (_server != value)
                {
                    _server = value;
                    _settingsChanged = true;
                    _serverNameChanged = true;
                }
             }
        }

        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                if (_port != value)
                {
                    _port = value;
                    _settingsChanged = true;
                }
            }
        }
        
        public string MusicCollectionFolder
        {
            get
            {
                return _musicCollectionFolder;
            }
            set
            {
                if (_musicCollectionFolder != value)
                {
                    _musicCollectionFolder = value;
                    _settingsChanged = true;
                }
            }
        }

        public string MusicCollectionFolderToken
        {
            get
            {
                return _musicCollectionFolderToken;
            }
            set
            {
                if (_musicCollectionFolderToken != value)
                {
                    _musicCollectionFolderToken = value;
                    _settingsChanged = true;
                    _musicCollectionFolderTokenChanged=true;
                }
            }
        }

        public int      ViewUpdateInterval
        {
            get
            {
                return _viewUpdateInterval;
            }
            set
            {
                if (_viewUpdateInterval != value)
                {
                    _viewUpdateInterval = value;
                    _settingsChanged = true;
                }
            }
        }
        public string   Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    _settingsChanged = true;
                }
            }
        }

        public bool DisplayFolderPictures
        {
            get
            {
                return _displayFolderPictures;
            }
            set
            {
                if (_displayFolderPictures != value)
                {
                    _displayFolderPictures = value;
                    _settingsChanged = true;
                }
            }
        }

        public string AlbumCoverFileName
        {
            get
            {
                return _albumCoverFileName;
            }
            set
            {
                if (_albumCoverFileName != value)
                {
                    _albumCoverFileName = value;
                    _settingsChanged = true;
                }
            }
        }
       
        public bool SettingsChanged { get { return _settingsChanged; } }
        public bool ServerNameChanged { get { return _serverNameChanged; } }
        public bool MusicCollectionFolderTokenChanged { get { return _musicCollectionFolderTokenChanged; } }
       
        public void Restore()
        {
            Object value;

            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

            value = LocalSettings.Values["InitialSetupDone"];
            if ( value != null ) InitialSetupDone = (bool)value;

                value = LocalSettings.Values["Server"];
            if (value != null) Server = (string)value;

            value = LocalSettings.Values["Port"];
            if (value != null) Port = (string)value;

            value = LocalSettings.Values["ViewUpdateInterval"];
            if (value != null) ViewUpdateInterval = (int)value;

            value = LocalSettings.Values["Password"];
            if (value != null) Password = (string)value;

            value = LocalSettings.Values["MusicCollectionFolder"];
            if (value != null)     MusicCollectionFolder = (string)value;
            value = LocalSettings.Values["MusicCollectionFolderToken"];
            if (value != null)     MusicCollectionFolderToken = (string)value;
            value = LocalSettings.Values["AlbumCoverFileName"];
            if (value != null)     AlbumCoverFileName = (string)value;

            value = LocalSettings.Values["DisplayFolderPictures"];
            if (value != null) DisplayFolderPictures = (bool)value;
            
        }

        public void Save()
        {
            InitialSetupDone = false;
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
                
            LocalSettings.Values["Server"] = Server;
            LocalSettings.Values["Port"] = Port;
            LocalSettings.Values["ViewUpdateInterval"] = ViewUpdateInterval;
            LocalSettings.Values["Password"] = Password;

            LocalSettings.Values["MusicCollectionFolder"]= MusicCollectionFolder;
            LocalSettings.Values["MusicCollectionFolderToken"] = MusicCollectionFolderToken;
            LocalSettings.Values["AlbumCoverFileName"] = AlbumCoverFileName;

            LocalSettings.Values["DisplayFolderPictures"] = DisplayFolderPictures;

            if (Server != null && Port != null)
                    InitialSetupDone = true;
            LocalSettings.Values["InitialSetupDone"] = InitialSetupDone;

            _settingsChanged = false;
            _serverNameChanged = false;
            _musicCollectionFolderTokenChanged = false;
        }

        public void Prepare()
        {
           _settingsChanged = false;
           _serverNameChanged = false;
           _musicCollectionFolderTokenChanged = false;
        }

    public void SetDefault()
        {
            InitialSetupDone = false;
            Port = "6600";
            ViewUpdateInterval = 500;
            Password = "";
            DisplayFolderPictures = false;
     }

        private enum MessageBoxButtons { OK, Cancel, Continue, Retry, OK_Cancel }

        private async Task<MessageBoxButtons> MessageBox(string title, string message, MessageBoxButtons buttons)
        {

            object cmdid = new object();
            MessageDialog md = new MessageDialog(message, "Connection test");
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    md.Commands.Add(new UICommand("OK", null, MessageBoxButtons.OK));
                    break;
                case MessageBoxButtons.Cancel:
                    md.Commands.Add(new UICommand("Cancel", null, MessageBoxButtons.Cancel));
                    break;
                case MessageBoxButtons.Continue:
                    md.Commands.Add(new UICommand("Continue", null, MessageBoxButtons.Continue));
                    break;
                case MessageBoxButtons.Retry:
                    md.Commands.Add(new UICommand("Retry", null, MessageBoxButtons.Retry));
                    break;
                case MessageBoxButtons.OK_Cancel:
                    md.Commands.Add(new UICommand("OK", null, MessageBoxButtons.OK));
                    md.Commands.Add(new UICommand("Cancel", null, MessageBoxButtons.Cancel));
                    break;
            }
            UICommand selected = (UICommand)await md.ShowAsync();
            return (MessageBoxButtons)selected.Id;
        }
     }
}
