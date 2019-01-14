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
        ActivateDataPanel = 2
    }

    public delegate void ActionRequestedEventHandler(object sender, ActionParams actionParams);

    public interface IDataPanel
    {
        void Init(Server server);
        void Update();
        void HandleUserResponse(MsgBoxButton pressedButton);
    }

    public interface IActionRequested
    {
        /// <summary>
        ///    Occurs when a command button pressed.
        /// </summary>
        event ActionRequestedEventHandler ActionRequested;
        
    }

    public class ActionParams
    {
        public ActionParams( Message message)
        {
            ActionType = ActionType.DisplayMessage;
            Message = message;
        }

        public ActionParams(string panelClassName)
        {
            ActionType = ActionType.ActivateDataPanel;
            PanelClassName = panelClassName;
        }

        public ActionType ActionType { get; private set; }
        public Message Message { get; private set; }
        public string PanelClassName { get; private set; } = string.Empty;
    }

    public struct Message
    {
        public MsgBoxType Type;
        public string Text;
        public MsgBoxButton Buttons;
        public int BoxHeight;
    }

}
