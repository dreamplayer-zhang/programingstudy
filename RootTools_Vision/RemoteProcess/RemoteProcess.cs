using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

    public enum REMOTE_PROCESS_STATE
    {
        None = 0,
        Start,
        Sync,
        ReadyWork,
        Working,
        WorkDone,
    }

    public class RemoteProcess
    {
        public event PipeCommMessageReceivedHandler MessageReceived;
        public event PipeCommConnection Connected;
        public event PipeCommConnection Disconnected;

        #region [Members]

        REMOTE_MODE mode;

        REMOTE_PROCESS_STATE processState = REMOTE_PROCESS_STATE.None;
        //Process process;
        private string processName = "RootTools_Vision";
        private PipeComm pipeComm = null;

        int timeoutMillisecond;

        #endregion

        public string ProcessName
        {
            get => this.processName;
        }

        public REMOTE_PROCESS_STATE ProcessState
        {
            get => this.processState;
            set => this.processState = value;
        }

        public RemoteProcess(REMOTE_MODE _mode = REMOTE_MODE.None, string moduleName = "module", int timeoutMillisecond = 3000)
        {
            this.timeoutMillisecond = timeoutMillisecond;

            this.mode = _mode;
            if (_mode == REMOTE_MODE.Master)
            {
                pipeComm = new PipeComm(moduleName, PIPE_MODE.Server);
                pipeComm.Connected += Connected_Callback;
                pipeComm.DisConnected += Disconnected_Callback;
            }
            else if(_mode == REMOTE_MODE.Slave)
            {
                pipeComm = new PipeComm(moduleName, PIPE_MODE.Client);
                pipeComm.Connected += Connected_Callback;
                pipeComm.DisConnected += Disconnected_Callback;
            }
        }

        public void EventReset()
        {
            MessageReceived = null;
        }

        public void EventChange(PipeCommMessageReceivedHandler handler)
        {
            MessageReceived = null;
            MessageReceived += handler;
        }

        private void Connected_Callback()
        {
            Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Connected !");
            pipeComm.MessageReceived += PipeComm_MessageReceived;

            if (Connected != null)
                Connected();

        }
        private void Disconnected_Callback()
        {
            Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Disconnected !");
            pipeComm.MessageReceived -= PipeComm_MessageReceived;

            if (Disconnected != null)
                Disconnected();
        }

        #region [Properties]
        public bool IsConnected
        {
            get => this.pipeComm.IsConnected;
        }

        #endregion


        #region [Communiation]
        private void PipeComm_MessageReceived(PipeProtocol protocol)
        {
            Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Message Received: " + protocol.msgType.ToString() + "/" + protocol.msg);

            if (this.MessageReceived != null)
                this.MessageReceived(protocol);
        }


        public void ListenStart()
        {
            pipeComm.Listen();
        }

        public void TryConnect()
        {
            if(!this.pipeComm.IsConnected)
            {
                pipeComm.Connect();
            }

            //int timeOutSecond = 10;
            //int sec = 0;
            //while(!this.pipeComm.IsConnected)
            //{
            //    if (sec > timeOutSecond)
            //    {
            //        Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Try connect time-out, Check server state");
            //        break;
            //    }
                    
            //    pipeComm.Connect();

            //    Thread.Sleep(1000);

            //    sec++;
            //}
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

            Logger.AddMsg(LOG_MESSAGE_TYPE.Network, "Message Sent: " + protocol.msgType.ToString() + "/" + protocol.msg);
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

        public void StartProcess()
        {
            ExitProcess();

            Process[] processes = Process.GetProcessesByName(this.processName);
            string startPath = this.processName + ".exe";
            Process.Start(startPath);
          
        }

        public bool CheckProcess()
        {
            Process[] processes = Process.GetProcessesByName(this.processName);
            if (processes.Length == 1)
                return true;
            else
                return false;
        }

        public void Exit()
        {
            ExitPipe();
            ExitProcess();
        }

        private void ExitProcess()
        {
            foreach (Process prc in Process.GetProcesses())
            {
                if (prc.ProcessName.StartsWith(processName))
                {
                    try
                    {
                        prc.Kill();
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void ExitPipe()
        {
            this.pipeComm.Exit();
        }
        #endregion
    }
}
