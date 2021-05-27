using RootTools;
using RootTools.Camera.Dalsa;
using RootTools.Comm;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Root_Pine2_Vision.Module
{
    public class Vision : ModuleBase
    {
        public enum eVision
        {
            Top3D,
            Top2D,
            Bottom
        }

        #region ToolBox
        Camera_Dalsa m_camera;
        public LightSet m_lightSet;
        public override void GetTools(bool bInit)
        {
            if (p_eRemote == eRemote.Server)
            {
                p_sInfo = m_toolBox.GetCamera(ref m_camera, this, "Camera"); 
                p_sInfo = m_toolBox.Get(ref m_lightSet, this);
                m_aVisionWorks[eWorks.A].GetTools(m_toolBox, bInit);
                m_aVisionWorks[eWorks.B].GetTools(m_toolBox, bInit);
            }
            m_remote.GetTools(bInit);
        }
        #endregion

        #region Light
        public string SetLight()
        {
            List<double> aPower = new List<double>();
            return SetLight(aPower); 
        }

        public string SetLight(List<double> aPower)
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.SetLight, eRemote.Client, aPower);
            else
            {
                while (aPower.Count < m_lightSet.m_aLight.Count) aPower.Add(0);
                for (int n = 0; n < aPower.Count; n++)
                {
                    LightBase light = m_lightSet.m_aLight[n].m_light;
                    if (light != null) light.p_fSetPower = aPower[n];
                }
            }
            return "OK";
        }
        #endregion

        #region VisionWorks
        public enum eWorks
        {
            A,
            B,
        }
        public class VisionWorks
        {
            MemoryPool m_memoryPool;
            TCPAsyncClient m_tcpip; 
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.Get(ref m_memoryPool, m_vision, "Memory" + p_id, 1); 
                toolBox.GetComm(ref m_tcpip, m_vision, "TCPIP" + p_id);
                if (bInit)
                {
                    InitMemory(); 
                    m_tcpip.EventReciveData += M_tcpip_EventReciveData;
                }
            }

            #region Memory
            public MemoryGroup m_memoryGroup;
            MemoryData m_memoryExt;
            MemoryData m_memoryColor;
            MemoryData[] m_memoryRGB = new MemoryData[3] { null, null, null };
            MemoryData[] m_memoryConv = new MemoryData[3] { null, null, null };
            MemoryData[] m_memoryHSI = new MemoryData[3] { null, null, null };
            MemoryData m_memoryGerbber;
            void InitMemory()
            {
                m_memoryGroup = m_memoryPool.GetGroup("Pine2");
                m_memoryExt = m_memoryGroup.CreateMemory("EXT", 2, 3, new CPoint(50000, 90000));
                m_memoryColor = m_memoryGroup.CreateMemory("Color", 1, 3, new CPoint(50000, 90000));
                m_memoryRGB[0] = m_memoryGroup.CreateMemory("Red", 1, 1, new CPoint(50000, 90000));
                m_memoryRGB[1] = m_memoryGroup.CreateMemory("Green", 1, 1, new CPoint(50000, 90000));
                m_memoryRGB[2] = m_memoryGroup.CreateMemory("Blue", 1, 1, new CPoint(50000, 90000));
                m_memoryConv[0] = m_memoryGroup.CreateMemory("Axial", 1, 1, new CPoint(50000, 90000));
                m_memoryConv[1] = m_memoryGroup.CreateMemory("Pad", 1, 1, new CPoint(50000, 90000));
                m_memoryConv[2] = m_memoryGroup.CreateMemory("Side", 1, 1, new CPoint(50000, 90000));
                m_memoryHSI[0] = m_memoryGroup.CreateMemory("Hui", 1, 1, new CPoint(50000, 90000));
                m_memoryHSI[1] = m_memoryGroup.CreateMemory("Saturation", 1, 1, new CPoint(50000, 90000));
                m_memoryHSI[2] = m_memoryGroup.CreateMemory("Intensity", 1, 1, new CPoint(50000, 90000));
                m_memoryGerbber = m_memoryGroup.CreateMemory("Gerbber", 1, 3, new CPoint(50000, 90000));
            }
            #endregion

            #region TCPIP
            private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
            {
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
                m_bThreadProcess = true;
                Thread.Sleep(10000);
                while (m_bThreadProcess)
                {
                    Thread.Sleep(100);
                    if (m_bStartProcess)
                    {
                        try
                        {
                            if (IsProcessRun() == false)
                            {
                                Process process = Process.Start(m_sFileVisionWorks, p_id);
                                m_nProcessID = process.Id;
                            }
                        }
                        catch (Exception e) { m_vision.p_sInfo = p_id + " StartProcess Error : " + e.Message; }
                    }
                }
            }

            public string m_idProcess = "VisionWorks";
            bool IsProcessRun()
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

            public void RunTree(Tree tree)
            {
                if (m_vision.p_eRemote == eRemote.Server)
                {
                    m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "VisionWorks Process ID");
                    m_sFileVisionWorks = tree.SetFile(m_sFileVisionWorks, m_sFileVisionWorks, ".exe", "File", "VisionWorks File Name");
                    m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start Memory Process");
                }
            }

            public eWorks m_eVisionWorks = eWorks.A; 
            public string p_id { get; set; }
            public Vision m_vision; 
            public VisionWorks(eWorks eVisionWorks, Vision vision)
            {
                m_eVisionWorks = eVisionWorks; 
                p_id = eVisionWorks.ToString();
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
        public Dictionary<eWorks, VisionWorks> m_aVisionWorks = new Dictionary<eWorks, VisionWorks>(); 
        List<string> m_asVisionWorks = new List<string>();
        void InitVisionWorks()
        {
            m_aVisionWorks.Add(eWorks.A, new VisionWorks(eWorks.A, this));
            m_aVisionWorks.Add(eWorks.B, new VisionWorks(eWorks.B, this));
            foreach (VisionWorks vision in m_aVisionWorks.Values) m_asVisionWorks.Add(vision.p_id); 
        }

        VisionWorks GetVisionWorks(string sVisionWorks)
        {
            foreach (VisionWorks vision in m_aVisionWorks.Values)
            {
                if (vision.p_id == sVisionWorks) return vision; 
            }
            return null; 
        }
        #endregion

        #region override
        public override void Reset()
        {
            if (p_eRemote == eRemote.Client) RemoteRun(eRemoteRun.Reset, eRemote.Client, null);
            else
            {
                base.Reset();
            }
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (p_eRemote == eRemote.Client) return RemoteRun(eRemoteRun.StateHome, eRemote.Client, null);
            else p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_aVisionWorks[eWorks.A].RunTree(tree.GetTree(m_aVisionWorks[eWorks.A].p_id));
            m_aVisionWorks[eWorks.B].RunTree(tree.GetTree(m_aVisionWorks[eWorks.B].p_id));
        }
        #endregion

        public eVision m_eVision = eVision.Top3D; 
        public Vision(eVision eVision, IEngineer engineer, eRemote eRemote)
        {
            m_eVision = eVision; 
            InitVisionWorks(); 
            InitBase("Vision " + eVision.ToString(), engineer, eRemote);
        }

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            InitVisionWorks();
            InitBase(id, engineer, eRemote);
        }

        public override void ThreadStop()
        {
            foreach (VisionWorks visionWorks in m_aVisionWorks.Values) visionWorks.ThreadStop(); 
            base.ThreadStop();
        }

        #region RemoteRun
        public enum eRemoteRun
        {
            StateHome,
            Reset,
            SetLight,
        }

        Run_Remote GetRemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = new Run_Remote(this);
            run.m_eRemoteRun = eRemoteRun;
            run.m_eRemote = eRemote;
            switch (eRemoteRun)
            {
                case eRemoteRun.StateHome: break;
                case eRemoteRun.Reset: break;
                case eRemoteRun.SetLight: run.m_aPower = value; break; 
            }
            return run;
        }

        string RemoteRun(eRemoteRun eRemoteRun, eRemote eRemote, dynamic value)
        {
            Run_Remote run = GetRemoteRun(eRemoteRun, eRemote, value);
            StartRun(run);
            while (run.p_eRunState != ModuleRunBase.eRunState.Done)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            return p_sInfo;
        }

        public class Run_Remote : ModuleRunBase
        {
            Vision m_module;
            public Run_Remote(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public eRemoteRun m_eRemoteRun = eRemoteRun.StateHome;
            public List<double> m_aPower = new List<double>(); 
            public override ModuleRunBase Clone()
            {
                Run_Remote run = new Run_Remote(m_module);
                run.m_eRemoteRun = m_eRemoteRun;
                foreach (double fPower in m_aPower) run.m_aPower.Add(fPower); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eRemoteRun = (eRemoteRun)tree.Set(m_eRemoteRun, m_eRemoteRun, "RemoteRun", "Select Remote Run", bVisible);
                m_eRemote = (eRemote)tree.Set(m_eRemote, m_eRemote, "Remote", "Remote", false);
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.SetLight: break;
                    default: break; 
                }
            }

            public override string Run()
            {
                switch (m_eRemoteRun)
                {
                    case eRemoteRun.StateHome: return m_module.StateHome();
                    case eRemoteRun.Reset: m_module.Reset(); break;
                    case eRemoteRun.SetLight: m_module.SetLight(m_aPower); break;
                }
                return "OK";
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Remote(this), true, "Remote Run");
            AddModuleRunList(new Run_Delay(this), true, "Time Delay");
            AddModuleRunList(new Run_Grab(this), true, "Time Delay");
        }

        public class Run_Delay : ModuleRunBase
        {
            Vision m_module;
            public Run_Delay(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secDelay = 2;
            public override ModuleRunBase Clone()
            {
                Run_Delay run = new Run_Delay(m_module);
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Time Delay (sec)", bVisible);
            }

            public override string Run()
            {
                Thread.Sleep((int)(1000 * m_secDelay / 2));
                return "OK";
            }
        }

        public class Run_Grab : ModuleRunBase
        {
            Vision m_module;
            public Run_Grab(Vision module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eBoat
            {
                Boat1,
                Boat2
            }
            eBoat m_eBoat = eBoat.Boat1; 
            public override ModuleRunBase Clone()
            {
                Run_Grab run = new Run_Grab(m_module);
                run.m_eBoat = m_eBoat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eBoat = (eBoat)tree.Set(m_eBoat, m_eBoat, "Boat", "Boat ID", bVisible);
            }

            public override string Run()
            {
                //forget
                return "OK";
            }
        }
        #endregion

    }
}
