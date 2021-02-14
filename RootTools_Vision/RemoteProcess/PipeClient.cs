using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    class PipeClient
    {
        public PipeClient(string pipeName)
        {
            this.pipeName = pipeName;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
          String pipeName,
          uint dwDesiredAccess,
          uint dwShareMode,
          IntPtr lpSecurityAttributes,
          uint dwCreationDisposition,
          uint dwFlagsAndAttributes,
          IntPtr hTemplate);

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public delegate void MessageReceivedHandler(PipeProtocol protocol);

        public event MessageReceivedHandler MessageReceived;

        void _Dummy()
        {
            if (MessageReceived != null) MessageReceived(new PipeProtocol()); 
        }

        public const int BUFFER_SIZE = 4096;

        string pipeName;
        private FileStream stream;
        private SafeFileHandle handle;
        Thread readThread;
        bool connected;

        public bool Connected
        {
            get { return this.connected; }
        }

        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        /// <summary>
        /// Connects to the server
        /// </summary>
        public void Connect()
        {
            this.handle =
               CreateFile(
                  @"\\.\pipe\" + this.pipeName,
                  GENERIC_READ | GENERIC_WRITE,
                  0,
                  IntPtr.Zero,
                  OPEN_EXISTING,
                  FILE_FLAG_OVERLAPPED,
                  IntPtr.Zero);

            //could not create handle - server probably not running
            if (this.handle.IsInvalid)
                return;

            this.connected = true;

            this.stream = new FileStream(this.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);

            //start listening for messages
            this.readThread = new Thread(new ThreadStart(Read));
            this.readThread.Start();
        }
        public void Send<T>(T obj)
        {
            byte[] bufferData = Tools.ObjectToByteArray<T>(obj);
            byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);

            this.stream.Write(bufferSize, 0, sizeof(int));
            this.stream.Write(bufferData, 0, bufferData.Length);
            this.stream.Flush();
        }

        public void Send(string msg)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] bufferData = encoder.GetBytes(msg);
            byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);

            this.stream.Write(bufferSize, 0, sizeof(int));
            this.stream.Write(bufferData, 0, bufferData.Length);

            this.stream.Flush();
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(message);

            this.stream.Write(messageBuffer, 0, messageBuffer.Length);
            this.stream.Flush();
        }

        private int ConvertBytesToInt(byte[] buffer)
        {
            byte[] temp = new byte[sizeof(int)];
            Buffer.BlockCopy(buffer, 0, temp, 0, sizeof(int));
            
            return (int)BitConverter.ToInt32(temp, 0);
        }

        public void Read()
        {
            byte[] bufferType = new byte[sizeof(PIPE_MESSAGE_TYPE)];
            bool isExit = false;
            int readBytes = 0;
            while (isExit == false)
            {

                readBytes = stream.Read(bufferType, 0, bufferType.Length);
                PIPE_MESSAGE_TYPE type = (PIPE_MESSAGE_TYPE)BitConverter.ToInt32(bufferType, 0);

                object data = null;
                string dataType = "";

                switch (type)
                {
                    case PIPE_MESSAGE_TYPE.Message:
                    case PIPE_MESSAGE_TYPE.Command:
                    case PIPE_MESSAGE_TYPE.Event:
                    case PIPE_MESSAGE_TYPE.Data:
                        {
                            byte[] bufferDataTypeSize = new byte[sizeof(int)];
                            readBytes = stream.Read(bufferDataTypeSize, 0, sizeof(int));
                            int dataTypeSize = BitConverter.ToInt32(bufferDataTypeSize, 0);

                            byte[] bufferDataType = new byte[dataTypeSize];
                            readBytes = stream.Read(bufferDataType, 0, dataTypeSize);
                            dataType = Tools.ByteArrayToObject<string>(bufferDataType);

                            byte[] bufferDataSize = new byte[sizeof(int)];
                            readBytes = stream.Read(bufferDataSize, 0, sizeof(int));
                            int dataSize = BitConverter.ToInt32(bufferDataSize, 0);

                            byte[] bufferData = new byte[dataSize];
                            readBytes = stream.Read(bufferData, 0, dataSize);
                            data = Tools.ByteArrayToObject(bufferData);
                        }
                        break;
                    default:
                        Debug.WriteLine("No Defined Message");
                        stream.Flush();
                        isExit = true;
                        break;
                }

                //if (MessageReceived != null)
                    //MessageReceived(new PipeProtocol(type, dataType, data));

                stream.Flush();
            }

            stream.Close();
            this.handle.Close();
        }



        public void Send(PipeProtocol protocol)
        {
            byte[] bufferType = BitConverter.GetBytes((int)protocol.msgType);
            stream.Write(bufferType, 0, bufferType.Length);

            switch (protocol.msgType)
            {
                case PIPE_MESSAGE_TYPE.Message:
                case PIPE_MESSAGE_TYPE.Command:
                case PIPE_MESSAGE_TYPE.Event:
                case PIPE_MESSAGE_TYPE.Data:
                    {
                        byte[] bufferDataType = Tools.ObejctToByteArray(protocol.dataType);
                        byte[] bufferDataTypeSize = Tools.ObejctToByteArray(bufferDataType.Length);

                        byte[] bufferData = Tools.ObjectToByteArray(protocol.data);
                        byte[] bufferDataSize = BitConverter.GetBytes(bufferData.Length);

                        stream.Write(bufferDataTypeSize, 0, sizeof(int));
                        stream.Write(bufferDataType, 0, bufferDataType.Length);
                        stream.Write(bufferDataSize, 0, sizeof(int));
                        stream.Write(bufferData, 0, bufferData.Length);
                    }
                    break;
            }

            stream.Flush();
        }
    }
}
