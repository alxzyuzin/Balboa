/*-----------------------------------------------------------------------
 * Copyright 2017 Alexandr Zyuzin.
 *
 * This file is part of MPD client Balboa.
 *
 * Устанавливает соедиение с сервером и обеспечивает обмен данными.
 *
 --------------------------------------------------------------------------*/

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


namespace Balboa.Common
{
    public enum ConnectionStatus
    {
        Disconnected,            // Not connected and idle
        Connecting,              // (Re-)establishing connection
        Connected,               // Connection OK
        Disconnecting,            // Disconnecting and not reconnecting when done
        ConnectionError
    }

    public sealed class Connection : INotifyPropertyChanged
    {
        /// <summary>
        /// Уведомление об изменении значения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Fields

        private StreamSocket        _socket;
        private ConnectionStatus    _statusID;
        private string              _status;
        
        private DataReader _datareader;
        private DataWriter _datawriter;

        private string _host;
        private string _port;
        private string _error;

        #endregion

        #region Properties

        public ConnectionStatus StatusID
        {
            get { return _statusID; }

            private set
            {
                if(_statusID != value)
                { 
                    string s = _host + " port : " + _port;

                    _statusID = value;
                    switch (_statusID)
                    {
                        case ConnectionStatus.Connected:       Status = "Connected to " + s; break;
                        case ConnectionStatus.Connecting:      Status = "Connecting to " + s; break;
                        case ConnectionStatus.Disconnected:    Status = "Disconnected from " + s; break;
                        case ConnectionStatus.Disconnecting:   Status = "Disconnecting from " + s; break;
                        case ConnectionStatus.ConnectionError: Status = Error; break;
                    }
                    NotifyPropertyChanged("StatusID");
                }
            }
            
        }

        public string Status
        {
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
            get { return _status; }

        }

        public string Error
        {
            private set
            {
                _error = value;
                NotifyPropertyChanged("Error");
            }
            get { return _error; }

        }

        public string InitialResponce { get; set; }
        #endregion
     
        /// <summary>
        /// Устанавливаем соединение с сервером
        /// Если соединение установлено успешноб читаем ответ сервера при установлении соедиения
        /// Если ответ сервера говорит об успешном соединении, меняем статус соединения на Connected и завершаем работу
        /// </summary>
        public async Task<bool> Open(string host, string port = "6600", string password="")
        {
            _host = host;
            _port = port;

            try
            {
                HostName hostName = new HostName(host);

                StatusID =ConnectionStatus.Connecting;

                _socket = new StreamSocket();
                _socket.Control.KeepAlive = true;

                await _socket.ConnectAsync(hostName, port);
                
                _datareader = new DataReader(_socket.InputStream);
                _datareader.InputStreamOptions = InputStreamOptions.Partial;
                _datawriter = new DataWriter(_socket.OutputStream);

                if (_datareader == null || _datawriter == null)
                {
                    Clear();
                    Error = "Undefined error found while Input or Output streams opening.\n";
                    return false;
                }
                // Если соединение установлено то прочитаем ответ от сервера 
                // и изменим статус соединения в зависимости от ответа
                // InitialResponce = await ReadBanner();
                InitialResponce = await ReadResponce();

                if (InitialResponce.Length == 0)
                {
                    Clear();
                    Error = "Error reading server connection responce, check settings.";
                    return false;
                }

                if (InitialResponce.StartsWith("OK ") && InitialResponce.Contains("MPD"))
                {
                   if (password!=null && password.Length>0)
                   {
                      await SendCommand("password " + password);
                      string res = await ReadResponce();
                      if (res != "OK\n")
                      {
                         await SendCommand("close");
                         Clear();
                         Error = "Connection error.\n" + res;
                         return false;
                      }
                    }
                    StatusID = ConnectionStatus.Connected;
                    return true;
                 }
                 else
                 {
                    Clear();
                    Error = "Server connection responce is not valid.\n" + InitialResponce;
                    return false;
                  }
            }
             catch (Exception e)
             {
                Clear();

                Error = "Error while connecting to server '" + _host + "' port : " + _port + ".\n ";
                if (e.Message.Contains("\r\n"))
                { 
                    string exeptionmessage = e.Message.Substring(0, e.Message.IndexOf("\r\n"));
                    Error += exeptionmessage;
                }
                else
                {
                    Error += e.Message;
                }
                return false;
             }
        }

        /// <summary>
        /// Выполняет закрытие потоков чтения, записи, соединения с сервером и очистку
        /// </summary>
        private void Clear()
        {
            StatusID = ConnectionStatus.ConnectionError;

            if (_datareader != null) _datareader.Dispose();
            if (_datawriter != null) _datawriter.Dispose();
            if (_socket != null) _socket.Dispose();
        }

        public void Close()
        {
            if (_statusID != ConnectionStatus.Disconnected)
            {
                Clear();
                StatusID = ConnectionStatus.Disconnected;
            }

        }

        /// <summary>
        /// Читает первый ответ сервера при установлении соединения
        /// </summary>
        /// 
        /*
        private async Task<string> ReadBanner()
        {
                await   _datareader.LoadAsync(128);
                return  _datareader.ReadString(_datareader.UnconsumedBufferLength);
         }
         */
        public async Task<bool> SendCommand(string command)
        {
            try {
                _datawriter.WriteString(command + "\n");
                await _datawriter.StoreAsync();
                return true;
            }
            catch(Exception)
            {
                throw;
            }

        }
        
        /// <summary>
        /// Read server responce and analyse result (OK or ACK).
        /// Блок данных прочитанный с сервера разбивается на строки.
        /// Функция возвращает массив строк без конечной стр
        public async Task<string> ReadResponce()
        {
            string buffer;
            try
            {
                await _datareader.LoadAsync(1000000);

                buffer = _datareader.ReadString(_datareader.UnconsumedBufferLength);
                return buffer;
            }
            catch(Exception)
            {
                throw;
            }
        }
 
    }
}
