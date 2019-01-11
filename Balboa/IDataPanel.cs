using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Balboa;
using Balboa.Common;

namespace Balboa
{
    public interface IDataPanel
    {
        void Init(Server server);
        void Update();
    }

    public class Message
    {
        public Message(MsgBoxType type, string messageText, MsgBoxButton buttons, int boxHeight)
        {
            Type = type;
            Text = messageText;
            Buttons = buttons;
            BoxHeight = boxHeight;
        }
        public MsgBoxType Type { get; private set; }
        public string Text { get; private set; }
        public MsgBoxButton Buttons { get; private set; } = MsgBoxButton.Close;
        public int BoxHeight { get; private set; } = 200;
    }

}
