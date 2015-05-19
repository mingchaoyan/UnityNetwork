// ------------------------------------------------------------------------------
// NetPacket类 网络层和逻辑层之间的数据流
// 实现byte数组的复制
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace UnityNetwork
{
    public class NetPacket
    {
        public byte[] _bytes;
        public Socket _peer = null;
        protected int _length = 0;
        public string _error = "";

        public NetPacket ()
        {
            _bytes = new byte[NetBitStream.HEADER_LENGTH + NetBitStream.MAX_BODY_LENGTH];
        }

        public void CopyBytes (NetBitStream stream)
        {
            stream.BYTES.CopyTo (_bytes, 0);
            _length = stream.TOTAL_LENGTH;
        }

        public void SetMsgId (ushort msg_id)
        {
            byte[] bs = System.BitConverter.GetBytes (msg_id);
            bs.CopyTo (_bytes, NetBitStream.HEADER_LENGTH);
            _length = NetBitStream.HEADER_LENGTH + NetBitStream.SHORT16_LEN;
        }

        public void GetMsgId (out ushort msg_id)
        {
            msg_id = System.BitConverter.ToUInt16 (_bytes, NetBitStream.HEADER_LENGTH);
        }
    }
}

