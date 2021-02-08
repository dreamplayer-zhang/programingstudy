using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public enum REMOTE_MODE
    {
        None = 0,
        Master,
        Slave
    }

    public class RemoteProcess
    {
        #region [Members]

        REMOTE_MODE mode;
        Process process;
        private string processName = "RootTools_Vision";
        private PipeComm pipe;
        private PipeServer pipeServer;
        private PipeClient pipeClient;

        #endregion

        public RemoteProcess(REMOTE_MODE _mode = REMOTE_MODE.None)
        {
            this.mode = _mode;
            if(this.mode == REMOTE_MODE.Master)
            {
                //pipe = new PipeComm(PIPE_MODE.Server, processName);
                pipeServer = new PipeServer(processName);
            }
            else if(this.mode == REMOTE_MODE.Slave)
            {
                //pipe = new PipeComm(PIPE_MODE.Client, processName);
                pipeClient = new PipeClient(processName);
            }
        }
        
        public void StartProcess()
        {
            if (this.mode == REMOTE_MODE.None)
            {
                //MessageBox.Show("Remote Mode 가 아닙니다.");
                return;
            }

            if (this.mode == REMOTE_MODE.Master)
            {
                //foreach (Process prc in Process.GetProcesses())
                //{
                //    if (prc.ProcessName.StartsWith(processName))
                //    {
                //        prc.Kill();
                //    }
                //}

                //process = Process.Start(processName + ".exe");

                pipeServer.Start();
            }
            else
            {
                pipeClient.Connect();
            }

            //pipe.Connect();
        }


        private void ConnectProcess()
        {
            if (this.mode == REMOTE_MODE.None) return;

            this.pipe.Connect();
        }

        public void ExitProcess()
        {
            foreach (Process prc in Process.GetProcesses())
            {
                if (prc.ProcessName.StartsWith(processName))
                {
                    prc.Kill();
                }
            }
        }

        public void Send<T>(T obj)
        {
            this.pipeServer.Send<T>(obj);

            return;
            if (obj == null) return;
            if(this.mode == REMOTE_MODE.None)
            {
                MessageBox.Show("Remote Mode를 사용할 수 없습니다.\n생성자에서 Remote Mode를 Master로 설정하세요");
                return;
            }
            this.pipe.Write<T>(obj);
        }

        public void Send(string msg)
        {
            this.pipeServer.Send(msg);

            return;
        }
    }
}
