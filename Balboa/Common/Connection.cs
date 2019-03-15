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
  
    public sealed class Connection : INotifyPropertyChanged, IDisposable
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

        private StreamSocket _socket;
        private DataReader _datareader;
        private DataWriter _datawriter;
        private string _host;
        private string _port;

        #endregion

        #region Properties

        private bool _isActive = false;
        public  bool IsActive
        {
            get {return _isActive; }
            private set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyPropertyChanged(nameof(IsActive));
                }
            }
        }

        private string _status;
        public string Status
        {
           private set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
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
            IsActive = false;
            _error = string.Empty;
            Status = $"Connecting to {_host} port: {_port}.";
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
                    if (password != null && password.Length > 0)
                    {
                        await SendCommand("password " + password);
                        string res = await ReadResponse();
                        if (res != "OK\n")
                        {
                            await SendCommand("close");
                            Clear();
                            Error = _resldr.GetString("ConnectionError") + "\n" + res;
                            IsActive = false;
                            Status = "Disconnected";
                            return false;
                        }
                    }
                    IsActive = true;
                    Status = $"Connected to  {_host} port: {_port}";
                    return true;
                }
                else
                {
                    Clear();
                    Error = _resldr.GetString("InvalidInitialResponse") + InitialResponse;
                    IsActive = false;
                    Status = "Disconnected";
                    return false;
                }
            }
            catch (Exception e)
            {
                Clear();

                string ExceptionMsg = e.Message.Contains("\r\n") ? e.Message.Substring(0, e.Message.IndexOf("\r\n")) : e.Message;
                Error = string.Format(CultureInfo.CurrentCulture, _resldr.GetString("ConnectionErrorDescription"),
                                        _host, _port, ExceptionMsg);
                IsActive = false;
                Status = "Disconnected";
                return false;
            }
        }

        /// <summary>
        /// Выполняет закрытие потоков чтения, записи, соединения с сервером и очистку
        /// </summary>
        private void Clear()
        {
            if (_datareader != null) _datareader.Dispose();
            if (_datawriter != null) _datawriter.Dispose();
            if (_socket != null) _socket.Dispose();
        }

        public void Close()
        {
            if (IsActive)
            {
                Clear();
                Status = $"Disconnected.";
                IsActive = false;
            }
        }

        public async Task SendCommand(string command)
        {
                _datawriter.WriteString(command + "\n");
                await _datawriter.StoreAsync();
        }
        
        
        public async Task<string> ReadResponse()
        {
                await _datareader.LoadAsync(1000000);
                return _datareader.ReadString(_datareader.UnconsumedBufferLength);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
