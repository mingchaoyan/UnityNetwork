// ------------------------------------------------------------------------------
// MsgId类 消息标志符
// ------------------------------------------------------------------------------
using System;
namespace UnityNetwork
{
    static public class MsgId
    {
        public enum Id
        {
            NULL = 0,
            CONNECTION_REQUEST_ACCEPTED,
            CONNECTION_ATTEMPT_FAILED,
            CONNECTION_LOST,
            NEW_INCOMING_CONNECTION,
            ID_CHAT,
        };
    }
}

