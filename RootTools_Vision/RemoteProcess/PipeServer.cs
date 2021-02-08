using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
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

        public delegate void MessageReceivedHandler(Client client, string message);

        public event MessageReceivedHandler MessageReceived;
        public const int BUFFER_SIZE = 4096;
        void _Dummy()
        {
            if (MessageReceived != null) MessageReceived(null, "");
        }

        string pipeName;
        Thread listenThread;
        bool running;
        List<Client> clients;

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
            this.clients = new List<Client>();
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

                Client client = new Client();
                client.handle = clientHandle;

                lock (clients)
                    this.clients.Add(client);

                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Start(client);
            }
        }

        /// <summary>
        /// Reads incoming data from connected clients
        /// </summary>
        /// <param name="clientObj"></param>
        private void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            while (true)
            {
                int bytesRead = 0;
                try
                {
                    byte[] bufferSize = new byte[sizeof(Int32)];
                    client.stream.Read(bufferSize, 0, sizeof(Int32));

                    int len = BitConverter.ToInt32(bufferSize, 0);
                    byte[] buffer = new byte[len];
                    client.stream.Read(buffer, 0, len);
                }
                catch
                {
                    //read error has occurred
                    break;
                }

                //client has disconnected
                if (bytesRead == 0)
                    break;

                //fire message received event
                //if (this.MessageReceived != null)
                //    this.MessageReceived(client, encoder.GetString(buffer, 0, bytesRead));
            }

            //clean up resources
            client.stream.Close();
            client.handle.Close();
            lock (this.clients)
                this.clients.Remove(client);
        }

        public void Send<T>(T obj)
        {
            if(this.clients.Count == 0)
            {
                MessageBox.Show("연결된 Client가 업습니다.");
            }

            lock (this.clients)
            {
                Client client = this.clients[0];


                client.stream.Write(ConvertIntToBytes((int)PIPE_MESSAGE_TYPE.Object), 0, BUFFER_SIZE);

                byte[] buffer = Tools.ObejctToByteArray<T>(obj);
                client.stream.Write(ConvertIntToBytes(buffer.Length), 0, BUFFER_SIZE);
                client.stream.Write(buffer, 0, buffer.Length);
                client.stream.Flush();
            }
        }

        public void Send(string msg)
        {
            if (this.clients.Count == 0)
            {
                MessageBox.Show("연결된 Client가 없습니다.");
            }

            lock (this.clients)
            {
                Client client = this.clients[0];

                client.stream.Write(ConvertIntToBytes((int)PIPE_MESSAGE_TYPE.String), 0, BUFFER_SIZE);

                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(msg);
                client.stream.Write(buffer, 0, buffer.Length);
                client.stream.Flush();
            }
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
