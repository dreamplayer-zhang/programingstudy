using RootTools;
using RootTools.Camera.BaslerPylon;
using RootTools.Camera.Dalsa;
using RootTools.Camera.Matrox;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_VEGA_P_Vision.Module
{
    public class Vision : ModuleBase, IRTRChild
    {
        #region ToolBox
        LightSet lightSet;
        MemoryPool memoryPool;
        MemoryGroup memoryGroup;
        public int sideGrabCnt = 0;
        public enum eParts
        {
            EIP_Cover, EIP_Plate
        }
        public enum eUpDown
        {
            Front, Back
        }
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref memoryPool, this, "Memory", 1);
            p_sInfo = m_toolBox.Get(ref lightSet, this);
            m_stage.GetTools(m_toolBox, bInit); 
            m_mainOptic.GetTools(m_toolBox, bInit); //TDI, Stain, ZStack
            m_sideOptic.GetTools(m_toolBox, bInit);  //Side
            m_remote.GetTools(bInit);
        }
        #endregion

        #region[Move]
        public string Move(Axis axis, double pos, bool bWait = true)
        {
            string sRun = axis.StartMove(pos);
            if (sRun.Equals("OK")) return sRun;
            return bWait ? axis.WaitReady() : "OK";
        }
        public string Move(Axis axis, double pos,double v,bool bWait = true)
        {
            string sRun = axis.StartMove(pos,v);
            if (sRun.Equals("OK")) return sRun;
            return bWait ? axis.WaitReady() : "OK";
        }
        public string MoveXY(RPoint posmm, bool bWait = true)
        {
            string sRun = m_stage.m_axisXY.StartMove(new RPoint(posmm));
            if (sRun.Equals("OK")) return sRun;
            return bWait ? m_stage.m_axisXY.WaitReady() : "OK";
        }
        #endregion

        #region Stage
        public class Stage : NotifyProperty
        {
            public AxisXY m_axisXY;
            public Axis m_axisR;
            DIO_I[] m_diStageLoad = new DIO_I[2] { null, null };
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisXY, m_vision, "Stage");
                m_vision.p_sInfo = toolBox.Get(ref m_axisR, m_vision, "Stage Rotate");
                m_vision.p_sInfo = toolBox.Get(ref m_diStageLoad[0], m_vision, "Stage Load X");
                m_vision.p_sInfo = toolBox.Get(ref m_diStageLoad[1], m_vision, "Stage Load Y");
                if (bInit)
                {

                }
            }

            public bool IsLoad()
            {
                if (m_diStageLoad[0] == null) return false;
                if (m_diStageLoad[1] == null) return false;
                return m_diStageLoad[0].p_bIn && m_diStageLoad[1].p_bIn; 
            }

            double _pulsePerRound = 360000;
            public double p_pulsePerRound
            {
                get { return _pulsePerRound; }
                set
                {
                    _pulsePerRound = value;
                    OnPropertyChanged(); 
                }
            }

            public string Rotate(double degree, bool bWait = true)
            {
                string sRun = m_axisR.StartMove(degree * p_pulsePerRound / 360);
                if (sRun != "OK") return sRun;
                return bWait ? m_axisR.WaitReady() : "OK";
            }

            double m_pulsePermm = 10000; 

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)"); 
                p_pulsePerRound = tree.Set(p_pulsePerRound, p_pulsePerRound, "Pulse / Round", "Stage Rotate Pulse per Round (pulse)"); 
            }

            Vision m_vision; 
            public Stage(Vision vision)
            {
                m_vision = vision; 
            }
        }
        public Stage m_stage;
        #endregion

        #region MainOptic
        public class MainOptic : NotifyProperty
        {
            public Axis m_axisZ;
            public Camera_Dalsa camTDI;
            public Camera_Basler camStain;
            public Camera_Matrox camZStack;

            public enum eInsp
            {
                Stain,Main
            }

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref m_axisZ, m_vision, "Main Optic AxisZ");
                m_vision.p_sInfo = toolBox.Get(ref camTDI, m_vision, "TDI Cam");
                m_vision.p_sInfo = toolBox.Get(ref camStain, m_vision, "Stain Cam");
                m_vision.p_sInfo = toolBox.Get(ref camZStack, m_vision, "Z-Stacking Cam");

                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                foreach(var v in Enum.GetValues(typeof(eParts)))
                    foreach(var v2 in Enum.GetValues(typeof(eUpDown)))
                        m_vision.memoryGroup.CreateMemory(v.ToString() + "." + v2.ToString(), 1, 1, 1000,1000);
            }
            public MemoryData GetMemoryData(eParts parts,eUpDown updown)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, parts.ToString()+"."+updown.ToString());
            }
            public double m_pulsePermm = 10000;

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            Vision m_vision;
            public MainOptic(Vision vision)
            {
                m_vision = vision;
                StainMems = new MemoryData[4];
                MainMems = new MemoryData[4];
            }
        }
        public MainOptic m_mainOptic;
        #endregion

        #region SideOptic
        public class SideOptic : NotifyProperty
        {
            public Camera_Basler camSide;

            public enum eSide
            {
                Top,Left,Bottom,Right
            }
            public Axis axisZ;

            public void GetTools(ToolBox toolBox, bool bInit)
            {
                if (m_vision.p_eRemote == eRemote.Client) return;
                m_vision.p_sInfo = toolBox.Get(ref axisZ, m_vision, "Side Optic AxisZ");
                m_vision.p_sInfo = toolBox.Get(ref camSide, m_vision, "Side Cam");
                if (bInit)
                {

                }
            }

            public void InitMemorys()
            {
                foreach (var v in Enum.GetValues(typeof(eParts)))
                    foreach (var v2 in Enum.GetValues(typeof(eSide)))
                        m_vision.memoryGroup.CreateMemory(v.ToString() + "." + v2.ToString(), 1, 1, 1000, 1000);
            }
            public MemoryData GetMemoryData(eParts parts,eSide side)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, parts.ToString()+"."+side.ToString());
            }
            public MemoryData GetMemoryData(string str)
            {
                return m_vision.memoryPool.GetMemory(m_vision.p_id, str);
            }
            public double m_pulsePermm = 10000;

            public void RunTree(Tree tree)
            {
                m_pulsePermm = tree.Set(m_pulsePermm, m_pulsePermm, "Pulse / mm", "Stage XY Pulse per 1mm (pulse)");
            }

            Vision m_vision;
            public SideOptic(Vision vision)
            {
                m_vision = vision;
            }
        }
        public SideOptic m_sideOptic;
        #endregion

        #region InfoPod
        InfoPod _infoPod = null;
        public InfoPod p_infoPod
        {
            get { return _infoPod; }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1; 
                _infoPod = value;
                m_reg.Write("InfoPod", nPod);
                value.WriteReg(); 
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            int nPod = m_reg.Read("InfoPod", -1);
            if (nPod < 0) return;  
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }
        #endregion

        #region IRTRChild
        public string IsGetOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready"; 
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            switch (infoPod.p_ePod)
            {
                case InfoPod.ePod.EOP_Dome:
                case InfoPod.ePod.EOP_Door:
                    return p_id + " Invalid Pod Type"; 
            }
            return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
        }

        public string BeforeGet()
        {
            // Move to Ready Pos ?
            // Vacuum Off ?
            return "OK";
        }

        public string BeforePut(InfoPod infoPod)
        {
            // Move to Ready Pos ?
            // Vacuum Off ?
            return "OK";
        }

        public string AfterGet()
        {
            // ??
            return "OK";
        }

        public string AfterPut()
        {
            // ??
            return "OK";
        }

        public bool IsPodExist()
        {
            return (p_infoPod != null);
        }
        #endregion 

        #region Teach RTR
        Buffer.TeachRTR m_teach; 
        public int GetTeachRTR(InfoPod infoPod)
        {
            return m_teach.GetTeach(infoPod);
        }

        public void RunTreeTeach(Tree tree)
        {
            m_teach.RunTree(tree.GetTree(p_id));
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }

        public override void InitMemorys()
        {
            memoryGroup = memoryPool.GetGroup(p_id);
            m_mainOptic.InitMemorys();
            m_sideOptic.InitMemorys(); 
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";

            if (p_eRemote == eRemote.Client)
            {
                m_remote.RemoteSend(Remote.eProtocol.Initial, "INIT", "INIT");
                return "OK";
            }
            else
            {
                p_sInfo = base.StateHome();
                p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
                return "OK";
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_stage.RunTree(tree.GetTree("Stage"));
            m_mainOptic.RunTree(tree.GetTree("Main Optic"));
            m_sideOptic.RunTree(tree.GetTree("Side Optic"));
            RunTreeGrabMode(tree.GetTree("Grab Mode"));
        }
        #endregion

        #region Grab Mode
        int m_lGrabMode = 0;
        public ObservableCollection<GrabMode> m_aGrabMode = new ObservableCollection<GrabMode>();
        public List<string> p_asGrabMode
        {
            get
            {
                List<string> asGrabMode = new List<string>();
                foreach (GrabMode grabMode in m_aGrabMode) 
                    asGrabMode.Add(grabMode.p_sName);
                return asGrabMode;
            }
        }

        public GrabMode GetGrabMode(string sGrabMode)
        {
            foreach (GrabMode grabMode in m_aGrabMode)
                if (sGrabMode == grabMode.p_sName) return grabMode;
          
            return null;
        }

        void RunTreeGrabMode(Tree tree)
        {
            m_lGrabMode = tree.Set(m_lGrabMode, m_lGrabMode, "Count", "Grab Mode Count");
            while (m_aGrabMode.Count < m_lGrabMode)
            {
                string id = "Mode." + m_aGrabMode.Count.ToString("00");
                GrabMode grabMode = new GrabMode(id, m_cameraSet, lightSet, memoryPool);
                m_aGrabMode.Add(grabMode);
            }
            while (m_aGrabMode.Count > m_lGrabMode) m_aGrabMode.RemoveAt(m_aGrabMode.Count - 1);
            foreach (GrabMode grabMode in m_aGrabMode) grabMode.RunTreeName(tree.GetTree("Name", false));
            foreach (GrabMode grabMode in m_aGrabMode)
                grabMode.RunTree(tree.GetTree(grabMode.p_sName, false), true, false);
        }

        #endregion

        public Vision(string id, IEngineer engineer, eRemote eRemote)
        {
            m_reg = new Registry(p_id + "_InfoPod");
            m_teach = new Buffer.TeachRTR(); 
            m_stage = new Stage(this);
            m_mainOptic = new MainOptic(this);
            m_sideOptic = new SideOptic(this); 
            InitBase(id, engineer, eRemote); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Rotate(this), true, "Rotate");
            AddModuleRunList(new Run_MainGrab(this), true, "Main Grab");
            AddModuleRunList(new Run_SideGrab(this), true, "Side Grab");
            AddModuleRunList(new Run_StainGrab(this), true, "Stain Grab");
        }        
        #endregion
    }
}
