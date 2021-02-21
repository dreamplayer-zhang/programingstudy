using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{

    public enum PIPE_MODE
    {
        Server = 0,
        Client = 1
    }

    public delegate void PipeCommMessageReceivedHandler(PipeProtocol protocol);

    public class PipeComm
    {
        public event PipeCommMessageReceivedHandler MessageReceived;

        public PipeComm(string pipeName, PIPE_MODE mode)
        {
            this.pipeName = pipeName;
            this.mode = mode;
        }

        #region [Win32API]

        // Server
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

        // Client
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
        public const uint DUPLEX = (0x00000003);
        #endregion

        #region [Common]
        private string pipeName;
        private PIPE_MODE mode;
        public const int BUFFER_SIZE = 4096;
        private FileStream stream;
        private SafeFileHandle handle;
        Thread readThread;
        bool connected;

        Thread listenThread;
        //bool running;

        #endregion

        #region [Properties]
        public bool Connected
        {
            get { return this.connected; }
        }

        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }
        #endregion

        #region [Client]

        public void Connect()
        {
            if (this.mode == PIPE_MODE.Server)
            {
                throw new ArgumentException("Server는 Listen Method를 사용할 수 없습니다");
            }

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
        #endregion


        #region [Server]
        public void Listen()
        {
            if (this.mode == PIPE_MODE.Client)
            {
                throw new ArgumentException("Client는 Listen Method를 사용할 수 없습니다");
            }

            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            //this.running = true;
        }

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

                handle = clientHandle;
                stream = new FileStream(handle, FileAccess.ReadWrite, BUFFER_SIZE, true);

                this.connected = true;

                Thread readThread = new Thread(new ThreadStart(Read));
                readThread.Start();
            }
        }
        #endregion

        #region [Read/Write]
        private void Read()
        {
            byte[] bufferMsgType = new byte[sizeof(int)];
            byte[] bufferMsgSize = new byte[sizeof(int)];
            byte[] bufferDataTypeSize = new byte[sizeof(int)];
            byte[] bufferDataSize = new byte[sizeof(int)];

            bool isExit = false;
            int readBytes = 0;

            try
            {
                while (isExit == false)
                {
                    readBytes = stream.Read(bufferMsgType, 0, bufferMsgType.Length);
                    PIPE_MESSAGE_TYPE msgType = (PIPE_MESSAGE_TYPE)BitConverter.ToInt32(bufferMsgType, 0);

                    string msg = "";
                    object data = null;
                    string dataType = "";

                    // Read Msg
                    readBytes = stream.Read(bufferMsgSize, 0, bufferMsgSize.Length);
                    int msgSize = BitConverter.ToInt32(bufferMsgSize, 0);

                    byte[] bufferMsg = new byte[msgSize];
                    readBytes = stream.Read(bufferMsg, 0, bufferMsg.Length);
                    msg = Tools.ByteArrayToObject<string>(bufferMsg);

                    switch (msgType)
                    {
                        case PIPE_MESSAGE_TYPE.Message:
                            break;
                        case PIPE_MESSAGE_TYPE.Command:
                            break;
                        case PIPE_MESSAGE_TYPE.Event:
                        case PIPE_MESSAGE_TYPE.Data:
                            {
                                // Read Data Type
                                readBytes = stream.Read(bufferDataTypeSize, 0, sizeof(int));
                                int dataTypeSize = BitConverter.ToInt32(bufferDataTypeSize, 0);

                                if (dataTypeSize == 0) break;

                                byte[] bufferDataType = new byte[dataTypeSize];
                                readBytes = stream.Read(bufferDataType, 0, dataTypeSize);
                                dataType = Tools.ByteArrayToObject<string>(bufferDataType);


                                // Read Data
                                readBytes = stream.Read(bufferDataSize, 0, sizeof(int));
                                int dataSize = BitConverter.ToInt32(bufferDataSize, 0);

                                byte[] bufferData = new byte[dataSize];
                                readBytes = 0;
                                while (readBytes != dataSize)
                                {
                                    readBytes += stream.Read(bufferData, readBytes, dataSize - readBytes);
                                }
                                data = Tools.ByteArrayToObject(bufferData);
                            }
                            break;
                        default:
                            Debug.WriteLine("No Defined Message");
                            stream.Flush();
                            isExit = true;
                            break;
                    }
                    if (this.MessageReceived != null)
                        MessageReceived(new PipeProtocol(msgType, msg,dataType, data));

                    stream.Flush();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Connection 종료\n"+ex.Message);
            }
            finally
            {
                stream.Close();
                handle.Close();
            }
            if(handle.IsClosed == false)
            {
                stream.Close();
                handle.Close();
            }
        }


        private object lockObj = new object();
        public void Write(PipeProtocol protocol)
        {
            lock(lockObj)
            {
                if(stream == null)
                {
                    MessageBox.Show("연결된 Client가 없습니다.");
                    return;
                }
                byte[] bufferMsgType = BitConverter.GetBytes((int)protocol.msgType);
                stream.Write(bufferMsgType, 0, bufferMsgType.Length);

                byte[] bufferMsg = Tools.ObjectToByteArray<string>(protocol.msg);
                byte[] bufferMsgSize = BitConverter.GetBytes(bufferMsg.Length);

                stream.Write(bufferMsgSize, 0, bufferMsgSize.Length);
                stream.Write(bufferMsg, 0, bufferMsg.Length);

                switch (protocol.msgType)
                {
                    case PIPE_MESSAGE_TYPE.Message:
                        break;
                    case PIPE_MESSAGE_TYPE.Command:
                        break;
                    case PIPE_MESSAGE_TYPE.Event:
                        {
                            byte[] bufferDataType = Tools.ObejctToByteArray(protocol.dataType);
                            byte[] bufferDataTypeSize = BitConverter.GetBytes(bufferDataType.Length);

                            if(protocol.data == null)
                            {
                                break;
                            }
                            byte[] bufferData = Tools.ObjectToByteArray(protocol.data);
                            byte[] bufferDataSize = BitConverter.GetBytes(bufferData.Length);

                            stream.Write(bufferDataTypeSize, 0, sizeof(int));
                            stream.Write(bufferDataType, 0, bufferDataType.Length);
                            stream.Write(bufferDataSize, 0, sizeof(int));
                            stream.Write(bufferData, 0, bufferData.Length);
                        }
                        break;
                    case PIPE_MESSAGE_TYPE.Data:
                        {
                            byte[] bufferDataType = Tools.ObejctToByteArray(protocol.dataType);
                            byte[] bufferDataTypeSize = BitConverter.GetBytes(bufferDataType.Length);

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
        #endregion
    }

}
