/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Класс обеспечивает взвимодействие с MPD сервером
 *
  --------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Balboa.Common;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Balboa
{
    public sealed class Server : INotifyPropertyChanged
    {
        #region Объявление событий

        /// <summary>
        /// Уведомление об изменении значения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private async void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); });
            }
        }
        /// <summary>
        /// Уведомление об ошибке в процессе обмена данными с сервером
        /// </summary>
        public delegate void ErrorEventHandler(object sender, ServerEventArgs e);

        public event ErrorEventHandler Error;

        private async void NotifyError(string errormessage)
        {
            if (Error != null)
            {
                ServerEventArgs args = new ServerEventArgs(errormessage);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { Error(this, args); });
            }
        }

        /// <summary>
        /// Уведомление о критической ошибке в процессе обмена данными с сервером
        /// </summary>
        
        public event ErrorEventHandler CriticalError;

        private async Task NotifyCriticalError(string errormessage)
        {
            if (CriticalError != null)
            {
                ServerEventArgs args = new ServerEventArgs(errormessage);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { CriticalError(this, args); });
            }
        }
        /// <summary>
        /// Уведомление об изменении статуса соединения с сервером
        /// </summary>
        public delegate void ConnectionStatusChangingEventHandler(object sender, ServerConnectionStatusEventArgs e);

        public event ConnectionStatusChangingEventHandler ConnectionStatusChanged;

        private async void NotifyConnectionStatusChanged(ConnectionStatus status)
        {
            if (ConnectionStatusChanged != null)
            {
                ServerConnectionStatusEventArgs args = new ServerConnectionStatusEventArgs(status);
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { ConnectionStatusChanged(this, args); });;
            }
        }

        /// <summary>
        /// Уведомление об изменении статуса сервера
        /// </summary>
        public delegate void StatusChangingEventHandler(object sender, ServerEventArgs e);

        public event StatusChangingEventHandler StatusChanged;

        private async void NotifyServerStatusChanged(string status)
        {
            if (StatusChanged != null)
            {
                ServerEventArgs args = new ServerEventArgs(status);
               
                await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { StatusChanged(this, args); }); ;
            }
        }


        /// <summary>
        /// Уведомление о завершении выполнения команды
        /// </summary>
        public delegate void CommandCompletionEventHandler(object sender, ServerCommandCompletionEventArgs e);
   //     public delegate void CommandCompletionEventHandler(string command, string result, string message);

        public event CommandCompletionEventHandler CommandCompleted;

        private async void NotifyCommandCompletion(string command, string result, string message)
        {
            if (CommandCompleted != null)
            {
                ServerCommandCompletionEventArgs args = new ServerCommandCompletionEventArgs();
                args.Command = command;
                args.Result = result;
                args.Message = message;
               await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate { CommandCompleted(this, args); }); ;
            }
        }
        #endregion

        #region Поля
        private DispatcherTimer     _timer;

        private string              _strConnectionState;
        private MainPage            _mainPage;
        private bool                _Terminating;

        private Connection          _Connection  = new Connection();
        private Queue<MPDCommand>   _CommandQueue = new Queue<MPDCommand>();
        private Queue<MPDCommand>   _SentCommandQueue = new Queue<MPDCommand>();

        private ManualResetEvent    _ThreadEvent = new ManualResetEvent(false);
        private object              _Lock = new object();

        private Statistic                                   _Statistics;
        private Status                                      _Status;
        private CurrentSong                                 _CurrentSong;
        private ObsevableObjectCollection<Track>            _Playlist;
        private ObsevableObjectCollection<File>             _FileList;
        private ObsevableObjectCollection<File>             _SavedPlaylists;
        private ObsevableObjectCollection<Output>           _Outputs;
        private ObsevableObjectCollection<CommonGridItem>   _Genres;
        private ObsevableObjectCollection<CommonGridItem>   _Artists;
        private ObsevableObjectCollection<CommonGridItem>   _Albums;
        private ObsevableObjectCollection<Track>            _Tracks;

        private string _Responce;

        private bool _filelistCancelUpdate = false;
        private bool _filelistUpdatInProcess = false;

        #endregion

        #region Свойства      

        public string        ConnectionState
        {
            set
            {
                _strConnectionState = value;
                NotifyPropertyChanged("ConnectionState");
            }
            get { return _strConnectionState; }
            
        }
        public bool          IsConnected         { get { return (_Connection.StatusID == ConnectionStatus.Connected)?true:false; } }
        public bool          IsDisconnected      { get { return (_Connection.StatusID == ConnectionStatus.Disconnected)?true:false; } }
        public string        Host                { get; set; }
        public string        Port                { get; set; } 
        public string        Password            { get; set; }
        public int           ViewUpdateInterval  { get; set; } = 500;
        public bool          IsRunning           { get; private set; }=false;
        public string        ErrorMessage        { get; private set; } = "";

        public Status        StatusData         { get { return _Status; } }
        public Statistic     StatisticData      { get { return _Statistics; } }
        public CurrentSong   CurrentSongData { get { return _CurrentSong; } }

        public string        MusicCollectionFolder { get; set; }
        public string        CurrentFolder         { get; set; }
        public string        AlbumCoverFileNames   { get; set; }
        public bool          DisplayFolderPictures { get; set; }

        public ObsevableObjectCollection<Track>  PlaylistData { get { return _Playlist; } }
        public ObsevableObjectCollection<File>   DirectoryData { get { return _FileList; } }
        public ObsevableObjectCollection<File>   SavedPlaylistsData { get { return _SavedPlaylists; } }
        public ObsevableObjectCollection<Output> OutputsData  { get { return _Outputs; } }
        public ObsevableObjectCollection<CommonGridItem> Artists { get { return _Artists; } }
        public ObsevableObjectCollection<CommonGridItem> Genres { get { return _Genres; } }
        public ObsevableObjectCollection<CommonGridItem> Albums { get { return _Albums; } }
        public ObsevableObjectCollection<Track> Tracks { get { return _Tracks; } }

        /*
            CustomDateNormalizer = new DateNormalizer();
            YearNormalizer = new DateNormalizer(new string[] { "YYYY" });
         */

        #endregion

        public Server(MainPage mainpage)
        {
            _mainPage = mainpage;

            _Statistics =   new Statistic(_mainPage);
            _Status =       new Status(_mainPage);
            _CurrentSong =  new CurrentSong(_mainPage);

            _Playlist =     new ObsevableObjectCollection<Track>(_mainPage);
            _FileList =     new ObsevableObjectCollection<File>(_mainPage);
            _SavedPlaylists = new ObsevableObjectCollection<File>(_mainPage);
            _Outputs =      new ObsevableObjectCollection<Output>(_mainPage);
            _Artists =      new ObsevableObjectCollection<CommonGridItem>(_mainPage);
            _Genres =       new ObsevableObjectCollection<CommonGridItem>(_mainPage);
            _Albums =       new ObsevableObjectCollection<CommonGridItem>(_mainPage);
            _Tracks =       new ObsevableObjectCollection<Track>(_mainPage);

            _Connection.PropertyChanged += Connection_PropertyChanged;

        }


        #region Методы
        /// <summary>
        /// Запуск процесса взаимодествия с сервером
        /// </summary>
        public async void Start()
        {
            SetInitialState();

            _Terminating = false;
            CreateTimer(ViewUpdateInterval);
            //   ThreadPool.RunAsync(delegate { ServerCommunicator(); }, WorkItemPriority.High, WorkItemOptions.TimeSliced);
            
            WorkItemHandler communicator = delegate{ ServerCommunicator(); };
            
            await ThreadPool.RunAsync(communicator, WorkItemPriority.High, WorkItemOptions.TimeSliced);

            _timer.Start();
        }
        /// <summary>
        /// Остановка прцесса взаимодествия с сервером
        /// </summary>
        /// <returns></returns>
        public async Task Halt()
        {
            if (!IsRunning)  // Если процесс не активет то сразу выходим
                return;
                            // иначе

            _timer.Stop(); // Останавливаем тамер (прекращаем отправку команд серверу)
            _Terminating = true;  // Говорим процессу что пора прекращать работу
            Status();               // Ставим в очередь на отправку команду статус
                                    //  Процесс может находиться в состоянии ожидания , в котором не выполняется 
                                    // цикл обработки команд из очереди и процесс не выполняет проверку 
                                    // переменной _Terminating
                                    // Постановкой команды в очередь мы будим процесс и заставляем проверить
                                    // значение переменной _Terminating
            do        // В цикле с интервалом 100 миллисекунд проверяем статус ппроцесса пока он не закончится
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            while (IsRunning);
        
         }
        /// <summary>
        /// Перезапуск процесса взаимодествия с сервером
        /// </summary>
        /// <returns></returns>
        public async Task Restart()
        {
            StatusData.State = "restart";
            await Halt();
            Start();
        }

        private void Connection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName =="StatusID")
            {
                NotifyConnectionStatusChanged(_Connection.StatusID);
                ConnectionState = _Connection.Status;
            }
        }
        /// <summary>
        ///  Процесс взаимодествия с сервером
        ///  Устанавливает соединение с сервером
        ///  Запускает обработку команд из очереди
        ///  После остановки обработки команд закрывает соединение с сервером
        /// </summary>
        private async void ServerCommunicator()
        {

            try
            {
                _Terminating = false;
                bool res = await _Connection.Open(Host, Port, Password);
                if (!res)
                {
                    await NotifyCriticalError(_Connection.Error);
                    _Terminating = true;
                }
                IsRunning = true;
                await ExecuteCommands();
                IsRunning = false;
                _Connection.Close();
            }
            catch(Exception e)
            {
                _Terminating = true;
                IsRunning = false;
                string exeptionmessage;
                _Connection.Close();
                if (e.Message.Contains("\r\n"))
                {
                    exeptionmessage = e.Message.Substring(0, e.Message.IndexOf("\r\n"));
                }
                else
                {
                    exeptionmessage = e.Message;
                }
                 await NotifyCriticalError("Server communication error\n" + exeptionmessage);

            }

        }

        private void SetInitialState()
        {
            _CommandQueue.Clear();
            _SentCommandQueue.Clear();
            _Responce = string.Empty;
            _Terminating = false;

            _Playlist.ClearAndNotify();
            _FileList.ClearAndNotify();
            _SavedPlaylists.ClearAndNotify();
            _Outputs.ClearAndNotify();
            _Artists.ClearAndNotify();
            _Genres.ClearAndNotify();
            _Albums.ClearAndNotify();
            _Tracks.ClearAndNotify();
         }
        /// <summary>
        ///  Обработчик команд из очери команд
        /// </summary>
        /// <returns></returns>
        private async Task ExecuteCommands()
        {
            // Берём очередную команду из очереди
            // Отправляем запрос на сервер
            // Читаем ответ
            // Разбираем ответ и заполняем данными 

            bool terminating = _Terminating;
            while (!terminating)
            { 
                MPDCommand command = null;
 
                lock (_Lock)
                {
                    terminating = _Terminating;

                    if (_CommandQueue.Count > 0)
                    { // Есть команды к выполнению. Забираем команду из очереди  и разморахиваем процесс
                        command = _CommandQueue.Dequeue();
                        _ThreadEvent.Reset();
                    }
                }
 
              
                if (!terminating)
                {
                    if (command == null)
                    { // Если нет команд к выполнению замораживаем процесс
                        _ThreadEvent.WaitOne();
                    }
                    else
                    {
                        string r;
                        try
                        {
                            await _Connection.SendCommand(command.FullSyntax);
                            _SentCommandQueue.Enqueue(command);
                            r = await _Connection.ReadResponce();
                            _Responce += r;

                            _Responce = ParceResponce(_Responce);
                        }
                        catch(Exception)
                        {
                            throw;
                        }
                    }
                }             
            } // End while
        }
        /// <summary>
        /// Разбирает ответ от сервера
        /// По неизвестной причине в ответ на команду от тсервера может прийти не полный ответ
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
        private string ParceResponce(string responce)
        {

            MPDResponce mpdresponce = new MPDResponce();
            // Oтвет при ошибке начинается с ASC и заканчивается \n
            // Ответ при нормальном завершении заканчивается OK\n

            // Проверка 1
            // Строка начинается с символов OK\n - это значит что в начале строки содержится ответ 
            // об успешном выполнении команды не возвращающей данных
            // Просто убираем этот ответ из входной строки
            if (responce.StartsWith("OK\n"))
                {
                    responce = responce.Remove(0, 3);
                    mpdresponce.Command = _SentCommandQueue.Dequeue();
                    mpdresponce.Keyword = MPDResponce.ResponceKeyword.OK;
                    Task.Run(()=>HandleResponse(mpdresponce));
                }

            // Проверка 2
            // Строка содержит символы \nOK\n - это значит что строка содержит полный ответ 
            // об успешном выполнении команды возвращающей данные
            // Забираем этот ответ из входной строки и разбираем его
            if (responce.Contains("\nOK\n"))
                {
                     //Забираем из входной строки подстроку от начала до до символов ОК (вместе с ОК)

                    int nextresponcestart = responce.IndexOf("\nOK\n") + 4;

                    string currentresponce = responce.Substring(0, nextresponcestart);
                    responce = responce.Remove(0, nextresponcestart);


                    string[] lines = currentresponce.Split('\n');
                    for (int i = 0; i < lines.Length - 2; i++)
                    {
                        mpdresponce.Add(lines[i]);
                    }
                    mpdresponce.Keyword = MPDResponce.ResponceKeyword.OK;
                    mpdresponce.Command = _SentCommandQueue.Dequeue();

                    Task.Run(() => HandleResponse(mpdresponce));
                 }

            // Проверка 3
            // Строка содержит символы ACK - это значит что строка содержит ответ с информацией 
            // об ошибке при выполнении команды  
            // Забираем этот ответ из входной строки и cообщаем об ошибке
            if (responce.StartsWith("ACK"))
                {
                    int newresponcestart = responce.IndexOf("\n") + 1;
                    string currentresponce = responce.Substring(0, newresponcestart);
                    responce = responce.Remove(0, newresponcestart);
                    NotifyError("Server return error : \n" + currentresponce);
                    return responce;
                }
 
            return responce;
        }

        public void TimerStart()
        {
            _timer.Start();
        }

        public void TimerStop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Read server responces and convert them into data objects
        /// </summary>
        private async Task HandleResponse(MPDResponce responce)
        {
            try
            {
                switch (responce.Command.Op)
                {
                    case "update":  // в ответ на команду сервер возвращает" "updating_db: 1\n"  (Реализовано)
                    case "stats": _Statistics.Update(responce); break;
                    case "status": _Status.Update(responce); break;       // Реализовано
                    case "currentsong": _CurrentSong.Update(responce); break;
                    case "lsinfo":
                        if (responce.Count > 0)  // TO DO обработать случай пустого каталога
                        {
                           string currentfolder = string.Empty;
                           if (CurrentFolder.Length != 0)
                              currentfolder = CurrentFolder + "\\";
                            // Создаём временный список файлов и заполняем его данными из ответв сервера
                            ObsevableObjectCollection<File> filelist = new ObsevableObjectCollection<File>(_mainPage); 
                            filelist.Update(responce);
                        
                            if (_filelistUpdatInProcess)
                            {
                                _filelistCancelUpdate = true;
                                do
                                {
                                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                                }
                                while (_filelistUpdatInProcess);
                                _filelistCancelUpdate = false;
                            }

                            await UpdateFileList(filelist);

                        }
                        break;
                    case "list":
                        switch (responce.Command.Argument1)
                        {
                            case "genre": _Genres.Update(responce); break;
                            case "artist": _Artists.Update(responce); break;
                            case "album": _Albums.Update(responce); break;
                            case "title": _Tracks.Update(responce); break;
                        }
                        break;
                    case "search": _Tracks.Update(responce); break;
                    case "playlistinfo": _Playlist.Update(responce); break;
                    case "listplaylists": _SavedPlaylists.Update(responce); break;
                    case "outputs":
                        if (responce.Count > 0)
                            _Outputs.Update(responce);
                        break;
                    case "config":
                        break;
                    default: break;
                }
                if (responce.Command.Op != "status")
                    NotifyCommandCompletion(responce.Command.Op, "OK", responce.Error);
            }
            catch
            {
                throw;

            }
         }



        private async Task UpdateFileList(ObsevableObjectCollection<File> filelist)
        {
          
            _filelistUpdatInProcess = true;

             _FileList.Clear();
            // переносим из временного списка в список файлов отображаемый пользователю только элементы с типом File и Directory
            foreach (File file in filelist)
                if (file.Type != FileType.Playlist)
                {
                    _FileList.Add(file);
                };
            // Сортируем элементы списка по типу так чтобы каталоги оказались в начале
            _FileList.Sort(new GenericComparer<File>("Type", GenericComparer<File>.SortOrder.Descending));
            // Добавляем картинки каталогов
            if (DisplayFolderPictures)
            {
                string currentfolder = string.Empty;
                if (CurrentFolder.Length != 0)
                    currentfolder = CurrentFolder + "\\";

                foreach (File file in _FileList)
                {
                    if (_filelistCancelUpdate)
                        break;
                    if (file.Type == FileType.Directory)
                    {
                        IRandomAccessStream fileStream = await Utils.GetFolderImageStream(MusicCollectionFolder, currentfolder + file.Name, AlbumCoverFileNames);
                        if (fileStream != null)
                        {
                            await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                async delegate
                                {
                                    BitmapImage bmi = new BitmapImage();
                                    await bmi.SetSourceAsync(fileStream);
                                    file.ImageSource = bmi;
                                });
                        }
                    }
                }
                _filelistUpdatInProcess = false;
            }
        }
        #endregion

        private void CreateTimer(int interval)
        {
            _timer =  new DispatcherTimer();
            _timer.Tick += OnTimerTick;
            _timer.Interval = new System.TimeSpan(0, 0, 0, 0, interval);
        }

        private void OnTimerTick(object sender, object e)
        {
            Status();
        }

          #region Раздел с утилитами

        /// <summary>
        /// Добавляет команду в очередь команд
        /// </summary>
        private void AddCommand(MPDCommand command)
        {
            if (_Connection.StatusID == ConnectionStatus.Connected)
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
            AddCommand(new MPDCommand("listmounts"));
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
            AddCommand(new MPDCommand("update"));
        }

        #endregion

        #region Informational commands
        /// <summary>
        /// Получение статистической информации
        /// </summary>
        public void Stats()
        {
            AddCommand(new MPDCommand("stats"));
        }

        /// <summary>
        /// Получение информации о статусе сервера
        /// </summary>
        public void Status()
        {
            //m_StatusUpdateEnqueued = true;
            AddCommand(new MPDCommand("status"));
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
            AddCommand(new MPDCommand("list", type));
        }

        public void List(string type, string filtertype, string filterwhat)
        {
            AddCommand(new MPDCommand("list", type, filtertype, filterwhat));
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
            AddCommand(new MPDCommand("listfiles"));
        }

        public void ListFiles(string uri)
        {
            AddCommand(new MPDCommand("listfiles", uri));
        }
        

       
        #endregion

        #region Playlist commands
        
        /// <summary>
        /// Adds the file URI to the playlist (directories add recursively). URI can also be a single file. 
        /// </summary>
        public void Add(string path)
        {
            AddCommand(new MPDCommand("add", path));
        }

        /// <summary>
        /// addid {URI} [POSITION]
        /// Adds a song to the playlist(non-recursive) and returns the song id.
        /// </summary>
        public void AddId(string path, int position)
        {
            AddCommand(new MPDCommand("addid", path, position));
        }

        /// <summary>
        /// Clears the current playlist. 
        /// </summary>
        public void Clear()
        {
            AddCommand(new MPDCommand("clear"));
        }

        /// <summary>
        /// Получение информации о текущем треке
        /// </summary>
        public void CurrentSong()
        {
            AddCommand(new MPDCommand("currentsong"));
        }

        /// <summary>
        /// Удаляет Track c указанныv Id из текущего Playlist'а
        /// </summary>
        public void DeleteId(int id)
        {
            AddCommand(new MPDCommand("deleteid", id));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListPlaylistInfo(string playlist)
        {
            AddCommand(new MPDCommand("listplaylistinfo", playlist));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load(string name)
        {
            AddCommand(new MPDCommand("load", name));
        }

        /// <summary>
        /// 
        /// </summary>
        public void LsInfo()
        {
            AddCommand(new MPDCommand("lsinfo"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void LsInfo(string uri)
        {
            AddCommand(new MPDCommand("lsinfo", uri));
        }

        /// <summary>
        /// search {TYPE} {WHAT} [...] [sort TYPE] [window START:END]
        /// Searches for any song that contains WHAT.Parameters have the same meaning as for find, except that search is not case sensitive.
        /// </summary>
        public void Search(string type, string what)
        {
            AddCommand(new MPDCommand("search", type, what));
        }

        /// <summary>
        ///  searchadd {TYPE } {WHAT} [...]
        /// Searches for any song that contains WHAT in tag TYPE and adds them to current playlist.
        ///Parameters have the same meaning as for find, except that search is not case sensitive.
        /// </summary>

        public void SearchAdd(string type, string what)
        {
            AddCommand(new MPDCommand("searchadd", type, what));
        }

        /// <summary>
        /// 
        /// </summary>
        public void MoveId(int id, int position)
        {
            AddCommand(new MPDCommand("moveid", id, position));
        }

        /// <summary>
        /// playlistinfo[[SONGPOS] | [START: END]]
        /// Displays a list of all songs in the playlist, or if the optional argument is given, 
        /// displays information only for the song SONGPOS or the range of songs START:END[6]
        /// </summary>
        public void PlaylistInfo()
        {
            AddCommand(new MPDCommand("playlistinfo"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Rename(string oldName, string newName)
        {
            AddCommand(new MPDCommand("rename", oldName, newName));
        }

        /// <summary>
        /// Removes the playlist NAME.m3u from the playlist directory. 
        /// </summary>
        
        public void Rm(string name)
        {
            AddCommand(new MPDCommand("rm", name));
        }
        
        /// <summary>
        /// Сохраняет текущий Playlist с указанным именем
        /// </summary>
        public void Save(string name)
        {
            AddCommand(new MPDCommand("save", name));
        }
        
        /// <summary>
        /// Shuffles the current playlist. START:END is optional and specifies a range of songs.  
        /// </summary>
        public void Shuffle()
        {
            AddCommand(new MPDCommand("shuffle"));
        }

        /// <summary>
        /// Prints a list of the playlist directory.
        /// After each playlist name the server sends its last modification time as attribute "Last-Modified"
        ///  in ISO 8601 format.To avoid problems due to clock differences between clients and the server, 
        /// clients should not compare this value with their local clock.
        /// </summary>
        public void ListPlaylists()
        {
            AddCommand(new MPDCommand("listplaylists"));
        }


        #endregion

        #region Playback commands

        /// <summary>
        /// Plays next song in the playlist. 
        /// </summary>
        public void Next()
        {
            AddCommand(new MPDCommand("next"));
        }
        
        /// <summary>
        /// Toggles pause/resumes playing, PAUSE is 0 or 1
        /// </summary>
        public void Pause()
        {
            AddCommand(new MPDCommand("pause"));
        }
        
        /// <summary>
        /// Begins playing the playlist at song number SONGPOS. 
        /// </summary>
        public void Play()
        {
            AddCommand(new MPDCommand("play"));
        }
        
        /// <summary>
        /// Begins playing the playlist at song SONGID. 
        /// </summary>
        public void PlayId(int id)
        {
            AddCommand(new MPDCommand("playid", id));
        }

        /// <summary>
        /// Plays previous song in the playlist. 
        /// </summary>
        public void Previous()
        {
            AddCommand(new MPDCommand("previous"));
        }
        
        /// <summary>
        /// Sets random state to STATE, STATE should be 0 or 1. 
        /// </summary>
        public void Random(bool to)
        {
            AddCommand(new MPDCommand("random", to));
        }
        
        /// <summary>
        /// Sets repeat state to STATE, STATE should be 0 or 1. 
        /// </summary>
        public void Repeat(bool to)
        {
            AddCommand(new MPDCommand("repeat", to));
        }

        /// <summary>
        /// Seeks to the position TIME (in seconds; fractions allowed) of entry SONGPOS in the playlist. 
        /// </summary>
        public void Seek(int songIndex, int position)
        {
            AddCommand(new MPDCommand("seek", songIndex, position));
        }

        ///<summary
        /// Seeks to the position TIME(in seconds; fractions allowed) within the current song.
        /// If prefixed by '+' or '-', then the time is relative to the current playing position. 
        ///  seekcur {TIME}
        ///</summary

        public void SeekCurrent(string position)
        {
            AddCommand(new MPDCommand("seekcur", position));
        }

        /// <summary>
        /// Sets volume to VOL, the range of volume is 0-100. 
        /// </summary>
        public void SetVol(int vol)
        {
            AddCommand(new MPDCommand("setvol", vol));
        }

        /// <summary>
        /// Stops playing. 
        /// </summary>
        public void Stop()
        {
            AddCommand(new MPDCommand("stop"));
        }

        #endregion

        #region Outputs
        /// <summary>
        /// 
        /// </summary>
        public void Outputs()
        {
            AddCommand(new MPDCommand("outputs"));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void EnableOutput(int index)
        {
            AddCommand(new MPDCommand("enableoutput", index));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void DisableOutput(int index)
        {
            AddCommand(new MPDCommand("disableoutput", index));
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
            AddCommand(new MPDCommand("consume", on ? "1" : "0"));
        }

        /// <summary>
        /// Sets single state to STATE, STATE should be 0 or 1. 
        /// When single is activated, playback is stopped after current song, or song is repeated
        ///  if the 'repeat' mode is enabled. 
        /// </summary>
        public void Single(bool on)
        {
            AddCommand(new MPDCommand("single", on ? "1" : "0"));
        }

        public void Config()
        {
            AddCommand(new MPDCommand("config"));
        }


        #endregion

        #region Helpers

        #endregion

        #endregion

    }   // Class End
}
