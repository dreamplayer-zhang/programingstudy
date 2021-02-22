using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    class StreamDataReader
    {
        FileStream stream;

        public StreamDataReader(FileStream fs)
        {
            this.stream = fs;
        }

        public byte[] Read()
        {
            int readBytes = 0;

            // Read Data Size
            byte[] bufferSize = new byte[sizeof(int)];
            readBytes = this.stream.Read(bufferSize, 0, sizeof(int));
            if (readBytes != sizeof(int))
            {
                MessageBox.Show("MessageRead Fail");
                this.stream.Flush();
                return null;
            }

            int dataSize = BitConverter.ToInt32(bufferSize, 0);

            // Read Data

            byte[] bufferData = new byte[dataSize];

            readBytes = 0;
            while (dataSize != readBytes)
            {
                if (dataSize < readBytes)
                {
                    MessageBox.Show("Error readBytes over bufferSize");
                    this.stream.Flush();
                    break;
                }

                readBytes += this.stream.Read(bufferData, readBytes, dataSize - readBytes);
            }

            return bufferData;
        }
    }
}
