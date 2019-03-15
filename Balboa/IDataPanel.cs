using System;
using Windows.UI.Xaml.Controls;

namespace Balboa
{
    [Flags]
    public enum ActionType
    {
        DisplayMessage = 1,
        ActivateDataPanel = 2,
        DisplayPanel = 4,
        HidePanel = 8
    }

    public delegate void ActionRequestedEventHandler(object sender, ActionParams e);

    public interface IDataPanel
    {
        void Init(Server server);
        void Update();
        Orientation Orientation { get; set; }
    }

    public interface IRequestAction
    {
        event ActionRequestedEventHandler RequestAction;
        void HandleUserResponse(MsgBoxButton pressedButton);
    }

    public interface IDataPanelInfo
    {
        string DataPanelInfo { get; set; }
        string DataPanelElementsCount { get; set; }
        double TotalButtonWidth { get; }
    }


    public class ActionParams : EventArgs
    {
        public ActionParams(Message message)
        {
            ActionType = ActionType.DisplayMessage;
            Message = message;
        }

        public ActionParams(ActionType action)
        {
            ActionType = action;
        }

        public ActionParams SetPanel<T>(T panel) where T : IDataPanel, IRequestAction
        {
            Panel = panel;
            ((IDataPanel)Panel).Orientation = Orientation.Horizontal;
            return this;
        }

        public  ActionType ActionType { get; private set; }
        public Message Message { get; private set; }
        public object Panel { get; private set; }
    } // class ActionParams

    public class Message
    {
        public Message(MsgBoxType type, string text, MsgBoxButton buttons = MsgBoxButton.Close, double height = 200, double width = 500)
        {
            Type = type;
            Text = text;
            Buttons = buttons;
            BoxHeight = height;
            BoxWidth = width;
        }
        
        public MsgBoxType Type { get; private set; }
        public string Text { get; private set; }
        public MsgBoxButton Buttons { get; private set; }
        public double BoxHeight { get; private set; }
        public double BoxWidth { get; private set; }
    } // class Message

} // namespace Balboa
