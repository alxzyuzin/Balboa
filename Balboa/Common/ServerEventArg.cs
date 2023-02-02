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

    public enum CommandButton
    {
        AddTracks
    }

    internal class ServerEventArgs : EventArgs
    {
        public EventType EventType { get; set; } = EventType.NoEvent;
        public string Command { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool ConnectionStatus { get; set; }
        public string Content { get; set; } = string.Empty;

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


    public class DisplayMessageEventArgs : EventArgs
    {
        public DisplayMessageEventArgs(MsgBoxType type, string message, MsgBoxButton buttons, int boxHeight)
        {
            Type = type;
            Message = message;
            Buttons = buttons;
            BoxHeight = boxHeight;
        }
        public MsgBoxType Type { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public MsgBoxButton Buttons { get; private set; } = MsgBoxButton.Close;
        public int BoxHeight { get; private set; } = 200;
    }

    public class CommandButtonPressedEventArgs : EventArgs
    {
        public CommandButton PressedButton { get; private set; }

        public CommandButtonPressedEventArgs(CommandButton button)
        {
            PressedButton = button;
        }
    }
}

