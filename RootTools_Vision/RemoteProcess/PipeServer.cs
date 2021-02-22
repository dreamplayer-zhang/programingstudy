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



    class PipeServer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
   String pipeName,
   uint dwOpenMode,
   uint dwPipeMode,
   uint nMaxInstances,
   uint nOutBufferSize,
   uint nInBufferSize,
   uint nDefaultTimeOut,
   IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }

        public delegate void MessageReceivedHandler(PipeProtocol protocol);

        public event MessageReceivedHandler MessageReceived;
        public const int BUFFER_SIZE = 4096;
        void _Dummy()
        {
            if (MessageReceived != null) MessageReceived(new PipeProtocol());
        }

        string pipeName;
        Thread listenThread;
        bool running;
        Client client;

        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        public bool Running
        {
            get { return this.running; }
        }

        public PipeServer(string pipeName)
        {
            this.pipeName = pipeName;
        }

        /// <summary>
        /// Starts the pipe server
        /// </summary>
        public void Start()
        {
            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            this.running = true;
        }

        /// <summary>
        /// Listens for client connections
        /// </summary>
        private void ListenForClients()
        {
            while (true)
            {
                SafeFileHandle clientHandle =
                CreateNamedPipe(
                     @"\\.\pipe\" + this.pipeName,
                     DUPLEX | FILE_FLAG_OVERLAPPED,
                     0,
                     255,
                     BUFFER_SIZE,
                     BUFFER_SIZE,
                     0,
                     IntPtr.Zero);

                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;

                client = new Client();
                client.handle = clientHandle;

                client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);

                Thread readThread = new Thread(new ThreadStart(Read));
                readThread.Start();
            }
        }

        public T Read<T>()
        {
            StreamDataReader reader = new StreamDataReader(client.stream);
            T obj = Tools.ByteArrayToObject<T>(reader.Read());
            client.stream.Close();

            return obj;
        }


        public void Read()
        {
            Client client = this.client;
            FileStream stream = this.client.stream;

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
                //    MessageReceived(new PipeProtocol(type, dataType, data));

                stream.Flush();
            }

            stream.Close();
            client.handle.Close();
        }

        public void Send<T>(T obj)
        {
            lock (this.client)
            {
                byte[] bufferData = Tools.ObjectToByteArray<T>(obj);
                byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);
                
                client.stream.Write(bufferSize, 0, sizeof(int));
                client.stream.Write(bufferData, 0, bufferData.Length);
                client.stream.Flush();
            }
        }

        public void Send(PipeProtocol protocol)
        {
            lock (this.client)
            {
                byte[] bufferType = BitConverter.GetBytes((int)protocol.msgType);
                client.stream.Write(bufferType, 0, bufferType.Length);

                switch (protocol.msgType)
                {
                    case PIPE_MESSAGE_TYPE.Message:
                    case PIPE_MESSAGE_TYPE.Command:
                    case PIPE_MESSAGE_TYPE.Event:
                    case PIPE_MESSAGE_TYPE.Data:
                        {
                            byte[] bufferDataType = Tools.ObejctToByteArray(protocol.dataType);
                            byte[] bufferDataTypeSize = BitConverter.GetBytes(bufferDataType.Length);
                            
                            byte[] bufferData = Tools.ObjectToByteArray(protocol.data);
                            byte[] bufferDataSize = BitConverter.GetBytes(bufferData.Length);


                            client.stream.Write(bufferDataTypeSize, 0, sizeof(int));
                            client.stream.Write(bufferDataType, 0, bufferDataType.Length);
                            client.stream.Write(bufferDataSize, 0, sizeof(int));
                            client.stream.Write(bufferData, 0, bufferData.Length);
                        }
                        break;
                }

                client.stream.Flush();
            }
        }

        public void Send(string msg)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] bufferData = encoder.GetBytes(msg);
            byte[] bufferSize = BitConverter.GetBytes(bufferData.Length);

            client.stream.Write(bufferSize, 0, sizeof(int));
            client.stream.Write(bufferData, 0, bufferData.Length);

            client.stream.Flush();
        }

        public byte[] ConvertIntToBytes(int data)
        {
            byte[] rst = new byte[BUFFER_SIZE];

            byte[] temp = new byte[sizeof(int)];
            temp = BitConverter.GetBytes(data);
            for(int i = 0; i < temp.Length; i++)
            {
                rst[i] = temp[i];
            }

            return rst;
        }
    }
}
