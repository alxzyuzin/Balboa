using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Balboa
{
    public sealed partial class TrackDirectory : UserControl, INotifyPropertyChanged, IDataPanel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //public event EventHandler<DisplayMessageEventArgs> MessageReady;
        //public event EventHandler<CommandButtonPressedEventArgs> CommandButtonPressed;
        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;


      
        private string _currentPath = string.Empty;
        private string _parentFolderName = string.Empty;
        private string _newPathChunck = string.Empty;

        private GridViewItem _gridViewItemGotFocus;
        public ObservableCollection<File> Files => _files;
        private ObservableCollection<File> _files = new ObservableCollection<File>();
        private bool _fileIconsUpdating = false;

        private Progress<File> _progress;
        private ManualResetEvent _ThreadEvent = new ManualResetEvent(false);
        private CancellationTokenSource _tokenSource; 

        private string _emptyDirectoryMessage = string.Empty;
        public string EmptyDirectoryMessage
        {
            get
            {
                return _emptyDirectoryMessage;
            }
            set
            {
                if (_emptyDirectoryMessage != value)
                {
                    _emptyDirectoryMessage = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmptyDirectoryMessage)));
                }

            }
        }

        private ControlAction _action = ControlAction.NoAction;
        public ControlAction Action
        {
            get { return _action; }
            private set
            {
//                if (_action!= value)
//                {
                    _action = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Action)));
//                }
            }
        }

        private Message _message;
        public Message Message
        {
            get { return _message; }
            private set
            {
                if (_message != value)
                {
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }


        public TrackDirectory()
        {
            this.InitializeComponent();
            DataContext = this;
            _progress = new Progress<File>(SetFileIcon);
        }

        public void Init(Server server)
        {
            _server = server;
            _server.DataReady += _server_DataReady;
        }

        public void Update()
        {
            _server.LsInfo( _currentPath);
        }

        private async void _server_DataReady(object sender, EventArgs e)
        {
            
            var mpdData = e as MpdResponse;
            if (mpdData.Command.Op != "lsinfo")
                return;
                
            if (mpdData.Keyword == ResponseKeyword.OK)
            {
                if (_fileIconsUpdating)
                {
                    _tokenSource.Cancel();
                    _ThreadEvent.WaitOne();
                    while (_fileIconsUpdating){ await Task.Delay(100); }
                }
                
                    UpdateControlData(mpdData.Content);
                    HighLiteLastOpenedFolder();
                    if (_currentPath.Length>0 && _newPathChunck.Length>0)
                        _currentPath += "/";
                    _currentPath += _newPathChunck;
                    _newPathChunck = string.Empty;

                    appbtn_Up.IsEnabled = _currentPath.Length>0 ? true : false;
                    EmptyDirectoryMessage = _files.Count == 0 ?
                         string.Format(_resldr.GetString("NoAudioFilesInFolder"), "\""+_currentPath+ "\"") : string.Empty;

                    _tokenSource = new CancellationTokenSource();
                    CancellationToken token = _tokenSource.Token;
                    WorkItemHandler workhandler = delegate { UpdateFilesIcons(token); };
                    
                    await ThreadPool.RunAsync(workhandler, WorkItemPriority.High, WorkItemOptions.TimeSliced);
             }
            else
            {
                _newPathChunck = string.Empty;
            }
        }

        public void UpdateControlData(List<string> serverData)
        {
            _files.Clear();
            while (serverData.Count > 0)
            {
                var file = new File();
                file.Update(serverData);
                if(file.Nature == FileNature.File || file.Nature == FileNature.Directory)
                    _files.Add(file);
            }
        }

        private void gr_FileSystemContent_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e != null)
                _gridViewItemGotFocus = e.OriginalSource as GridViewItem;
        }

        private void gr_FileSystemContent_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_gridViewItemGotFocus == null)
                return;

            File currentfileitem = _gridViewItemGotFocus.Content as File;

            if (currentfileitem == null)
                return;

            if (currentfileitem.Nature == FileNature.Directory)
            {
                _newPathChunck = currentfileitem.Name.Trim();
                 string path = _currentPath;
                if (path.Length > 0)
                    path += "/";
                path += _newPathChunck;
                _server.LsInfo(path);
             }
        }

        private async void appbtn_Up_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_fileIconsUpdating)
            {
                _tokenSource.Cancel();
                while (_fileIconsUpdating)
                {
                    await Task.Delay(50);
                }
                bool b = _ThreadEvent.WaitOne();
            }

            int i = _currentPath.LastIndexOf("/");
            if (i >= 0)
                _currentPath = _currentPath.Remove(i);
             else
             _currentPath = string.Empty;
             _server.LsInfo(_currentPath);
                // Eсли мы поднялись на самый верх по дереву каталогов отключим кнопку Up
             appbtn_Up.IsEnabled = _currentPath.Length > 0 ? true : false;
            
        }

        private void appbtn_RescanDatabase_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _server.Update();
        }

        private void appbtn_AddFromFileSystem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string path = string.Empty;
            if (_currentPath.Length>0 && !_currentPath.EndsWith("/"))
                path = _currentPath + "/";  
            //else
            //    path = _currentPath;
            if (gr_FileSystemContent.SelectedItems.Count == 0)
            {
                Message = new Message(MsgBoxType.Info, _resldr.GetString("NoSelectedItemsToAdd"), MsgBoxButton.Close, 200);
                return;
            }
            foreach (File item in gr_FileSystemContent.SelectedItems)
            {
                string lp = path + item.Name;
                _server.Add(path + item.Name);
            }
            gr_FileSystemContent.SelectedItems.Clear();
            Action = ControlAction.SwitchToPlaylist;
        }

        private void HighLiteLastOpenedFolder()
        {
            // При переходе на уровень вверх по файловой системе подсветим 
            //последний открывавшийся фолдер и прокрутим Grid так чтобы он был виден
            string s = _parentFolderName; ;
            var parentFolder = _files.FirstOrDefault(item => item.Nature == FileNature.Directory && item.Name == _parentFolderName);
            if (parentFolder == null)
                return;
            parentFolder.JustClosed = true;
            gr_FileSystemContent.ScrollIntoView(parentFolder);

        }



        private async void UpdateFilesIcons(CancellationToken token)
        {
            if (_server.DisplayFolderPictures == false || _server.MusicCollectionFolder.Length == 0)
                return;
            try
            {
                _fileIconsUpdating = true;
                foreach (File file in _files.Where(f=>f.Nature== FileNature.Directory))
                {
                    token.ThrowIfCancellationRequested();
                    var PathToAlbumArt = new StringBuilder(_currentPath.Replace('/', '\\'));
                    if (PathToAlbumArt.Length>0 && !PathToAlbumArt.ToString().EndsWith(@"\"))
                        PathToAlbumArt.Append(@"\");
                    PathToAlbumArt.Append(file.Name);
                    await file.AlbumArt.LoadImageData(_server.MusicCollectionFolder, PathToAlbumArt.ToString(), _server.AlbumCoverFileNames);
                    ((IProgress<File>)_progress).Report(file);
                }
            }
            catch (OperationCanceledException)
            {
            }
            _fileIconsUpdating = false;
            _ThreadEvent.Set();
            //            _ThreadEvent.Reset();
            return;
        }

        private async void SetFileIcon(File file)
        {
            await file.AlbumArt.UpdateImage();
            file.AlbumArtWidth = new GridLength(60);
        }


        //private async void UpdateFilesIcons(CancellationToken token)
        //{
        //    var aa = new AlbumArt();

        //    if (_server.DisplayFolderPictures == false || _server.MusicCollectionFolder.Length == 0)
        //        return;
        //    try
        //    {
        //        _fileIconsUpdating = true;
        //        foreach (File file in _files.Where(f => f.Nature == FileNature.Directory))
        //        {
        //            token.ThrowIfCancellationRequested();

        //            var fullPathToAlbumArt = new StringBuilder(_server.MusicCollectionFolder);
        //            fullPathToAlbumArt.Append(@"\").Append(_currentPath.Replace('/', '\\'));
        //            if (!fullPathToAlbumArt.ToString().EndsWith(@"\"))
        //                fullPathToAlbumArt.Append(@"\");
        //            fullPathToAlbumArt.Append(file.Name);

        //            IRandomAccessStream fileStream = await GetFolderImageStream(fullPathToAlbumArt.ToString());
        //            ((IProgress<FileIconParams>)_progress).Report(new FileIconParams(fileStream, file));
        //        }

        //    }
        //    catch (OperationCanceledException)
        //    {

        //    }
        //    _fileIconsUpdating = false;
        //    _ThreadEvent.Set();
        //}


        //private async void SetFileIcon(FileIconParams iconParams)
        //{
        //    if (iconParams.Stream == null)
        //        return;

        //    BitmapImage bmi = new BitmapImage();
        //    await bmi.SetSourceAsync(iconParams.Stream);
        //    iconParams.File.ImageSource = bmi;

        //    iconParams.File.AlbumArtWidth = new GridLength(60);
        //}



        private async Task<IRandomAccessStream> GetFolderImageStream(string PathToAlbumArt)
        {
            StorageFile file = null;
            string[] CoverFileNames = _server.AlbumCoverFileNames.Split(';');

            foreach (string albumCoverFilename in CoverFileNames)
            {
                try
                {
                    var fileName = new StringBuilder(PathToAlbumArt).Append(@"\").Append(albumCoverFilename);
                    file = await StorageFile.GetFileFromPathAsync(fileName.ToString());
                    break;
                }
                catch (FileNotFoundException)
                {
                    //sb.Remove(pathlength, albumcoverfilename.Length);
                }
            }

            if (file == null)
                return null;
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            if (fileStream.Size == 0)
                return null;
            return fileStream;
            
        }

        private struct FileIconParams
        {
            public FileIconParams(IRandomAccessStream stream, File file)
            {
                Stream = stream;
                File = file;
            }
            public IRandomAccessStream Stream;
            public File File;
        }
    } // class TrackDirectory
}
