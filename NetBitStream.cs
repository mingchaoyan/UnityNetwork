// ------------------------------------------------------------------------------
// NetBitStream类 网络底层数据流
// 实现基本内置类型和比特流之间的转换
// ------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
namespace UnityNetwork
{
    public class NetBitStream
    {
        public const int HEADER_LENGTH = 4;
        public const int MAX_BODY_LENGTH = 512;

        public const int BYTE_LEN = 1;
        public const int INT32_LEN = 4;
        public const int SHORT16_LEN = 2;
        public const int FLOAT_LEN = 4;

        private byte[] _bytes = null;
        public byte[] BYTES {
            get {
                return _bytes;
            }
            set {
                _bytes = value;
            }
        }

        private int _body_length = 0;
        public int BODY_LENGTH {
            get {
                return _body_length;
            }
        }

        public int TOTAL_LENGTH {
            get {
                return HEADER_LENGTH + _body_length;
            }
        }

        public Socket _socket = null;

        public NetBitStream ()
        {
            _body_length = 0;
            _bytes = new byte[HEADER_LENGTH + MAX_BODY_LENGTH];
        }

        public void BeginWrite (ushort msg_id)
        {
            _body_length = 0;
            this.WriteUShort (msg_id);
        }

        public void WriteByte (byte bt)
        {
            if (_body_length + BYTE_LEN > MAX_BODY_LENGTH)
                return;
            _bytes [HEADER_LENGTH + _body_length] = bt;
            _body_length += BYTE_LEN;
        }

        public void WriteBool (bool flag)
        {
            if (_body_length + BYTE_LEN > MAX_BODY_LENGTH)
                return;
            byte b = (byte)'1';
            if (!flag)
                b = (byte)'0';
            _bytes [HEADER_LENGTH + _body_length] = b;
            _body_length += BYTE_LEN;
        }

        public void WriteInt (int number)
        {
            if (_body_length + INT32_LEN > MAX_BODY_LENGTH)
                return;
            byte[] bs = System.BitConverter.GetBytes (number);
            bs.CopyTo (_bytes, HEADER_LENGTH + _body_length);
            _body_length += INT32_LEN;
        }
        public void WriteUInt (uint number)
        {
            if (_body_length + INT32_LEN > MAX_BODY_LENGTH)
                return;
            byte[] bs = System.BitConverter.GetBytes (number);
            bs.CopyTo (_bytes, HEADER_LENGTH + _body_length);
            _body_length += INT32_LEN;
        }

        public void WriteShort (short number)
        {
            if (_body_length + SHORT16_LEN > MAX_BODY_LENGTH)
                return;
            byte[] bs = System.BitConverter.GetBytes (number);
            bs.CopyTo (_bytes, HEADER_LENGTH + _body_length);
            _body_length += SHORT16_LEN;
        }
        public void WriteUShort (ushort number)
        {
            if (_body_length + SHORT16_LEN > MAX_BODY_LENGTH)
                return;
            byte[] bs = System.BitConverter.GetBytes (number);
            bs.CopyTo (_bytes, HEADER_LENGTH + _body_length);
            _body_length += SHORT16_LEN;
        }

        public void WriteFloat (float number)
        {
            if (_body_length + FLOAT_LEN > MAX_BODY_LENGTH)
                return;
            byte[] bs = System.BitConverter.GetBytes (number);
            bs.CopyTo (_bytes, HEADER_LENGTH + _body_length);
            _body_length += FLOAT_LEN;
        }

        public void WriteString (string str)
        {
            ushort length = (ushort)System.Text.Encoding.UTF8.GetByteCount (str);
            this.WriteUShort (length);
            if (length + _body_length > MAX_BODY_LENGTH)
                return;
            System.Text.Encoding.UTF8.GetBytes (str, 0, str.Length, _bytes, HEADER_LENGTH + length);
            _body_length += length;
        }

        public void BeginRead (NetPacket packet, out ushort msg_id)
        {
            packet._bytes.CopyTo (this.BYTES, 0);
            this._socket = packet._peer;
            _body_length = 0;
            this.ReadUShort (out msg_id);
        }
        public void BeginRead2 (NetPacket packet)
        {
            packet._bytes.CopyTo (this.BYTES, 0);
            this._socket = packet._peer;
            _body_length = 0;
            _body_length += SHORT16_LEN;
        }

        public void ReadByte (out byte bt)
        {
            bt = 0;
            if (_body_length + BYTE_LEN > MAX_BODY_LENGTH) 
                return;
            bt = _bytes [HEADER_LENGTH + _body_length];
            _body_length += BYTE_LEN;
        }

        public void ReadBool (out bool flag)
        {
            flag = false;
            if (_body_length + BYTE_LEN > MAX_BODY_LENGTH)
                return;
            byte bt = _bytes [HEADER_LENGTH + _body_length];
            if (bt == (byte)'1')
                flag = true;
            else
                flag = false;
            _body_length += BYTE_LEN;
        }

        public void ReadInt (out int number)
        {
            number = 0;
            if (_body_length + INT32_LEN > MAX_BODY_LENGTH)
                return;
            number = System.BitConverter.ToInt32 (_bytes, HEADER_LENGTH + _body_length);
            _body_length += INT32_LEN;
        }
        public void ReadUInt (out uint number)
        {
            number = 0;
            if (_body_length + INT32_LEN > MAX_BODY_LENGTH)
                return;
            number = System.BitConverter.ToUInt32 (_bytes, HEADER_LENGTH + _body_length);
            _body_length += INT32_LEN;
        }

        public void ReadShort (out short number)
        {
            number = 0;
            if (_body_length + SHORT16_LEN > MAX_BODY_LENGTH)
                return;
            number = System.BitConverter.ToInt16 (_bytes, HEADER_LENGTH + _body_length);
            _body_length += SHORT16_LEN;
        }
        public void ReadUShort (out ushort number)
        {
            number = 0;
            if (_body_length + SHORT16_LEN > MAX_BODY_LENGTH)
                return;
            number = System.BitConverter.ToUInt16 (_bytes, HEADER_LENGTH + _body_length);
            _body_length += SHORT16_LEN;
        }

        public void ReadFloat (out float number)
        {
            number = 0;
            if (_body_length + FLOAT_LEN > MAX_BODY_LENGTH)
                return;
            number = System.BitConverter.ToSingle (_bytes, HEADER_LENGTH + _body_length);
            _body_length += FLOAT_LEN;
        }

        public void ReadString (out string str)
        {
            str = "";
            ushort length = 0;
            ReadUShort (out length);
            if (_body_length + length > MAX_BODY_LENGTH)
                return;
            str = Encoding.UTF8.GetString (_bytes, HEADER_LENGTH + _body_length, length);
            _body_length += length;
        }

        public void EncodeHeader ()
        {
            byte[] bs = System.BitConverter.GetBytes (_body_length);
            bs.CopyTo (_bytes, 0);
        }

        public void DecodeHeader ()
        {
            _body_length = System.BitConverter.ToInt32 (_bytes, 0);
        }
    }
}

