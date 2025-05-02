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
                default:
                    return "INVALID";
            }

        }
    }
}
