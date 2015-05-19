// ------------------------------------------------------------------------------
// NetworkManager类 处理逻辑
// 实现收到数据包入队列，从队列中取出数据包，并根据数据包的消息标志判断如何处理数据包
// ------------------------------------------------------------------------------
using System.Collections;
using System.Text;
namespace UnityNetwork
{
    public class NetworkManager
    {
        protected static NetworkManager _instance = null;
        public static NetworkManager Instance {
            get { 
                return _instance;
            }
        }
        private static Queue packets = new Queue ();
        public int packet_size {
            get {
                return packets.Count;
            }
        }

        public NetworkManager ()
        {
            _instance = this;
        }

        public void AddPacket (NetPacket packet)
        {
            packets.Enqueue (packet);
        }

        public NetPacket GetPacket ()
        {
            if (packets.Count == 0)
                return null;
            return (NetPacket)packets.Dequeue ();
        }

        public virtual void Update ()
        {
            //子类重写Update处理逻辑
        }
    }
}

