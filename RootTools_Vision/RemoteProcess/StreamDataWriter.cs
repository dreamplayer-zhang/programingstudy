using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    class StreamDataWriter
    {
        FileStream stream;
        ASCIIEncoding encoder;

        public StreamDataWriter(FileStream fs)
        {
            this.stream = fs;
            this.encoder = new ASCIIEncoding();
        }

        public void Write<T>(T obj)
        {
            byte[] bufferData = Tools.ObjectToByteArray<T>(obj);
            byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);

            this.stream.Write(bufferSize, 0, sizeof(int));
            this.stream.Write(bufferData, 0, bufferData.Length);
            this.stream.Flush();
        }

        public void WriteString(string msg)
        {
            byte[] bufferData = encoder.GetBytes(msg);
            byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);

            this.stream.Write(bufferSize, 0, sizeof(int));
            this.stream.Write(bufferData, 0, bufferData.Length);
            this.stream.Flush();
        }
    }
}
