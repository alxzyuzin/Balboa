using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balboa.Common
{
    public class ServerEventArgs : EventArgs
    {
        public ServerEventArgs()
        {

        }
        public ServerEventArgs(string m)
        {
            Message = m;
        }
        public string Message { get; set; }
    }

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
}
