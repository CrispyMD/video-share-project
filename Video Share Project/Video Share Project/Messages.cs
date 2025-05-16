using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video_Share_Project
{

    public enum Messages
    {
        DoesServerExist,
        ServerExists,
        AcceptClient,
        ConnectionEstablished,
        StartingVideoBroadcast,
    }

    public static class MessagesMethods
    {
        public static string name(this Messages message)
        {
            switch(message)
            {
                case Messages.DoesServerExist:
                    return "DoesServerExist";
                case Messages.ServerExists:
                    return "ServerExists";
                case Messages.AcceptClient:
                    return "AcceptClient";
                case Messages.ConnectionEstablished:
                    return "ConnectionEstablished";
                case Messages.StartingVideoBroadcast:
                    return "StartingVideoBroadcast";
                default:
                    return "INVALID";
            }

        }
    }
}
