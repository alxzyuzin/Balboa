using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class PageHeader : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
        private Song _song = new Song();
        
        public ControlAction Action
        {
            get
            {
                return ControlAction.NoAction;
            }
        }

        private Message _message;
        public Message Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message!=value)
                {
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }

        private BitmapImage _albumArt;
        public BitmapImage AlbumArt
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

        private void  UpdateControlData(List<string> serverData)
        {
            _song.Update(serverData);
            Title = _song.Title;
            Artist = _song.Artist;
            Album = _song.Album;
            if (_song.File != null)
                GetAlbumArt();

        }

        private async void GetAlbumArt()
        {
            string s = Utilities.ExtractFilePath(_song.File);
            AlbumArt = await GetFolderImage(_server.MusicCollectionFolder, s, _server.AlbumCoverFileNames);
          
        }

        private async Task<BitmapImage> GetFolderImage(string musicCollectionFolder, string folderName, string albumCoverFileNames)
        {
            StorageFile file = null;
            while (musicCollectionFolder.EndsWith("\\", StringComparison.Ordinal))
                musicCollectionFolder = musicCollectionFolder.Substring(0, musicCollectionFolder.Length - 1);

            StringBuilder sb = new StringBuilder(musicCollectionFolder);

            sb.Append('\\');
            sb.Append(folderName);
            sb.Replace('/', '\\');
            sb.Append('\\');

            int pathlength = sb.Length;

            string[] CoverFileNames = albumCoverFileNames.Split(';');

            foreach (string albumCoverFileName in CoverFileNames)
            {
                try
                {
                    sb.Append(albumCoverFileName);
                    file = await StorageFile.GetFileFromPathAsync(sb.ToString());
                    break;
                }
                catch (FileNotFoundException)
                {
                    sb.Remove(pathlength, albumCoverFileName.Length);
                }
                catch (UnauthorizedAccessException)
                {
                    string message = string.Format(_resldr.GetString("CheckDirectoryAvailability"), _server.Settings.MusicCollectionFolder);
                    Message = new Message(MsgBoxType.Error, message, MsgBoxButton.Close, 200);


                }
                catch (Exception ee)
                {
                    Message = new Message(MsgBoxType.Error,
                                            string.Format(_resldr.GetString("Exception"), ee.GetType().ToString(), ee.Message),
                                            MsgBoxButton.Close, 200);
                }
            }
            if (file == null)
                return null;
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
            }
        }
    }
    
}
