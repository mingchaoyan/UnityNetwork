// ------------------------------------------------------------------------------
// ------------------------------------------------------------------------------
using System;
using System.Net;
using System.Net.Sockets;
namespace UnityNetwork
{
    public class NetTCPClient
    {
        public int SEND_TIMEOUT = 3;
        public int REV_TIMEOUT = 3;

        private NetworkManager _net_mgr = null;
        private Socket _socket = null;

        public NetTCPClient ()
        {
            _net_mgr = NetworkManager.Instance;
        }

        public bool Connect (string address, int remote_port)
        {
            if (_socket != null && _socket.Connected)
                return true;
            IPHostEntry host_entry = Dns.GetHostEntry (address);
            foreach (IPAddress ip in host_entry.AddressList) {
                try {
                    IPEndPoint ipe = new IPEndPoint (ip, remote_port);
                    _socket = new Socket (ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    _socket.BeginConnect (ipe, new System.AsyncCallback (ConnectionCallback), _socket);
                    break;
                } catch (Exception e) {
                    PushPacket ((ushort)MsgId.Id.CONNECTION_ATTEMPT_FAILED, e.Message);
                    return false;
                }
            }
            return true;
        }

        void PushPacket (ushort msg_id, string exception)
        {
            NetPacket packet = new NetPacket ();
            packet.SetMsgId (msg_id);
            packet._error = exception;
            packet._peer = _socket;
            _net_mgr.AddPacket (packet);
        }

        void PushPacket2 (NetBitStream stream)
        {
            NetPacket packet = new NetPacket ();
            stream.BYTES.CopyTo (packet._bytes, 0);
            packet._peer = stream._socket;
            _net_mgr.AddPacket (packet);
        }

        void ConnectionCallback (System.IAsyncResult ar)
        {
            NetBitStream stream = new NetBitStream ();
            stream._socket = (Socket)ar.AsyncState;
            try {
                _socket.EndConnect (ar);
                _socket.SendTimeout = SEND_TIMEOUT;
                _socket.ReceiveTimeout = REV_TIMEOUT;

                PushPacket ((ushort)MsgId.Id.CONNECTION_REQUEST_ACCEPTED, "");
                _socket.BeginReceive (stream.BYTES, 0, NetBitStream.HEADER_LENGTH, SocketFlags.None, 
                                     new System.AsyncCallback (ReceiveHeader), stream);
            } catch (Exception ex) {
                if (ex.GetType () == typeof(SocketException)) {
                    if (((SocketException)ex).SocketErrorCode == SocketError.ConnectionRefused) {
                        PushPacket ((ushort)MsgId.Id.CONNECTION_ATTEMPT_FAILED, ex.Message);
                    } else
                        PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, ex.Message);
                }
                Disconnect (0);
            }
        }

        public void Disconnect (int timeout)
        {
            if (_socket.Connected) {
                _socket.Shutdown (SocketShutdown.Receive);
                _socket.Close (timeout);
            } else
                _socket.Close ();
        }

        void ReceiveHeader (System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try {
                int read = _socket.EndReceive (ar);
                if (read < 1) {
                    Disconnect (0);
                    PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, "");
                    return;
                }
                stream.DecodeHeader ();
                _socket.BeginReceive (stream.BYTES, NetBitStream.HEADER_LENGTH, stream.BODY_LENGTH, SocketFlags.None,
                                     new System.AsyncCallback (ReceiveBody), stream);
                
            } catch (Exception ex) {
                PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, ex.Message);
                Disconnect (0);
            }
        }

        void ReceiveBody (System.IAsyncResult ar)
        {
            NetBitStream stream = (NetBitStream)ar.AsyncState;
            try {
                int read = _socket.EndReceive (ar);
                if (read < 1) {
                    Disconnect (0);
                    PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, "");
                    return;
                }
                PushPacket2 (stream);
                _socket.BeginReceive (stream.BYTES, 0, NetBitStream.HEADER_LENGTH, SocketFlags.None,
                                     new System.AsyncCallback (ReceiveHeader), stream);
            } catch (Exception ex) {
                PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, ex.Message);
                Disconnect (0);
            }
        }

        public void Send (NetBitStream bts)
        {
            if (!_socket.Connected)
                return;
            NetworkStream ns;
            lock (_socket) {
                ns = new NetworkStream (_socket);
            }

            if (ns.CanWrite) {
                try {
                    ns.BeginWrite (bts.BYTES, 0, bts.TOTAL_LENGTH, new System.AsyncCallback (SendCallback), ns);
                } catch (Exception) {
                    PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, "");
                    Disconnect (0);
                }
            }
        }

        private void SendCallback (System.IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try {
                ns.EndWrite (ar);
                ns.Flush ();
                ns.Close ();
            } catch (Exception) {
                PushPacket ((ushort)MsgId.Id.CONNECTION_LOST, "");
                Disconnect (0);
            }
        }
    }
}


