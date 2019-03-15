using Balboa.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public sealed partial class TrackDirectoryPanel : UserControl, INotifyPropertyChanged, IDataPanel, IDataPanelInfo,
                                                                   IRequestAction, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ActionRequestedEventHandler RequestAction;

        private ResourceLoader _resldr = new ResourceLoader();
        private Server _server;
      
        private string _currentPath = string.Empty;
        private string _parentFolderName = string.Empty;
        private string _newPathChunck = string.Empty;

        private GridViewItem _gridViewItemGotFocus;
        public ObservableCollection<File> Files => _files;
        private ObservableCollection<File> _files = new ObservableCollection<File>();
        private bool _fileIconsUpdating;

        private Progress<File> _progress;
        private ManualResetEvent _ThreadEvent = new ManualResetEvent(false);
        private CancellationTokenSource _tokenSource;

        private Visibility _emptyContentMessageVisibility = Visibility.Collapsed;
        public Visibility EmptyContentMessageVisibility
        {
            get { return _emptyContentMessageVisibility; }
            set
            {
                if (_emptyContentMessageVisibility != value)
                {
                    _emptyContentMessageVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmptyContentMessageVisibility)));
                }
            }
        }

        private bool _appbtnUpIsEnabled;
        public bool AppbtnUpIsEnabled
        {
            get { return _appbtnUpIsEnabled; }
            set
            {
                if (_appbtnUpIsEnabled !=value)
                {
                    _appbtnUpIsEnabled = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(AppbtnUpIsEnabled)));
                }
            }
        }

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

        private string _dataPanelInfo;
        public string DataPanelInfo
        {
            get { return _dataPanelInfo; }
            set
            {
                if (_dataPanelInfo != value)
                {
                    _dataPanelInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataPanelInfo)));
                }
            }
        }

        private string _dataPanelElementsCount;
        public string DataPanelElementsCount
        {
            get { return _dataPanelElementsCount; }
            set
            {
                if (_dataPanelElementsCount != value)
                {
                    _dataPanelElementsCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataPanelElementsCount)));
                }
            }
        }

        public double TotalButtonWidth => AppBarButtons.Width;
        public Orientation Orientation { get; set; }

        public TrackDirectoryPanel()
        {
            InitializeComponent();
            _progress = new Progress<File>(SetFileIcon);
            RestoreStatus();
        }

        public TrackDirectoryPanel(Server server) : this()
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            _server = server;
            _server.DataReady += _server_DataReady;
            Unloaded += (object sender, RoutedEventArgs e)=> { SaveStatus(); };


            if (_currentPath.Length == 0)
                _server.LsInfo();
            else
                _server.LsInfo(_currentPath);
        }

        public void Init(Server server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

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

            if (mpdData.Keyword != ResponseKeyword.OK || mpdData.Command.Op != "lsinfo")
                return;
            {
                if (_fileIconsUpdating)
                {
                    _tokenSource.Cancel();
                    _ThreadEvent.WaitOne();
                    while (_fileIconsUpdating){ await Task.Delay(100); }
                }
                
                UpdateControlData(mpdData.Content);
                HighLiteLastOpenedFolder();
                if (_currentPath.Length > 0 && _newPathChunck.Length > 0)
                    _currentPath += "/";
                _currentPath += _newPathChunck;
                _newPathChunck = string.Empty;

                DataPanelInfo = $"Folder /{_currentPath}";

                AppbtnUpIsEnabled = _currentPath.Length>0 ? true : false;
                EmptyDirectoryMessage = ( _files.Count == 0 ) 
                                        ? string.Format(_resldr.GetString("NoAudioFilesInFolder"), "\""+_currentPath+ "\"") 
                                        : string.Empty;
                DataPanelElementsCount = $"{_files.Count.ToString()} items.";

                if (_server.DisplayFolderPictures == true && _server.MusicCollectionFolder.Length > 0)
                {
                    _tokenSource = new CancellationTokenSource();
                    CancellationToken token = _tokenSource.Token;
                    WorkItemHandler workhandler = delegate { UpdateFilesIcons(token); };

                    await ThreadPool.RunAsync(workhandler, WorkItemPriority.High, WorkItemOptions.TimeSliced);
                }
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
            
            EmptyContentMessageVisibility = _files.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
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
            {
                _parentFolderName = _currentPath.Substring(i+1);
                _currentPath = _currentPath.Remove(i);
            }
            else
            {
                _parentFolderName = _currentPath;
                _currentPath = string.Empty;
            }
             _server.LsInfo(_currentPath);
            // Eсли мы поднялись на самый верх по дереву каталогов отключим кнопку Up
            AppbtnUpIsEnabled = _currentPath.Length > 0 ? true : false;
            
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
   
            if (gr_FileSystemContent.SelectedItems.Count == 0)
            {
                var message = new Message( MsgBoxType.Info, _resldr.GetString("NoSelectedItemsToAdd"),
                                           MsgBoxButton.Continue, 150);
                RequestAction?.Invoke(this, new ActionParams(message));
                return;
            }
            foreach (File item in gr_FileSystemContent.SelectedItems)
            {
                string lp = path + item.Name;
                _server.Add(path + item.Name);
            }
            gr_FileSystemContent.SelectedItems.Clear();
            RequestAction?.Invoke(this, new ActionParams(ActionType.ActivateDataPanel).SetPanel<PlaylistPanel>(new PlaylistPanel(_server)));
        }

        private void appbtn_Home_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _currentPath = string.Empty;
            _server.LsInfo(_currentPath);
            // Отключим кнопку Up
            AppbtnUpIsEnabled = false;
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
            //if (_server.DisplayFolderPictures == false || _server.MusicCollectionFolder.Length == 0)
            //    return;
            try
            {
                _fileIconsUpdating = true;
                foreach (File file in _files.Where(f=>f.Nature== FileNature.Directory))
                {
                    token.ThrowIfCancellationRequested();
                    var PathToAlbumArt = new StringBuilder(_currentPath.Replace('/', '\\'));

                    if (PathToAlbumArt.Length > 0 && !PathToAlbumArt.ToString().EndsWith("\\"))
                        PathToAlbumArt.Append("\\");

                    PathToAlbumArt.Append(file.Name).Append('\\').Append("folder.jpj");
                    await file.AlbumArt.LoadImageData(_server.MusicCollectionFolder, PathToAlbumArt.ToString(), _server.AlbumCoverFileNames);
                   ((IProgress<File>)_progress).Report(file);
                }
            }
            catch (OperationCanceledException)
            {
            }
            _fileIconsUpdating = false;
            _ThreadEvent.Set();
            return;
        }

        private async void SetFileIcon(File file)
        {
            await file.AlbumArt.UpdateImage();
            file.AlbumArtWidth = new GridLength(60);
        }

        private void RestoreStatus()
        {
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
            Object value = LocalSettings.Values["CurrentMusicLibraryPath"];
            _currentPath = value as string ?? string.Empty;
        }

        private void SaveStatus()
        {
            ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
            LocalSettings.Values["CurrentMusicLibraryPath"] = _currentPath;
        }

        public void HandleUserResponse(MsgBoxButton pressedButton)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            _ThreadEvent.Dispose();
            _server.DataReady -= _server_DataReady;
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
