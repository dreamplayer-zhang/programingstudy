using RootTools.Comm;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_JEDI_Vision.Module
{
    public class VisionProcess
    {
        #region ToolBox
        public TCPAsyncClient m_tcpip;
        public void GetTools(ToolBox toolBox, bool bInit)
        {
            toolBox.GetComm(ref m_tcpip, (ModuleBase)m_vision, "Works");
            if (bInit)
            {
                m_tcpip.EventReceiveData += M_tcpip_EventReceiveData;
                m_tcpip.ThreadStop();
            }
        }
        #endregion

        #region TCPIP
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        Protocol m_protocolSend = null;

        private void M_tcpip_EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sSend = Encoding.Default.GetString(aBuf, 0, nSize);
            if (m_protocolSend == null) return;
            m_protocolSend.ReceiveData(sSend);
            m_protocolSend = null;
        }

        int m_iProtocol = 0;
        int m_secTimeout = 2;
        public string SendSnapInfo(SnapInfo snapInfo)
        {
            if (p_bStart == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SnapInfo, snapInfo);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitReply(m_secTimeout);
        }

        public string SendSnapDone(string sRecipe, int iSnap)
        {
            if (p_bStart == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SnapDone, sRecipe, iSnap);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitReply(m_secTimeout);
        }

        LotInfo m_lotInfo = null;
        public string SendLotInfo(LotInfo lotInfo)
        {
            if (p_bStart == false) return "OK";
            m_lotInfo = lotInfo; 
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.LotInfo, lotInfo);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitReply(m_secTimeout);
        }

        public string SendSortInfo(SortInfo sortInfo)
        {
            if (p_bStart == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SortingData, sortInfo);
            m_qProtocol.Enqueue(protocol);
            return protocol.WaitReply(m_secTimeout);
        }

        void ThreadSend()
        {
            if (m_protocolSend != null) return;
            if (m_qProtocol.Count == 0) return;
            m_protocolSend = m_qProtocol.Dequeue();
            m_tcpip.Send(m_protocolSend.m_sSend);
        }
        #endregion

        bool p_bStart { get; set; }
        #region Thread
        string m_sFile = "C:\\WisVision\\VisionWorks2.exe";
        bool m_bThread = false;
        Thread m_thread = null;
        void RunThread()
        {
            int nProcess = 0;
            m_bThread = true;
            Thread.Sleep(7000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                ThreadSend();
                if (p_bStart && (nProcess > 100))
                {
                    nProcess = 0;
                    try
                    {
                        if (IsProcessRun() == false)
                        {
                            m_tcpip.ThreadStop();
                            m_tcpip.InitClient();
                            ProcessStartInfo startInfo = new ProcessStartInfo(m_sFile);
                            startInfo.Arguments = m_vision.p_id + "." + m_tcpip.p_nPort.ToString();
                            startInfo.WorkingDirectory = "C://WisVision//";
                            Process process = Process.Start(startInfo);
                            m_nProcessID = process.Id;
                            Thread.Sleep(2000);
                            if (m_lotInfo != null) SendLotInfo(m_lotInfo);
                        }
                        else if (m_tcpip.p_bConnect == false)
                        {
                            if (m_tcpip.p_bUse) m_tcpip.Connect();
                        }
                    }
                    catch (Exception e) { m_vision.p_sInfo = m_vision.p_id + " StartProcess Error : " + e.Message; }
                }
                nProcess++;
            }
        }

        int m_nProcessID = -1;
        public string m_idProcess = "VisionWorks2";
        public bool IsProcessRun()
        {
            Process[] aProcess = Process.GetProcessesByName(m_idProcess);
            if (aProcess.Length == 0) return false;
            foreach (Process process in aProcess)
            {
                if (process.Id == m_nProcessID) return true;
            }
            return false;
        }
        #endregion

        public void Reset()
        {
            m_protocolSend = null;
            m_qProtocol.Clear();
        }

        public void RunTree(Tree tree)
        {
            m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "VisionWorks Process ID");
            m_sFile = tree.SetFile(m_sFile, m_sFile, "exe", "File", "VisionWorks File Name");
            p_bStart = tree.Set(p_bStart, p_bStart, "Start", "Start VisionWorks Process");
        }

        IVision m_vision; 
        public VisionProcess(IVision vision)
        {
            m_vision = vision; 
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
    }
}
