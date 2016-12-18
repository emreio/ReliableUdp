using ReliableUdp.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Transmission.Utilities
{
    public static class Helper
    {
        public static Message DeserializeMessage(byte[] message)
        {
            using (MemoryStream ms = new MemoryStream(message))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                ms.Seek(0, SeekOrigin.Begin);

                return (Message)formatter.Deserialize(ms);
            }
        }

        public static byte[] SerializeMessage(Message message)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, message);
                return ms.ToArray();
            }
        }
    }
}
