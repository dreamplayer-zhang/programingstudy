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
        public event PipeCommMessageReceivedHandler MessageReceived;

        #region [Members]

        REMOTE_MODE mode;
        //Process process;
        private string processName = "RootTools_Vision";
        private PipeComm pipeComm = null;

        int timeoutMillisecond;

        #endregion

        public RemoteProcess(REMOTE_MODE _mode = REMOTE_MODE.None, string moduleName = "module", int timeoutMillisecond = 3000)
        {
            this.timeoutMillisecond = timeoutMillisecond;

            this.mode = _mode;
            if (_mode == REMOTE_MODE.Master)
            {
                pipeComm = new PipeComm(moduleName, PIPE_MODE.Server);
                pipeComm.MessageReceived += PipeComm_MessageReceived;
            }
            else if(_mode == REMOTE_MODE.Slave)
            {
                pipeComm = new PipeComm(moduleName, PIPE_MODE.Client);
                pipeComm.MessageReceived += PipeComm_MessageReceived;
            }
        }


        #region [Communiation]
        private void PipeComm_MessageReceived(PipeProtocol protocol)
        {
            if (this.MessageReceived != null)
                this.MessageReceived(protocol);
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

                pipeComm.Listen();
            }
            else if(this.mode == REMOTE_MODE.Slave)
            {
                pipeComm.Connect();
            }
        }

        public void Send(PipeProtocol protocol)
        {
            if (this.mode == REMOTE_MODE.Master)
            {
                this.pipeComm.Write(protocol);
            }
            else
            {
                this.pipeComm.Write(protocol);
            }
        }

        public void Read(PipeProtocol protocol)
        {
            if (this.mode == REMOTE_MODE.Master)
            {
                //this.pipeServer.Read(protocol);
            }
            else
            {
                //this.pipeClient.Read(protocol);
            }
            return;
        }
        #endregion

        #region [Process]
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
        #endregion
    }
}
