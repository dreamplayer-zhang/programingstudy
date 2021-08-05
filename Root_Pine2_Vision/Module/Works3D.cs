using RootTools;
using RootTools.Comm;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public class Works3D
    {
        MemoryPool m_memoryPool;
        public TCPAsyncClient m_tcpip;
        public void GetTools(ToolBox toolBox, bool bInit)
        {
            toolBox.Get(ref m_memoryPool, m_vision, "Memory" + p_id, 1);
            toolBox.GetComm(ref m_tcpip, m_vision, "Works" + p_id);
            if (bInit)
            {
                InitMemory();
                m_tcpip.EventReceiveData += M_tcpip_EventReceiveData;
                m_tcpip.ThreadStop();
            }
        }

        #region Memory
        public MemoryGroup m_memoryGroup;
        // 2D
        MemoryData[] m_memoryExt = new MemoryData[2] { null, null };
        MemoryData[] m_memoryColor = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryRGB = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryAPS = new MemoryData[3] { null, null, null };
        MemoryData[] m_memoryHSI = new MemoryData[3] { null, null, null };

        // 3D
        MemoryData m_memory2DViewH = null;
        MemoryData m_memory2DViewB = null;
        MemoryData m_memoryHeight = null;
        MemoryData m_memoryBright = null;
        MemoryData m_memoryRawImage = null;

               
        public const string Mem3DViewH = "2D View H";
        public const string Mem3DViewB = "2D View B";
        public const string Mem3DHeight = "Height";
        public const string Mem3DBright= "Bright";
        public const string Mem3DRaw = "Raw";
        List<MemoryData> m_aMemory = new List<MemoryData>();
        void InitMemory()
        {
            m_memoryGroup = m_memoryPool.GetGroup("Pine2");

            // 2D
            m_aMemory.Add(m_memoryExt[0] = m_memoryGroup.CreateMemory("EXT1", 3, 1, new CPoint(10000, 10000))); // Red Green Blue      -> VisionWorks2 Gerbber/RGBtoG/CtoG
            m_aMemory.Add(m_memoryExt[1] = m_memoryGroup.CreateMemory("EXT2", 3, 1, new CPoint(10000, 10000))); // Axial Pad Side      -> VisionWorks2 Ext1/Ext2/SideTemp

            m_aMemory.Add(m_memoryColor[0] = m_memoryGroup.CreateMemory("Color1", 1, 3, new CPoint(10000, 10000))); // RGB 합성 이미지 버퍼
            m_aMemory.Add(m_memoryColor[1] = m_memoryGroup.CreateMemory("Color2", 1, 3, new CPoint(10000, 10000))); // APS 합성 이미지 버퍼
            m_aMemory.Add(m_memoryColor[2] = m_memoryGroup.CreateMemory("Color3", 1, 3, new CPoint(10000, 10000))); // HSI 합성 이미지 버퍼
            m_aMemory.Add(m_memoryRGB[0] = m_memoryGroup.CreateMemory("Red", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryRGB[1] = m_memoryGroup.CreateMemory("Green", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryRGB[2] = m_memoryGroup.CreateMemory("Blue", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryAPS[0] = m_memoryGroup.CreateMemory("Axial", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryAPS[1] = m_memoryGroup.CreateMemory("Pad", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryAPS[2] = m_memoryGroup.CreateMemory("Side", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryHSI[0] = m_memoryGroup.CreateMemory("Hue", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryHSI[1] = m_memoryGroup.CreateMemory("Saturation", 1, 1, new CPoint(10000, 10000)));
            m_aMemory.Add(m_memoryHSI[2] = m_memoryGroup.CreateMemory("Intensity", 1, 1, new CPoint(10000, 10000)));

            // 3D
            m_aMemory.Add(m_memory2DViewH = m_memoryGroup.CreateMemory(Mem3DViewH, 1, 1, new CPoint(40000, 80000)));
            m_aMemory.Add(m_memory2DViewB = m_memoryGroup.CreateMemory(Mem3DViewB, 1, 1, new CPoint(40000, 80000)));
            m_aMemory.Add(m_memoryHeight = m_memoryGroup.CreateMemory(Mem3DHeight, 1, 2, new CPoint(40000, 80000)));
            m_aMemory.Add(m_memoryBright = m_memoryGroup.CreateMemory(Mem3DBright, 1, 2, new CPoint(40000, 80000)));
            m_aMemory.Add(m_memoryRawImage = m_memoryGroup.CreateMemory(Mem3DRaw, 1, 1, new CPoint(153600, 80000)));


            string regGroup = "MMF Data " + p_id;   // MMF Data A, MMF Data B
            Registry reg = new Registry(false, regGroup, "MemoryOffset");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_mbOffset);
            reg = new Registry(false, regGroup, "MemoryDepth");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_nByte);
            reg = new Registry(false, regGroup, "MemorySizeX");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_sz.X);
            reg = new Registry(false, regGroup, "MemorySizeY");
            foreach (MemoryData mem in m_aMemory) reg.Write(mem.p_id, mem.p_sz.Y);
        }

        public MemoryData[] p_memSnap
        {
            get { return m_memoryExt; }
        }
        #endregion

        #region TCPIP
        Queue<Protocol> m_qProtocol = new Queue<Protocol>();
        Protocol m_protocolSend = null;

        int m_iProtocol = 0;
        int m_secTimeout = 2;
        string m_sRecipe = "";
        public string SendRecipe(string sRecipe)
        {
            m_sRecipe = sRecipe;
            return "OK";
        }

        public string SendSnapDone(int iSnap)
        {
            if (m_bStartProcess == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SnapDone, m_sRecipe, iSnap);
            m_qProtocol.Enqueue(protocol);
            return WaitReply(m_secTimeout);
        }

        public string SendSnapInfo(SnapInfo snapInfo)
        {
            if (m_bStartProcess == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SnapInfo, snapInfo);
            m_qProtocol.Enqueue(protocol);
            return WaitReply(m_secTimeout);
        }

        public string SendLotInfo(LotInfo lotInfo)
        {
            if (m_bStartProcess == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.LotInfo, lotInfo);
            m_qProtocol.Enqueue(protocol);
            return WaitReply(m_secTimeout);
        }

        public string SendSortInfo(SortInfo sortInfo)
        {
            if (m_bStartProcess == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.SortingData, sortInfo);
            m_qProtocol.Enqueue(protocol);
            return WaitReply(m_secTimeout);
        }

        public string SendReset()
        {
            if (m_bStartProcess == false) return "OK";
            Protocol protocol = new Protocol(m_iProtocol, eProtocol.Reset);
            m_qProtocol.Enqueue(protocol);
            return WaitReply(m_secTimeout);
        }

        bool m_bWait = true;
        string WaitReply(int secTimeout)
        {
            int msTimeout = 1000 * secTimeout;
            StopWatch sw = new StopWatch();
            try
            {
                while (m_bWait)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                    if (sw.ElapsedMilliseconds > msTimeout) return "Protocol Recieve Timeout";
                }
            }
            finally { m_protocolSend = null; }
            return "OK";
        }
        
        void ThreadSend()
        {
            if (m_protocolSend != null) return;
            if (m_qProtocol.Count == 0) return;
            m_protocolSend = m_qProtocol.Dequeue();
            m_tcpip.Send(m_protocolSend.m_sSend);
        }

        private void M_tcpip_EventReceiveData(byte[] aBuf, int nSize, Socket socket)
        {
            string sSend = Encoding.Default.GetString(aBuf, 0, nSize);
            if (aBuf[0] == '[')
            {
                Reply(sSend);
                return;
            }
            if (m_protocolSend == null) return;
            m_protocolSend.ReceiveData(sSend);
            m_protocolSend = null;
            m_bWait = false;
        }

        void Reply(string sSend)
        {
            try
            {
                string[] asSend = sSend.Split(',');
                if (asSend[1] == eProtocol.Snap.ToString())
                {
                    string sRecipe = asSend[2];
                    string sInfo = m_vision.ReqSnap(sRecipe, p_eWorks);
                    m_tcpip.Send(sSend.Substring(0, sSend.Length - 1) + "," + sInfo + "]");
                }
                if (asSend[1] == eProtocol.InspDone.ToString())
                {
                    string sStripID = asSend[2];
                    string sStripResult = asSend[3];
                    string sX = asSend[4];
                    string sY = asSend[5];
                    string sMapResult = asSend[6].Substring(0, asSend[6].Length - 1);
                    string sInfo = m_vision.ReqInspDone(sStripID, sStripResult, sX, sY, sMapResult, p_eWorks);
                    m_tcpip.Send(sSend.Substring(0, sSend.Length - 1) + "," + sInfo + "]");
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Process
        string m_sFileVisionWorks = "C:\\WisVision\\VisionWorks2.exe";
        bool m_bThreadProcess = false;
        Thread m_threadProcess = null;
        void InitThreadProcess()
        {
            m_threadProcess = new Thread(new ThreadStart(RunThreadProcess));
            m_threadProcess.Start();
        }

        bool m_bStartProcess = false;
        int m_nProcessID = -1;
        void RunThreadProcess()
        {
            int nProcess = 0;
            m_bThreadProcess = true;
            Thread.Sleep(7000);
            while (m_bThreadProcess)
            {
                Thread.Sleep(10);
                ThreadSend();
                if (m_bStartProcess && (nProcess > 100))
                {
                    nProcess = 0;
                    try
                    {
                        if (IsMemoryPool() && (IsProcessRun() == false))
                        {
                            m_sRecipe = "";
                            m_tcpip.ThreadStop();
                            m_tcpip.InitClient();
                            ProcessStartInfo startInfo = new ProcessStartInfo(m_sFileVisionWorks);
                            startInfo.Arguments = p_id + "." + m_tcpip.p_nPort.ToString();
                            startInfo.WorkingDirectory = "C://WisVision//";
                            Process process = Process.Start(startInfo);

                            //Process process = Process.Start(m_sFileVisionWorks, p_id + "." + m_tcpip.p_nPort.ToString());
                            m_nProcessID = process.Id;
                            Thread.Sleep(2000);
                            if (m_vision.m_lotInfo != null) SendLotInfo(m_vision.m_lotInfo);
                        }
                        else if (m_tcpip.p_bConnect == false)
                        {
                            if (m_tcpip.p_bUse) m_tcpip.Connect();
                        }
                    }
                    catch (Exception e) { m_vision.p_sInfo = p_id + " StartProcess Error : " + e.Message; }
                }
                nProcess++;
            }
        }

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

        bool IsMemoryPool()
        {
            return true;
            //MemoryPool pool = new MemoryPool(m_memoryPool.p_id);
            //return pool.m_MMF != null; 
        }
        #endregion

        public void Reset()
        {
            m_protocolSend = null;
            m_qProtocol.Clear();
        }

        public void RunTree(Tree tree)
        {
            if (m_vision.p_eRemote == ModuleBase.eRemote.Server)
            {
                m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "VisionWorks Process ID");
                m_sFileVisionWorks = tree.SetFile(m_sFileVisionWorks, m_sFileVisionWorks, "exe", "File", "VisionWorks File Name");
                m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start VisionWorks Process");
            }
        }

        public eWorks p_eWorks { get; set; }
        public string p_id { get; set; }
        public Vision3D m_vision;
        public Works3D(eWorks eWorks, Vision3D vision)
        {
            p_eWorks = eWorks;
            p_id = eWorks.ToString();
            m_vision = vision;
            InitThreadProcess();
        }

        public void ThreadStop()
        {
            if (m_bThreadProcess)
            {
                m_bThreadProcess = false;
                m_threadProcess.Join();
            }
        }
    }
}
