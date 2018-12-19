using System;

namespace Balboa.Common
{
    public enum EventType
    {
        NoEvent,
        Error,
        CriticalError,
        ConnectionStatusChanged,
        ServerStatusChanged,
        CommandCompleted
    }

    internal class ServerEventArgs : EventArgs
    {
        public EventType EventType { get; set; } = EventType.NoEvent;
        public string Command { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool ConnectionStatus { get; set; }

        public ServerEventArgs() {}
        public ServerEventArgs(EventType eventType, string command,string result, string message)
        {
            EventType = eventType;
            Message = message;
            Command = command;
            Result = result;
        }

        public ServerEventArgs(EventType eventType, bool status, string message)
        {
            EventType = eventType;
            Message = message;
            ConnectionStatus = status;
        }

        public ServerEventArgs(EventType eventType, string message)
        {
            EventType = eventType;
            Message = message;
        }

    }
    /*
        public class ServerCommandCompletionEventArgs : ServerEventArgs
        {
            public string Command { get; set; }
            public string Result { get; set; }
         }

        public class ServerConnectionStatusEventArgs : ServerEventArgs
        {
            public ServerConnectionStatusEventArgs(ConnectionStatus status)
            {
                Status = status;
            }
            public ConnectionStatus Status { get; set; }
        }
        */
}
