using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
        Client
    }

    class PipeComm
    {
        PipeStream pipe;
        PIPE_MODE mode;

        Thread readThread;

        public PipeComm(PIPE_MODE mode, string pipeName)
        {
            this.mode = mode;
            if(mode == PIPE_MODE.Server)
            {
                pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
            }
            else
            {
                pipe = new NamedPipeClientStream(".",pipeName, PipeDirection.InOut);
            }
        }

        public void Connect()
        {
            if(this.mode == PIPE_MODE.Server)
            {
                NamedPipeServerStream server = pipe as NamedPipeServerStream;
                server.WaitForConnection();
                this.readThread = new Thread(new ThreadStart(Read));
                this.readThread.Start();

                MessageBox.Show("Server!! Client");
            }
            else
            {
                NamedPipeClientStream client = pipe as NamedPipeClientStream;
                client.Connect();
                this.readThread = new Thread(new ThreadStart(Read));
                this.readThread.Start();
                MessageBox.Show("Connect!! Client");
            }
        }

        public void Write<T>(T obj)
        {
            if(this.pipe.IsConnected == true)
            {
                //NamedPipeServerStream server = pipe as NamedPipeServerStream;
                //server.RunAsClient(() =>
                {
                    byte[] buffer = new byte[sizeof(Int32)];

                    Int32 len = buffer.Length;

                    try
                    {
                        this.pipe.Write(BitConverter.GetBytes(len), 0, sizeof(Int32));
                        this.pipe.WaitForPipeDrain();
                    }
                    catch(Exception)
                    {

                    }
                    
                    //this.pipe.WriteAsync(buffer, 0, buffer.Length);                    
                }
                
            }
        }

        public void Read()
        {
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    byte[] buffer;
                    buffer = new byte[sizeof(int)];
                    this.pipe.Read(buffer, 0, sizeof(int));

                    int len = BitConverter.ToInt32(buffer, 0);
                    buffer = new byte[len];
                    bytesRead = this.pipe.Read(buffer, 0, len);

                    WorkplaceBundle wb = Tools.ByteArrayToObject<WorkplaceBundle>(buffer);
                    MessageBox.Show(wb.Count.ToString());
                }
                catch
                {
                    //read error occurred
                    
                }

                //server has disconnected
                //if (bytesRead == 0)
                //    break;

                //fire message received event
                //if (this.MessageReceived != null)
                //    this.MessageReceived(encoder.GetString(readBuffer, 0, bytesRead));
            }



            //clean up resource
            //this.pipe.Close();
        }

        public void Write()
        {

        }
    }
}
