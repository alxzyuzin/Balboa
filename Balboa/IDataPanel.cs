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
        public ActionParams( Message message)
        {
            ActionType = ActionType.DisplayMessage;
            Message = message;
        }

        public ActionParams(ActionType action, Panels panelClass)
        {
            ActionType = action;
            PanelClass = panelClass;
        }

        public ActionType ActionType { get; private set; }
        public Message Message { get; private set; }
        public Panels PanelClass { get; private set; }
    }

    public struct Message
    {
        public MsgBoxType Type;
        public string Text;
        public MsgBoxButton Buttons;
        public int BoxHeight;
    }

} // class ActionParams
