using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Balboa;
using Balboa.Common;

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
    }

    public interface IRequestAction
    {
        /// <summary>
        ///    Occurs when a command button pressed.
        /// </summary>
        event ActionRequestedEventHandler RequestAction;
        void HandleUserResponse(MsgBoxButton pressedButton);
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

        public ActionParams SetPanel<T>(T panel) where T : IRequestAction
        {
            Panel = panel;
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
