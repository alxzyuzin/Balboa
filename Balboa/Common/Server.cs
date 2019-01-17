/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс обеспечивает взвимодействие с MPD сервером
 *
  --------------------------------------------------------------------------*/

using Balboa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Sockets;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Balboa
{

    enum ack
    {
        ACK_ERROR_NOT_LIST = 1,
        ACK_ERROR_ARG = 2,
        ACK_ERROR_PASSWORD = 3,
        ACK_ERROR_PERMISSION = 4,
        ACK_ERROR_UNKNOWN = 5,
        ACK_ERROR_NO_EXIST = 50,
        ACK_ERROR_PLAYLIST_MAX = 51,
        ACK_ERROR_SYSTEM = 52,
        ACK_ERROR_PLAYLIST_LOAD = 53,
        ACK_ERROR_UPDATE_ALREADY = 54,
        ACK_ERROR_PLAYER_SYNC = 55,
        ACK_ERROR_EXIST = 56,
    };

    public delegate void ConnectionStatusChangedEventHandler(object sender, string connectionStatus);

    public delegate void EventHandler<TServerEventArgs>(object sender, TServerEventArgs eventArgs);
    
    public delegate void DataReadyEventHandler<MpdResponse>(object sender, MpdResponse eventArgs);

    public class Server : INotifyPropertyChanged, IDisposable
    {
        private const string _modName = "Server.cs";

        #region Events
        public event DataReadyEventHandler<MpdResponse> DataReady;

        /// <summary>
        /// Уведомление об изменении значения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

         /// <summary>
        /// Уведомление об ошибке в процессе обмена данными с сервером
        /// </summary>
        public event EventHandler Error;

        private async Task NotifyError(string errormessage)
        {
            if (Error != null)
            {
                ServerEventArgs args = new ServerEventArgs(EventType.Error,errormessage);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { Error(this, args); });
            }
        }

        /// <summary>
        /// Уведомление о критической ошибке в процессе обмена данными с сервером
        /// </summary>
        public event EventHandler CriticalError;

        private async void NotifyCriticalError(string errormessage)
        {
            if (CriticalError != null)
            {
                ServerEventArgs args = new ServerEventArgs(EventType.CriticalError, errormessage);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { CriticalError(this, args); });
            }
        }
        /// <summary>
        /// Уведомление об изменении статуса соединения с сервером
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        /// <summary>
        /// Уведомление о завершении выполнения команды
        /// </summary>
        public event EventHandler CommandCompleted;

        private async void NotifyCommandCompletion(string command, string result, string message)
        {
            if (CommandCompleted != null)
            {
                ServerEventArgs args = new ServerEventArgs(EventType.CommandCompleted, command,result,message);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { CommandCompleted(this, args); }); 
            }
        }
#endregion

#region Fields
        private CancellationTokenSource _tokenSource;
        private string _errorMessage = string.Empty;
        private AppSettings _appSettings = new AppSettings();
        private Progress<MpdResponse> _status;
        private MpdResponse _currentResponse = new MpdResponse();
        private Connection _Connection = new Connection();
        private ManualResetEvent _ThreadEvent = new ManualResetEvent(false);
        private object _Lock = new object();


        //---------------------------------------------------------------------------------
        ResourceLoader _resldr = new ResourceLoader();
        private DispatcherTimer     _timer;

        private MainPage            _mainPage;

        
        private Queue<MpdCommand>   _CommandQueue = new Queue<MpdCommand>();
        private Queue<MpdCommand>   _SentCommandQueue = new Queue<MpdCommand>();


//        private Statistic                                  _Statistics;
//        private Status                                     _Status;
//        private CurrentSong                                _CurrentSong;
//        private ObservableObjectCollection<Track>          _Playlist;
        private ObservableObjectCollection<File>           _SavedPlaylists;
//        private ObservableObjectCollection<CommonGridItem> _Genres;
//        private ObservableObjectCollection<CommonGridItem> _Artists;
//        private ObservableObjectCollection<CommonGridItem> _Albums;
        private ObservableObjectCollection<Track>          _Tracks;

        private string _Response;

        #endregion

        #region Properties
        public bool? DisplayFolderPictures => _appSettings.DisplayFolderPictures;
        public string ConnectionStatus => _Connection.Status;

        public string ConnectionState
        {
            set
            {
                if (_strConnectionState != value)
                {
                    _strConnectionState = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionState)));
                }
            }
            get { return _strConnectionState; }

        }



        private string _response;
        public string Response
        {
            get { return _response; }
            private set
            {
                if (_response!=value)
                {
                    _response = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Response)));
                }
            }

        }
        public AppSettings Settings { get { return _appSettings; } }
        public string MusicCollectionFolder => _appSettings.MusicCollectionFolder;
        public string AlbumCoverFileNames => _appSettings.AlbumCoverFileName;

        
        public string PlaylistName { get; set; }
        
        public bool Initialized { get; private set; } = false;
        private string      _strConnectionState;
        
        public bool          IsConnected         { get { return _Connection.IsActive; } }

        public string   Host                { get; set; }
        public string   Port                { get; set; } 
        public string   Password            { get; set; }
        public int      ViewUpdateInterval  { get; set; } = 500;


        public bool          IsRunning           { get; private set; }=false;
//        public Status        StatusData         { get { return _Status; } }
//        public Statistic     StatisticData      { get { return _Statistics; } }
//        public CurrentSong   CurrentSongData { get { return _CurrentSong; } }

       //
        

//        public ObservableObjectCollection<Track>  PlaylistData { get { return _Playlist; } }
        public ObservableObjectCollection<File>   SavedPlaylistsData { get { return _SavedPlaylists; } }
//        public ObservableObjectCollection<CommonGridItem> Artists { get { return _Artists; } }
//        public ObservableObjectCollection<CommonGridItem> Genres { get { return _Genres; } }
//        public ObservableObjectCollection<CommonGridItem> Albums { get { return _Albums; } }
        public ObservableObjectCollection<Track> Tracks { get { return _Tracks; } }

        #endregion

        public Server(MainPage mainPage)
        {
            _appSettings.Restore();

            if (_appSettings.InitialSetupDone)
            {
                Host = _appSettings.Server;
                Port = _appSettings.Port;
                Password = _appSettings.Password;
                ViewUpdateInterval = int.Parse(_appSettings.ViewUpdateInterval, System.Globalization.CultureInfo.InvariantCulture);
                //MusicCollectionFolder = _appSettings.MusicCollectionFolder;
                //AlbumCoverFileNames = _appSettings.AlbumCoverFileName;
                //DisplayFolderPictures = (bool)_appSettings.DisplayFolderPictures;
                Initialized = true;
            }

            _status = new Progress<MpdResponse>(ReportStatus);

            //-------------------------------------------------------------------------------------

            _mainPage = mainPage;

//            _Statistics =   new Statistic(_mainPage);
//            _Status =       new Status(_mainPage);
//            _CurrentSong =  new CurrentSong(_mainPage);

//            _Playlist =     new ObservableObjectCollection<Track>(_mainPage);
            _SavedPlaylists = new ObservableObjectCollection<File>(_mainPage);
//            _Artists =      new ObservableObjectCollection<CommonGridItem>(_mainPage);
//            _Genres =       new ObservableObjectCollection<CommonGridItem>(_mainPage);
//            _Albums =       new ObservableObjectCollection<CommonGridItem>(_mainPage);
            _Tracks =       new ObservableObjectCollection<Track>(_mainPage);

            _Connection.PropertyChanged += (object obj, PropertyChangedEventArgs args )=> 
                                    {
                                        if (args.PropertyName == nameof(Connection.Status))
                                            ConnectionStatusChanged?.Invoke(this, _Connection.Status);
                                    };
        }

        #region Методы
        /// <summary>
        /// Запуск процесса взаимодествия с сервером
        /// </summary>
        public async void Start()
        {
            SetInitialState();
            if (await _Connection.Open(Host, Port, Password))
            { 
                //CreateTimer(ViewUpdateInterval);
                _timer = new DispatcherTimer();
                _timer.Tick += (object sender, object e) => { Status(); };
                _timer.Interval = new TimeSpan(0, 0, 0, 0, ViewUpdateInterval);
                _timer.Start();

                _tokenSource = new CancellationTokenSource();
                CancellationToken token = _tokenSource.Token;
                WorkItemHandler communicator = delegate { ExecuteCommands(token); };
                await ThreadPool.RunAsync(communicator, WorkItemPriority.High, WorkItemOptions.TimeSliced);
            }

           
        }
        /// <summary>
        /// Остановка прцесса взаимодествия с сервером
        /// </summary>
        /// <returns></returns>
        public void Halt()
        {
            _timer?.Stop();        // Останавливаем тамер (прекращаем отправку команд серверу)
            _tokenSource?.Cancel();
            // Ставим в очередь на отправку команду статус
            //  Процесс может находиться в состоянии ожидания , в котором не выполняется 
            // цикл обработки команд из очереди
            // Постановкой команды в очередь мы будим процесс и заставляем проверить CancellationToken

            Status();
            _Connection.Close();
                //_Status.ExtendedStatus = "";
        }
        /// <summary>
        /// Перезапуск процесса взаимодествия с сервером
        /// </summary>
        /// <returns></returns>
        public void Restart()
        {
            //StatusData.State = "restart";
            Halt();
            Start();
        }

        private void ReportStatus(MpdResponse response)
        {
            DataReady?.Invoke(this, response );
        }

        //private void Connection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "IsActive")
        //    {
        //        NotifyConnectionStatusChanged(_Connection.IsActive);
        //        ConnectionState = _Connection.Status;
        //    }

        //    if (e.PropertyName == "Status")
        //    {
        //        NotifyConnectionStatusChanged(_Connection.IsActive);
        //        ConnectionState = _Connection.Status;
        //    }
        //}

        private void SetInitialState()
        {
            _CommandQueue.Clear();
            _SentCommandQueue.Clear();
            _Response = string.Empty;
//            _Playlist.ClearAndNotify();
            _SavedPlaylists.ClearAndNotify();
//            _Artists.ClearAndNotify();
//            _Genres.ClearAndNotify();
//            _Albums.ClearAndNotify();
            _Tracks.ClearAndNotify();
         }
        /// <summary>
        ///  Обработчик команд из очери команд
        /// </summary>
        /// <returns></returns>
        private async void ExecuteCommands(CancellationToken token)
        {
            // Берём очередную команду из очереди
            // Отправляем запрос на сервер
            // Читаем ответ
            // Разбираем ответ и заполняем данными 
            try
            { 
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    MpdCommand command = null;
                    lock (_Lock)
                    {
                       if (_CommandQueue.Count > 0)
                       { // Есть команды к выполнению. Забираем команду из очереди  и разморахиваем процесс
                           command = _CommandQueue.Dequeue();
                            _currentResponse.Init(command);
                           _ThreadEvent.Reset();
                       }
                    }
                    if (command == null)
                    { // Если нет команд к выполнению замораживаем процесс
                        _ThreadEvent.WaitOne();
                    }
                    else
                    {
                        await _Connection.SendCommand(command.FullSyntax);
                        _SentCommandQueue.Enqueue(command);
                        _Response += await _Connection.ReadResponse(); ;
                        _Response = await ParceResponse(_Response);
                    }
                } // End while
            }
            catch (OperationCanceledException)
            {
//                ((IProgress<OperationStatus>)_progress).Report(status);
                IsRunning = false;
            }
            catch (Exception ex)
            {
                Type type = ex.GetType();
                
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                    NotifyCriticalError(_resldr.GetString("CommunicationError") + Utilities.GetExceptionMsg(ex));
                var er = SocketError.GetStatus(ex.HResult);
            }
        }

        /// <summary>
        /// Разбирает ответ от сервера
        /// По неизвестной причине в ответ на команду от сервера может прийти не полный ответ
        /// При отправке следующе команды можно получить продолжение ответа на предыдущую комаду
        /// при этом не обязательно овет на предыдущую команду будет заверщён.
        /// 
        /// Обработка ответов сервера выполняется по следующему алгоритму.
        /// Все ответы полученные от сервера помещаются в буфер.
        /// После полученя каждого ответа анализируется содержимое буфера.
        /// Если буфер содержит полны ответ то этот полный ответ связывается с соответствующе командой
        /// и передается на обработку. 
        /// 
        /// </summary>
        /// <param name="responce"></param>
        /// <returns></returns>
        private async Task<string> ParceResponse(string response)
        {
            ResponseKeyword keyword = ResponseKeyword.Empty;
            string currentresponse = string.Empty;


            // Oтвет при ошибке начинается с ASC и заканчивается \n
            // Ответ при нормальном завершении заканчивается OK\n

            // Проверка 1
            // Строка начинается с символов OK\n - это значит что в начале строки содержится ответ 
            // об успешном выполнении команды не возвращающей данных
            // Просто убираем этот ответ из входной строки
            if (response.StartsWith("OK\n", StringComparison.Ordinal))
            {
                keyword = ResponseKeyword.OK;
                currentresponse = response.Substring(0, 3);
                response = response.Remove(0, 3);
            }

            // Проверка 2
            // Строка содержит символы \nOK\n - это значит что строка содержит полный ответ 
            // об успешном выполнении команды возвращающей данные
            // Забираем этот ответ из входной строки и разбираем его
            if (response.Contains("\nOK\n"))
            {
                keyword = ResponseKeyword.OK;
                //Забираем из входной строки подстроку от начала до до символов ОК (вместе с ОК)
                int nextresponsestart = response.IndexOf("\nOK\n", StringComparison.Ordinal) + 4;
                // currentresponce - содержит полный ответ от сервера
                currentresponse = response.Substring(0, nextresponsestart);
                response = response.Remove(0, nextresponsestart);
            }

            // Проверка 3
            // Строка содержит символы ACK - это значит что строка содержит ответ с информацией 
            // об ошибке при выполнении команды  
            // Забираем этот ответ из входной строки и cообщаем об ошибке
            if (response.StartsWith("ACK", StringComparison.Ordinal))
            {
                keyword = ResponseKeyword.ACK;
                int newresponsestart = response.IndexOf("\n", StringComparison.Ordinal) + 1;
                currentresponse = response.Substring(0, newresponsestart);
                response = response.Remove(0, newresponsestart);
            }
            if (keyword != ResponseKeyword.Empty)
            {
                MpdCommand command = _SentCommandQueue.Dequeue();
                MpdResponse mpdresp = new MpdResponse(keyword, command, currentresponse);
                ((IProgress<MpdResponse>)_status).Report(mpdresp);
            }

            return response;
        }

        /// <summary>
        /// Read server responces and convert them into data objects
        /// </summary>
        private async Task HandleResponse(MpdResponseCollection response)
        {
            
            try
            {
                switch (response.Command.Op)
                {
//                    case "update":  // в ответ на команду сервер возвращает" "updating_db: 1\n"  (Реализовано)
//                    case "stats": _Statistics.Update(response); break;
//                    case "status": _Status.Update(response); break;       // Реализовано
 //                   case "currentsong": _CurrentSong.Update(response); break;
                    case "list":
                        switch (response.Command.Argument1)
                        {
//                            case "genre": _Genres.Update(response); break;
//                            case "artist": _Artists.Update(response); break;
//                            case "album": _Albums.Update(response); break;
                            case "title": _Tracks.Update(response); break;
                        }
                        break;
//                    case "search": _Tracks.Update(response); break;
//                    case "playlistinfo": _Playlist.Update(response); break;
                    case "listplaylists": _SavedPlaylists.Update(response); break;
                    case "config":
                        break;
                    default: break;
                }
                if (response.Command.Op != "status")
                {
                    NotifyCommandCompletion(response.Command.Op, "OK", response.Error);
                    Response = response.Command.FullSyntax;
                }
            }
            catch (BalboaNullValueException be)
            {
//                _Terminating = true;
                _errorMessage = string.Format(_resldr.GetString("NullValueExceptionMsg"),
                                                be.VariableName, be.MethodName, be.FileName, be.LineNumber);
                throw be;
            }
            catch (BalboaException be)
            {
                //_Terminating = true;
                _errorMessage = string.Format(_resldr.GetString("BalboaExceptionMsg"),
                                                 be.MethodName, be.FileName, be.LineNumber, be.Message);
                throw be;
            }
            catch (Exception e)
            {
                //_Terminating = true;
                string exceptionMessage  = e.Message.Contains("\r\n") ? e.Message.Substring(0, e.Message.IndexOf("\r\n")) : e.Message;
                _errorMessage = string.Format(_resldr.GetString("UnexpectedServerError"), exceptionMessage);
                throw e;
            }

        }

        #endregion

  

        #region Раздел с утилитами

        /// <summary>
        /// Добавляет команду в очередь команд
        /// </summary>
        private void AddCommand(MpdCommand command)
        {
            if (_Connection.IsActive)
            { 
                lock( _Lock)
                {
                    _CommandQueue.Enqueue(command);
                    _ThreadEvent.Set();
                }
            }
        }

        #endregion

        #region Protocol commands

        // Не реализованные команды

        //delete[{POS} | {START:END}]
        //Deletes a song from the playlist.


        // deleteid {SONGID }
        // Deletes the song SONGID from the playlist

        // move[{FROM} | {START:END}] {TO}
        // Moves the song at FROM or range of songs at START:END to TO in the playlist. [6]

        // moveid { FROM} {TO}
        // Moves the song with FROM(songid) to TO(playlist index) in the playlist.If TO is negative, it is relative to the current song in the playlist (if there is one). 

        // The commands are in the order in which they appear in the
        // protocol spec.
        #region Mount commands

        public void ListMounts()
        {
            AddCommand(new MpdCommand("listmounts"));
        }
        
        #endregion

        #region Admin commands

        /// <summary>
        /// update [URI]
        /// Updates the music database: find new files, remove deleted files, update modified files.
        /// URI is a particular directory or song/file to update.If you do not specify it, everything is updated.
        /// Prints "updating_db: JOBID" where JOBID is a positive number identifying the update job.You can read the current job id in the status response.
        /// </summary>
        public void Update()
        {
            AddCommand(new MpdCommand("update"));
        }

        #endregion

        #region Informational commands
        /// <summary>
        /// Получение статистической информации
        /// </summary>
        public void Stats()
        {
            AddCommand(new MpdCommand("stats"));
        }

        /// <summary>
        /// Получение информации о статусе сервера
        /// </summary>
        public void Status()
        {
            AddCommand(new MpdCommand("status"));
        }

        #endregion

        #region Database commands
        /// <summary>
        /// list { TYPE} [FILTERTYPE] [FILTERWHAT] [...] [group] [GROUPTYPE] [...]
        /// Lists unique tags values of the specified type.TYPE can be any tag supported by MPD or file.
        /// Additional arguments may specify a filter like the one in the find command.
        /// The group keyword may be used (repeatedly) to group the results by one or more tags.
        ///    The following example lists all album names, grouped by their respective(album) artist: 
        /// list album group albumartist
        /// </summary>
        /// <param name="type"></param>
        public void List(string type)
        {
            AddCommand(new MpdCommand("list", type));
        }

        public void List(string type, string filterType, string filterWhat)
        {
            AddCommand(new MpdCommand("list", type, filterType, filterWhat));
        }
        /// <summary>
        ///         listfiles[URI]
        /// Lists the contents of the directory URI, including files are not recognized by MPD.
        /// URI can be a path relative to the music directory or an URI understood by one of the storage plugins.
        /// The response contains at least one line for each directory entry with the prefix "file: " or "directory: ",
        ///  and may be followed by file attributes such as "Last-Modified" and "size".  
        /// </summary>

        public void ListFiles()
        {
            AddCommand(new MpdCommand("listfiles"));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        public void ListFiles(string uri)
        {
            AddCommand(new MpdCommand("listfiles", uri));
        }
        

       
        #endregion

        #region Playlist commands
        
        /// <summary>
        /// Adds the file URI to the playlist (directories add recursively). URI can also be a single file. 
        /// </summary>
        public void Add(string path)
        {
            AddCommand(new MpdCommand("add", path));
        }

        /// <summary>
        /// addid {URI} [POSITION]
        /// Adds a song to the playlist(non-recursive) and returns the song id.
        /// </summary>
        public void AddId(string path, int position)
        {
            AddCommand(new MpdCommand("addid", path, position));
        }

        /// <summary>
        /// Clears the current playlist. 
        /// </summary>
        public void Clear()
        {
            AddCommand(new MpdCommand("clear"));
        }

        /// <summary>
        /// Получение информации о текущем треке
        /// </summary>
        public void CurrentSong()
        {
            AddCommand(new MpdCommand("currentsong"));
        }

        /// <summary>
        /// Удаляет Track c указанныv Id из текущего Playlist'а
        /// </summary>
        public void DeleteId(int id)
        {
            AddCommand(new MpdCommand("deleteid", id));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListPlaylistInfo(string playlist)
        {
            AddCommand(new MpdCommand("listplaylistinfo", playlist));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load(string name)
        {
            AddCommand(new MpdCommand("load", name));
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ls")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ls")]
        public void LsInfo()
        {
            AddCommand(new MpdCommand("lsinfo"));
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ls")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ls")]
        public void LsInfo(string uri)
        {
            AddCommand(new MpdCommand("lsinfo", uri));
        }

        /// <summary>
        /// search {TYPE} {WHAT} [...] [sort TYPE] [window START:END]
        /// Searches for any song that contains WHAT.Parameters have the same meaning as for find, except that search is not case sensitive.
        /// </summary>
        public void Search(string type, string what)
        {
            AddCommand(new MpdCommand("search", type, what));
        }

        /// <summary>
        ///  searchadd {TYPE } {WHAT} [...]
        /// Searches for any song that contains WHAT in tag TYPE and adds them to current playlist.
        ///Parameters have the same meaning as for find, except that search is not case sensitive.
        /// </summary>

        public void SearchAdd(string type, string what)
        {
            AddCommand(new MpdCommand("searchadd", type, what));
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveId(int id, int position)
        {
            AddCommand(new MpdCommand("moveid", id, position));
        }

        /// <summary>
        /// playlistinfo[[SONGPOS] | [START: END]]
        /// Displays a list of all songs in the playlist, or if the optional argument is given, 
        /// displays information only for the song SONGPOS or the range of songs START:END[6]
        /// </summary>
        public void PlaylistInfo()
        {
            AddCommand(new MpdCommand("playlistinfo"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Rename(string oldName, string newName)
        {
            AddCommand(new MpdCommand("rename", oldName, newName));
        }

        /// <summary>
        /// Removes the playlist NAME.m3u from the playlist directory. 
        /// </summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Rm")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rm")]
        public void Rm(string name)
        {
            AddCommand(new MpdCommand("rm", name));
        }
        
        /// <summary>
        /// Сохраняет текущий Playlist с указанным именем
        /// </summary>
        public void Save(string name)
        {
            AddCommand(new MpdCommand("save", name));
        }
        
        /// <summary>
        /// Shuffles the current playlist. START:END is optional and specifies a range of songs.  
        /// </summary>
        public void Shuffle()
        {
            AddCommand(new MpdCommand("shuffle"));
        }

        /// <summary>
        /// Prints a list of the playlist directory.
        /// After each playlist name the server sends its last modification time as attribute "Last-Modified"
        ///  in ISO 8601 format.To avoid problems due to clock differences between clients and the server, 
        /// clients should not compare this value with their local clock.
        /// </summary>
        public void ListPlaylists()
        {
            AddCommand(new MpdCommand("listplaylists"));
        }


        #endregion

        #region Playback commands

        /// <summary>
        /// Plays next song in the playlist. 
        /// </summary>
        public void Next()
        {
            AddCommand(new MpdCommand("next"));
        }
        
        /// <summary>
        /// Toggles pause/resumes playing, PAUSE is 0 or 1
        /// </summary>
        public void Pause()
        {
            AddCommand(new MpdCommand("pause"));
        }
        
        /// <summary>
        /// Begins playing the playlist at song number SONGPOS. 
        /// </summary>
        public void Play()
        {
            AddCommand(new MpdCommand("play"));
        }
        
        /// <summary>
        /// Begins playing the playlist at song SONGID. 
        /// </summary>
        public void PlayId(int id)
        {
            AddCommand(new MpdCommand("playid", id));
        }

        /// <summary>
        /// Plays previous song in the playlist. 
        /// </summary>
        public void Previous()
        {
            AddCommand(new MpdCommand("previous"));
        }
        
        /// <summary>
        /// Sets random state to STATE, STATE should be 0 or 1. 
        /// </summary>
        public void Random(bool to)
        {
            AddCommand(new MpdCommand("random", to));
        }
        
        /// <summary>
        /// Sets repeat state to STATE, STATE should be 0 or 1. 
        /// </summary>
        public void Repeat(bool to)
        {
            AddCommand(new MpdCommand("repeat", to));
        }

        /// <summary>
        /// Seeks to the position TIME (in seconds; fractions allowed) of entry SONGPOS in the playlist. 
        /// </summary>
        public void Seek(int songIndex, int position)
        {
            AddCommand(new MpdCommand("seek", songIndex, position));
        }

        ///<summary
        /// Seeks to the position TIME(in seconds; fractions allowed) within the current song.
        /// If prefixed by '+' or '-', then the time is relative to the current playing position. 
        ///  seekcur {TIME}
        ///</summary

        public void SeekCurrent(string position)
        {
            AddCommand(new MpdCommand("seekcur", position));
        }

        /// <summary>
        /// Sets volume to VOL, the range of volume is 0-100. 
        /// </summary>
        public void SetVolume(int volume)
        {
            AddCommand(new MpdCommand("setvol", volume));
        }

        /// <summary>
        /// Stops playing. 
        /// </summary>
        public void Stop()
        {
            AddCommand(new MpdCommand("stop"));
        }

        #endregion

        #region Outputs
        /// <summary>
        /// 
        /// </summary>
        public void Outputs()
        {
            AddCommand(new MpdCommand("outputs"));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void EnableOutput(int index)
        {
            AddCommand(new MpdCommand("enableoutput", index));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void DisableOutput(int index)
        {
            AddCommand(new MpdCommand("disableoutput", index));
        }
        
        /// <summary>
        /// 
        /// </summary>
        #endregion

        #region Miscellaneous commands
/*
        public void Password(string password)
        {
            AddCommand(new MPDCommand("password", password));
        }
*/
        #endregion

        #region Playback Options

        /// <summary>
        /// Sets consume state to STATE, STATE should be 0 or 1. 
        /// When consume is activated, each song played is removed from playlist
        /// </summary>
        public void Consume(bool on)
        {
            AddCommand(new MpdCommand("consume", on ? "1" : "0"));
        }

        /// <summary>
        /// Sets single state to STATE, STATE should be 0 or 1. 
        /// When single is activated, playback is stopped after current song, or song is repeated
        ///  if the 'repeat' mode is enabled. 
        /// </summary>
        public void Single(bool on)
        {
            AddCommand(new MpdCommand("single", on ? "1" : "0"));
        }

        public void Config()
        {
            AddCommand(new MpdCommand("config"));
        }


        #endregion

        #region Helpers

        #endregion

        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
             }
            // free native resources
            _ThreadEvent.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }   // Class End
}
