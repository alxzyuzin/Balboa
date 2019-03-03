using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Balboa;
using Balboa.Common;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Balboa
{
    [Flags]
    public enum ActionType
    {
        DisplayMessage = 1,
        ActivateDataPanel = 2,
        DisplayPanel = 4,
        HidePanel =8
    }

    public delegate void ActionRequestedEventHandler(object sender, ActionParams actionParams);

    public interface IDataPanel
    {
        void Init(Server server);
        void Update();
        Orientation Orientation { get; set; }
    }

    public interface IRequestAction
    {
        /// <summary>
        ///    Occurs when a command button pressed.
        /// </summary>
        event ActionRequestedEventHandler RequestAction;
        void HandleUserResponse(MsgBoxButton pressedButton);
    }

    public interface IDataPanelInfo
    {
        /// <summary>
        ///    Occurs when a command button pressed.
        /// </summary>

        string DataPanelInfo { get; set; }
        string DataPanelElementsCount { get; set; }
        double TotalButtonWidth { get; }
    }


    public class ActionParams
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
        public Message(MsgBoxType type, string text, MsgBoxButton buttons, int height)
        {
            Type = type;
            Text = text;
            Buttons = buttons;
            BoxHeight = height;
    }
        
        public MsgBoxType Type { get; private set; }
        public string Text { get; private set; }
        public MsgBoxButton Buttons { get; private set; }
        public int BoxHeight { get; private set; }
    } // class Message

} // namespace Balboa
