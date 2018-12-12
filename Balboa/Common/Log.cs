using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Balboa.Common
{
    public delegate void EventHandler<TLogEventArgs>(object sender, TLogEventArgs eventArgs);
    public class Log
    {
        public event EventHandler LogError;

        private void NotifyLogError(string message)
        {
            if (LogError != null)
            {
                LogError(this, new LogEventArgs(message));
            }
        }

        public enum LogLevel { NoLog, LogErrorsOnly, LogAllEvents }

        string _FileName = "Balboa.log";
        StorageFolder _Folder = ApplicationData.Current.LocalFolder;
        CreationCollisionOption _Options = CreationCollisionOption.ReplaceExisting;
        StorageFile _File = null;

        public bool Initialized { get; private set; } = false;

        public async Task<bool> Init()
        {
            try
            {
                await _Folder.GetFileAsync(_FileName);
                Initialized = true;
                return true;
            }
            catch (FileNotFoundException)
            {
                try
                {
                    _File = await _Folder.CreateFileAsync(_FileName, _Options);
                    Initialized = true;
                    return true;
                }
                catch(Exception ex)
                {
                    Initialized = false;
                    NotifyLogError("Error creating log file.\n  Exception: " + ex.GetType().Name + "\n" + ex.Message);
                    return false;
                }
            }
        }

        public async Task Clear()
        {
            try
            {
                _File = await _Folder.CreateFileAsync(_FileName, _Options);
                Initialized = true;
            }
            catch (Exception ex)
            {
                Initialized = false;
                NotifyLogError("Error clearing log file.\n Exception: " + ex.GetType().Name + "\n" + ex.Message);
            }
        }
  
        public async Task WriteLine(string line)
        {
            try
            {
                if(Initialized)
                   await FileIO.WriteTextAsync(_File, line);
            }
            catch (Exception ex)
            {
                NotifyLogError("Error writing to log file.\n Exception: "+ ex.GetType().Name+"\n"+ex.Message);
            }
        }

        public async Task<string> Read()
        {
            try
            {
                _File = await _Folder.GetFileAsync(_FileName);
                return await FileIO.ReadTextAsync(_File);
            }
            catch(Exception ex)
            {
                NotifyLogError("Error reading log file.\n Exception: " + ex.GetType().Name + "\n" + ex.Message);
                return string.Empty;
            }
        }
    }

    public class LogEventArgs: EventArgs
    {
        public LogEventArgs() { }
        public LogEventArgs(string message)
        {
            _Message = message;
        }
        private string _Message = string.Empty;
        public string Message { get { return _Message; } }
    }
}
