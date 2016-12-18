using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliableUdp.Utilities
{
    public class Logger
    {
        private static object lockObj = new object();

        public static void LogExeption(Exception ex)
        {
            if (ex != null)
            {
                lock (lockObj)
                {
                    File.AppendAllText("log.txt", ex.Message + Environment.NewLine);

                    if (ex.InnerException != null)
                    {
                        File.AppendAllText("log.txt", ex.InnerException.Message + Environment.NewLine);
                    }

                    File.AppendAllText("log.txt", ex.StackTrace);
                }
            }
        }
    }
}
