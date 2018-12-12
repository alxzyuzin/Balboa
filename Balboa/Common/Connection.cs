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
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
        ResourceLoader _resldr = new ResourceLoader();

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

        private string              _status;
        
        private DataReader _datareader;
        private DataWriter _datawriter;

        private string _host;
        private string _port;


        #endregion

        #region Properties
        private ConnectionStatus _statusId;
        public ConnectionStatus StatusId
        {
            get { return _statusId; }

            private set
            {
                if(_statusId != value)
                { 
                   string s = _host + " port : " + _port;

                    _statusId = value;
                    switch (_statusId)
                    {
                        case ConnectionStatus.Connected:       Status = "Connected to " + s; break;
                        case ConnectionStatus.Connecting:      Status = "Connecting to " + s; break;
                        case ConnectionStatus.Disconnected:    Status = "Disconnected from " + s; break;
                        case ConnectionStatus.Disconnecting:   Status = "Disconnecting from " + s; break;
                        case ConnectionStatus.ConnectionError: Status = Error; break;
                    }
                    NotifyPropertyChanged("StatusId");
                }
            }
            
        }

        public string Status
        {
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged("Status");
                }
            }
            get { return _status; }

        }

        private string _error;
        public string Error
        {
            private set
            {
                if (_error != value)
                {
                    _error = value;
                    NotifyPropertyChanged("Error");
                }
            }
            get { return _error; }
        }

        public string InitialResponse { get; set; }
        #endregion
     
        /// <summary>
        /// Устанавливаем соединение с сервером
        /// Если соединение установлено успешноб читаем ответ сервера при установлении соедиения
        /// Если ответ сервера говорит об успешном соединении, меняем статус соединения на Connected и завершаем работу
        /// </summary>
        public async Task<bool> Open(string host, string port, string password)
        {
            _host = host;
            _port = port;

            StatusId = ConnectionStatus.Connecting;
            try
            {
                HostName hostName = new HostName(host);
                _socket = new StreamSocket();
                _socket.Control.KeepAlive = true;
                await _socket.ConnectAsync(hostName, port);
                _datareader = new DataReader(_socket.InputStream);
                _datareader.InputStreamOptions = InputStreamOptions.Partial;
                _datawriter = new DataWriter(_socket.OutputStream);

                if (_datareader == null || _datawriter == null)
                {
                    Clear();
                    Error = _resldr.GetString("StreamsOpeningError");
                    return false;
                }
                // Если соединение установлено то прочитаем ответ от сервера 
                // и изменим статус соединения в зависимости от ответа
                InitialResponse = await ReadResponse();

                if (InitialResponse.Length == 0)
                {
                    Clear();
                    Error = _resldr.GetString("InitialServerResponseError");
                    return false;
                }

                if (InitialResponse.StartsWith("OK ") && InitialResponse.Contains("MPD"))
                {
                   if (password!=null && password.Length>0)
                   {
                      await SendCommand("password " + password);
                      string res = await ReadResponse();
                      if (res != "OK\n")
                      {
                         await SendCommand("close");
                         Clear();
                         Error = _resldr.GetString("ConnectionError")+"\n" + res;
                         return false;
                      }
                    }
                    StatusId = ConnectionStatus.Connected;
                    return true;
                 }
                 else
                 {
                    Clear();
                    Error = _resldr.GetString("InvalidInitialResponse") + InitialResponse;
                    return false;
                  }
            }
             catch (Exception e)
             {
                Clear();
                Error = string.Format(CultureInfo.CurrentCulture, _resldr.GetString("ConnectionErrorDescription"),
                                        _host, _port,  Utilities.GetExceptionMsg(e));
                return false;
             }
        }

        /// <summary>
        /// Выполняет закрытие потоков чтения, записи, соединения с сервером и очистку
        /// </summary>
        private void Clear()
        {
            StatusId = ConnectionStatus.ConnectionError;

            if (_datareader != null) _datareader.Dispose();
            if (_datawriter != null) _datawriter.Dispose();
            if (_socket != null) _socket.Dispose();
        }

        public void Close()
        {
            if (_statusId != ConnectionStatus.Disconnected)
            {
                Clear();
                StatusId = ConnectionStatus.Disconnected;
            }
        }

        public async Task<bool> SendCommand(string command)
        {
            _datawriter.WriteString(command + "\n");
            await _datawriter.StoreAsync();
            return true;
        }
        
        public async Task<string> ReadResponse()
        {
            await _datareader.LoadAsync(1000000);
            return _datareader.ReadString(_datareader.UnconsumedBufferLength);
        }
    }
}
