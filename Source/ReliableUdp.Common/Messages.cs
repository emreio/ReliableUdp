using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Common
{
    [Serializable]
    public class Message
    {
        public MessageHeader Header { get; set; }
        public MessageBody Body { get; set; }
    }

    [Serializable]
    public class MessageHeader
    {
        public IPEndPoint IP { get; set; }
        public string ClientID { get; set; }
        public ulong PacketID { get; set; }
    }

    [Serializable]
    public class MessageBody
    {
        public string Message { get; set; }
        public byte[] Frame { get; set; }
        public ulong ID { get; set; }
    }
}
