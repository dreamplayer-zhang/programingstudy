using Root_Pine2.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Transfer : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_loaderPusher.GetTools(m_toolBox, this, bInit);
            m_buffer.GetTools(m_toolBox, this, bInit);
            m_gripper.GetTools(m_toolBox, this, bInit);
            m_pusher.GetTools(m_toolBox, this, bInit);
        }
        #endregion

        #region Loader Pusher
        public class LoaderPusher
        {
            #region ToolBox
            Axis[] m_axis = new Axis[2] { null, null };
            DIO_I2O[] m_dioPusher = new DIO_I2O[2] { null, null };
            DIO_I[] m_diOverload = new DIO_I[2] { null, null };
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis[0], module, "Loader Pusher 0");
                toolBox.GetAxis(ref m_axis[1], module, "Loader Pusher 1");
                toolBox.GetDIO(ref m_dioPusher[0], module, "Loader Pusher 0", "Backward", "Forward");
                toolBox.GetDIO(ref m_dioPusher[1], module, "Loader Pusher 1", "Backward", "Forward");
                toolBox.GetDIO(ref m_diOverload[0], module, "Loader Pusher 0 Overload");
                toolBox.GetDIO(ref m_diOverload[1], module, "Loader Pusher 1 Overload");
                if (bInit) InitPosition(); 
            }
            #endregion

            #region Axis
            string c_sReady = "Ready";
            void InitPosition()
            {
                m_axis[0].AddPos(c_sReady);
                m_axis[1].AddPos(c_sReady);
                foreach (InfoStrip.eMagazine ePos in Enum.GetValues(typeof(InfoStrip.eMagazine)))
                {
                    m_axis[GetAxisID(ePos)].AddPos(ePos.ToString()); 
                }
            }

            InfoStrip.eMagazine m_ePosLeft = InfoStrip.eMagazine.Magazine3;
            int GetAxisID(InfoStrip.eMagazine ePos)
            {
                return (ePos <= m_ePosLeft) ? 0 : 1; 
            }

            public string RunMoveReady(bool bWait = true)
            {
                if ((m_axis[0].p_posCommand == m_axis[0].GetPosValue(c_sReady)) && (m_axis[1].p_posCommand == m_axis[1].GetPosValue(c_sReady))) return "OK";
                m_axis[0].StartMove(c_sReady);
                m_axis[1].StartMove(c_sReady);
                if (bWait == false) return "OK";
                if (m_axis[0].WaitReady() != "OK") return "Move Ready Error";
                if (m_axis[1].WaitReady() != "OK") return "Move Ready Error";
                return "OK";
            }

            public string RunMove(InfoStrip.eMagazine ePos, bool bWait = true)
            {
                Axis axis = m_axis[GetAxisID(ePos)]; 
                axis.StartMove(ePos);
                return bWait ? axis.WaitReady() : "OK";
            }

            public void Reset()
            {
                RunMoveReady(false); 
            }
            #endregion

            #region Pusher
            public string RunPusher(InfoStrip.eMagazine eMagazine)
            {
                string sRun = RunMove(eMagazine);
                if (sRun != "OK") return sRun;
                DIO_I2O dioPusher = m_dioPusher[GetAxisID(eMagazine)]; 
                DIO_I diOverload = m_diOverload[GetAxisID(eMagazine)];
                try
                {
                    dioPusher.Write(true);
                    StopWatch sw = new StopWatch();
                    int msTimeout = (int)(1000 * dioPusher.m_secTimeout);
                    Thread.Sleep(100);
                    while (!dioPusher.p_bDone)
                    {
                        Thread.Sleep(10);
                        if (diOverload.p_bIn) return "Loader Pusher Overload Sensor Error";
                        if (sw.ElapsedMilliseconds > msTimeout) return "Run Loader Pusher Forward Timeout";
                    }
                    Thread.Sleep(100);
                    dioPusher.Write(false);
                    return dioPusher.WaitDone(); 
                }
                finally { dioPusher.Write(false); }
            }

            public bool IsPusherOff()
            {
                if (m_dioPusher[0].m_aBitDI[0].p_bOn == false) return false;
                if (m_dioPusher[1].m_aBitDI[0].p_bOn == false) return false;
                return true; 
            }
            #endregion
        }
        LoaderPusher m_loaderPusher = new LoaderPusher();
        #endregion

        #region Buffer
        public class Buffer
        {
            #region ToolBox
            Axis m_axis;
            Axis m_axisWidth;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Buffer");
                toolBox.GetAxis(ref m_axisWidth, module, "Width");
                if (bInit) InitPosition();
            }

            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(InfoStrip.eMagazine)));
                m_axisWidth.AddPos(Enum.GetNames(typeof(eWidth)));
                
            }

            public double GetXScale(InfoStrip.eMagazine eMagazine)
            {
                double p0 = m_axis.GetPosValue(InfoStrip.eMagazine.Magazine0);
                double p7 = m_axis.GetPosValue(InfoStrip.eMagazine.Magazine7); 
                return (m_axis.GetPosValue(eMagazine) - p0)  / (p7 - p0); 
            }
            #endregion

            #region Axis
            double m_dxPulse = 0;
            public InfoStrip.eMagazine m_ePosDst = InfoStrip.eMagazine.Magazine0;
            public double m_xOffset = 0; 
            public string RunMove(InfoStrip.eMagazine ePos, double xOffset, bool bPushPos, bool bWait = true)
            {
                if (m_transfer.m_pusher.p_bLock) return "Lock by Sorter Picker";
                if (m_transfer.m_gripper.p_bLock) return "Lock by Loader Picker";
                m_transfer.m_pusher.p_bEnable = false;
                m_transfer.m_gripper.p_bEnable = false; 
                m_ePosDst = ePos;
                double dPos = 1000 * (m_transfer.m_pine2.m_widthDefaultStrip - m_transfer.m_pine2.p_widthStrip) / 2;
                m_xOffset = (bPushPos ? m_dxPulse : 0) + dPos + xOffset;
                foreach (MagazineEV magazineEV in m_transfer.m_magazineEV.m_aEV.Values) magazineEV.m_conveyor.m_bInv = false;
                m_transfer.m_magazineEV.m_aEV[ePos].m_conveyor.m_bInv = true; 
                m_axis.StartMove(ePos, m_xOffset); 
                return bWait ? m_axis.WaitReady() : "OK";
            }
            #endregion

            #region Width
            public enum eWidth
            {
                mm75,
                mm95,
            }
            public string RunWidth(double fWidth, bool bWait = true)
            {
                double f75 = m_axisWidth.GetPosValue(eWidth.mm75);
                double f95 = m_axisWidth.GetPosValue(eWidth.mm95);
                double dPos = (f95 - f75) * (fWidth - 75) / 20 + f75;
                m_axisWidth.StartMove(dPos);
                return bWait ? m_axisWidth.WaitReady() : "OK";
            }
            #endregion

            public void Reset(Pine2 pine)
            {
                RunWidth(pine.p_widthStrip);
            }

            public void RunTree(Tree tree)
            {
                m_dxPulse = tree.Set(m_dxPulse, m_dxPulse, "dPulse", "Distance between Buffer (pulse)");
            }

            Transfer m_transfer; 
            public Buffer(Transfer transfer)
            {
                m_transfer = transfer; 
            }
        }
        public Buffer m_buffer;
        #endregion

        #region Gripper
        public class Gripper : NotifyProperty
        {
            Axis m_axis;
            DIO_I2O m_dioGripper;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Gripper");
                toolBox.GetDIO(ref m_dioGripper, module, "Gripper", "Ungrip", "Grip");
                toolBox.GetDIO(ref m_diCheck, module, "Gripper Check", new string[] { "1", "2" });
                if (bInit) m_axis.AddPos(Enum.GetNames(typeof(eGripper)));
            }

            public enum eGripper
            {
                Ready,
                Ungrip,
                Grip
            }
            public string RunMoveGripper(eGripper eGripper, bool bWait = true)
            {
                m_axis.StartMove(eGripper);
                return bWait ? m_axis.WaitReady() : "OK";
            }

            public string RunGripperReady(eGripper eGripper)
            {
                m_dioGripper.Write(false);
                return RunMoveGripper(eGripper, false);
            }

            public string RunGripper()
            {
                if (Run(RunGripper(false))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Grip))) return m_sRun;
                if (Run(RunGripper(true))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Ungrip))) return m_sRun;
                if (Run(RunGripper(false))) return m_sRun;
                if (Run(RunMoveGripper(eGripper.Ready))) return m_sRun;
                return "OK";
            }

            string RunGripper(bool bGrip, bool bWait = true)
            {
                m_dioGripper.Write(bGrip);
                return bWait ? m_dioGripper.WaitDone() : "OK";
            }

            string m_sRun = "OK";
            bool Run(string sRun)
            {
                m_sRun = sRun;
                return sRun != "OK";
            }

            public bool IsExist()
            {
                return m_diCheck.ReadDI(0) || m_diCheck.ReadDI(1);
            }

            public bool IsGripped()
            {
                return m_diCheck.ReadDI(0) && m_diCheck.ReadDI(1);
            }

            public bool IsPushed()
            {
                return m_diCheck.ReadDI(0);
            }

            public InfoStrip p_infoStrip { get; set; }
            public void Reset()
            {
                p_bEnable = false;
                p_bLock = false;
                if (p_infoStrip == null) return;
                if (IsExist() == false) p_infoStrip = null;
            }

            bool _bEnable = false;
            public bool p_bEnable
            {
                get { return _bEnable; }
                set
                {
                    if (_bEnable == value) return;
                    _bEnable = value;
                    OnPropertyChanged();
                }
            }

            bool _bLock = false;
            public bool p_bLock
            {
                get { return _bLock; }
                set
                {
                    if (_bLock == value) return;
                    _bLock = value;
                    OnPropertyChanged();
                }
            }

            public string WaitUnlock()
            {
                while (p_bLock)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                return "OK";
            }

            public Gripper()
            {
                p_infoStrip = null;
            }
        }
        public Gripper m_gripper = new Gripper();
        #endregion

        #region Pusher
        public class Pusher : NotifyProperty
        {
            public DIO_I2O m_dioPusher;
            DIO_I m_diOverload;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, Transfer module, bool bInit)
            {
                toolBox.GetDIO(ref m_dioPusher, module, "Pusher", "Backward", "Forward");
                toolBox.GetDIO(ref m_diOverload, module, "Pusher Overload");
                toolBox.GetDIO(ref m_diCheck, module, "Pusher Check", new string[] { "1", "2" });
            }

            public string RunPusher()
            {
                try
                {
                    m_dioPusher.Write(true);
                    StopWatch sw = new StopWatch();
                    int msTimeout = (int)(1000 * m_dioPusher.m_secTimeout);
                    Thread.Sleep(100);
                    while (!m_dioPusher.p_bDone)
                    {
                        Thread.Sleep(10);
                        if (m_diOverload.p_bIn) return "Pusher Overload Sensor Error";
                        if (sw.ElapsedMilliseconds > msTimeout) return "Run Pusher Forward Timeout";
                    }
                    Thread.Sleep(100);
                    m_dioPusher.Write(false);
                    return m_dioPusher.WaitDone();
                }
                finally { m_dioPusher.Write(false); }
            }

            public bool IsExist()
            {
                return m_diCheck.ReadDI(0) || m_diCheck.ReadDI(1);
            }

            public bool IsPlaced()
            {
                return m_diCheck.ReadDI(0) && m_diCheck.ReadDI(1);
            }

            public InfoStrip p_infoStrip { get; set; }

            public void Reset()
            {
                p_bEnable = false;
                p_bLock = false;
                if (p_infoStrip == null) return;
                if (IsExist() == false) p_infoStrip = null;
            }

            bool _bEnable = false;
            public bool p_bEnable
            {
                get { return _bEnable; }
                set
                {
                    if (_bEnable == value) return;
                    _bEnable = value;
                    OnPropertyChanged();
                }
            }

            bool _bLock = false;
            public bool p_bLock
            {
                get { return _bLock; }
                set
                {
                    if (_bLock == value) return;
                    _bLock = value;
                    OnPropertyChanged();
                }
            }

            public string WaitUnlock()
            {
                while (p_bLock)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                return "OK";
            }

            public Pusher()
            {
                p_infoStrip = null;
            }
        }
        public Pusher m_pusher = new Pusher();
        #endregion

        #region RunLoad
        public string StartLoad()
        {
            return StartRun(m_runLoad.Clone()); 
        }

        public string RunLoad()
        {
            InfoStrip infoStrip = m_magazineEV.GetInfoStrip(true);
            if (infoStrip == null)
            {
                m_pusher.p_bEnable = (m_pusher.p_infoStrip == null);
                Thread.Sleep(2000);
                return m_pusher.WaitUnlock();
            }
            double xOffset = m_magazineEV.CalcXOffset(infoStrip);
            if (Run(m_loaderPusher.RunMove(infoStrip.p_eMagazine, false))) return p_sInfo;
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, xOffset, false, false))) return p_sInfo;
            if (Run(m_magazineEV.RunMove(infoStrip))) return p_sInfo;
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, xOffset, false, true))) return p_sInfo;
            m_pusher.p_bEnable = (m_pusher.p_infoStrip == null);
            if (Run(m_gripper.RunGripperReady(Gripper.eGripper.Grip))) return p_sInfo;
            if (Run(m_loaderPusher.RunPusher(infoStrip.p_eMagazine))) return p_sInfo;
            if (m_gripper.IsPushed())
            {
                if (Run(m_gripper.RunGripper())) return p_sInfo;
                infoStrip = m_magazineEV.GetInfoStrip(false);
                if (m_gripper.IsGripped()) m_gripper.p_infoStrip = infoStrip;
                else return "Check Strip in Gripper";
            }
            else
            {
                m_magazineEV.GetInfoStrip(false);
                infoStrip.Dispose();
            }
            return m_pusher.WaitUnlock();
        }
        #endregion

        #region RunWaitLoader
        public string StartWaitLoader()
        {
            return StartRun(m_runWaitLoader.Clone());
        }

        public string RunWaitLoader()
        {
            m_gripper.p_bEnable = (m_gripper.p_infoStrip != null);
            m_pusher.p_bEnable = (m_pusher.p_infoStrip == null); 
            Thread.Sleep(2000);
            if (Run(m_gripper.WaitUnlock())) return p_sInfo;
            if (Run(m_pusher.WaitUnlock())) return p_sInfo;
            return "OK";
        }
        #endregion

        #region RunUnload
        public string StartUnload()
        {
            return StartRun(m_runUnload.Clone()); 
        }

        public string RunUnload()
        {
            InfoStrip infoStrip = m_pusher.p_infoStrip; 
            if (infoStrip == null) return "OK";
            if (m_pusher.IsPlaced() == false) return "Check Strip in Pusher"; 
            double xOffset = m_magazineEV.CalcXOffset(infoStrip);
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, xOffset, true, false))) return p_sInfo;
            if (Run(m_magazineEV.RunMove(infoStrip))) return p_sInfo;
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, xOffset, true, true))) return p_sInfo;
            m_gripper.p_bEnable = (m_gripper.p_infoStrip != null);
            if (Run(m_pusher.RunPusher())) return p_sInfo;
            if (m_pusher.IsExist()) return "Strip Exist in Pusher after Push"; 
            ((Pine2_Handler)m_engineer.ClassHandler()).SendSortInfo(infoStrip); 
            m_magazineEV.PutInfoStrip(infoStrip);
            m_pusher.p_infoStrip = null;
            if (Run(m_gripper.WaitUnlock())) return p_sInfo; 
            m_engineer.ClassHandler().CheckFinish();
            return "OK"; 
        }
        #endregion

        #region Pusher Safe
        public string IsPusherOff()
        {
            if (m_loaderPusher.IsPusherOff() == false) return "Check Loader Pusher";
            if (m_pusher.m_dioPusher.m_aBitDI[0].p_bOn == false) return "Check Transfer Pusher";
            return "OK"; 
        }
        #endregion

        #region override
        public override string StateReady()
        {
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack:
                    m_loaderPusher.RunMoveReady(); 
                    break;
                case Pine2.eRunMode.Magazine:
                    if (m_pusher.p_infoStrip != null) return StartUnload();
                    if (m_gripper.p_infoStrip == null) return StartLoad();
                    return StartWaitLoader();
            }
            return "OK"; 
        }

        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            if (m_gripper.IsExist() || m_pusher.IsExist())
            {
                p_eState = eState.Init;
                return "Check Strip Sensor";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            m_loaderPusher.Reset();
            m_buffer.Reset(m_pine2);
            return p_sInfo;
        }

        public override void Reset()
        {
            m_loaderPusher.Reset();
            m_buffer.Reset(m_pine2);
            m_gripper.Reset();
            m_pusher.Reset();
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_buffer.RunTree(tree.GetTree("Buffer"));
        }
        #endregion

        Pine2 m_pine2;
        MagazineEVSet m_magazineEV; 
        public Transfer(string id, IEngineer engineer, Pine2 pine2, MagazineEVSet magazineEV)
        {
            m_pine2 = pine2;
            m_magazineEV = magazineEV;
            m_buffer = new Buffer(this); 
            InitBase(id, engineer); 
        }

        public override void ThreadStop()
        {
            Reset(); 
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnload;
        ModuleRunBase m_runWaitLoader; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_ChangeWidth(this), false, "Run Change Width");
            AddModuleRunList(new Run_LoaderPusher(this), false, "Run Load Pusher");
            AddModuleRunList(new Run_MoveTranfer(this), false, "Run MoveTray");
            AddModuleRunList(new Run_Grip(this), false, "Run Grip Strip");
            AddModuleRunList(new Run_Pusher(this), false, "Run Push Strip");
            m_runLoad = AddModuleRunList(new Run_RunLoad(this), false, "Run Load to Transfer");
            m_runUnload = AddModuleRunList(new Run_RunUnload(this), false, "Run Unload to Magazine");
            m_runWaitLoader = AddModuleRunList(new Run_WaitLoader(this), false, "Wait Loader");
        }

        public class Run_ChangeWidth : ModuleRunBase
        {
            Transfer m_module;
            public Run_ChangeWidth(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fWidth = 77;
            public override ModuleRunBase Clone()
            {
                Run_ChangeWidth run = new Run_ChangeWidth(m_module);
                run.m_fWidth = m_fWidth;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fWidth = tree.Set(m_fWidth, m_fWidth, "Width", "Strip Width", bVisible);
            }

            public override string Run()
            {
                return m_module.m_buffer.RunWidth(m_fWidth); 
            }
        }

        public class Run_LoaderPusher : ModuleRunBase
        {
            Transfer m_module;
            public Run_LoaderPusher(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            InfoStrip.eMagazine m_eMagazine = InfoStrip.eMagazine.Magazine0;
            bool m_bPush = false; 
            public override ModuleRunBase Clone()
            {
                Run_LoaderPusher run = new Run_LoaderPusher(m_module);
                run.m_eMagazine = m_eMagazine;
                run.m_bPush = m_bPush; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eMagazine = (InfoStrip.eMagazine)tree.Set(m_eMagazine, m_eMagazine, "MagazineEV", "MagazineEV", bVisible);
                m_bPush = tree.Set(m_bPush, m_bPush, "Push", "Run Pusher", bVisible); 
            }

            public override string Run()
            {
                if (m_bPush) return m_module.m_loaderPusher.RunPusher(m_eMagazine);
                else return m_module.m_loaderPusher.RunMove(m_eMagazine); 
            }
        }

        public class Run_MoveTranfer : ModuleRunBase
        {
            Transfer m_module;
            public Run_MoveTranfer(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            InfoStrip.eMagazine m_eMagazine = InfoStrip.eMagazine.Magazine0;
            bool m_bPushPos = false;
            public override ModuleRunBase Clone()
            {
                Run_MoveTranfer run = new Run_MoveTranfer(m_module);
                run.m_eMagazine = m_eMagazine;
                run.m_bPushPos = m_bPushPos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eMagazine = (InfoStrip.eMagazine)tree.Set(m_eMagazine, m_eMagazine, "MagazineEV", "MagazineEV", bVisible);
                m_bPushPos = tree.Set(m_bPushPos, m_bPushPos, "Pusher Pos", "Pusher Position", bVisible);
            }

            public override string Run()
            {
                double xOffset = m_module.m_magazineEV.m_aEV[m_eMagazine].CalcXOffset(); 
                return m_module.m_buffer.RunMove(m_eMagazine, xOffset, m_bPushPos);
            }
        }

        public class Run_Grip : ModuleRunBase
        {
            Transfer m_module;
            public Run_Grip(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Grip run = new Run_Grip(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.m_gripper.RunGripper(); 
            }
        }

        public class Run_Pusher : ModuleRunBase
        {
            Transfer m_module;
            public Run_Pusher(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Pusher run = new Run_Pusher(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.m_pusher.RunPusher(); 
            }
        }

        public class Run_RunLoad : ModuleRunBase
        {
            Transfer m_module;
            public Run_RunLoad(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_RunLoad run = new Run_RunLoad(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunLoad();
            }
        }

        public class Run_RunUnload : ModuleRunBase
        {
            Transfer m_module;
            public Run_RunUnload(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_RunUnload run = new Run_RunUnload(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunUnload();
            }
        }

        public class Run_WaitLoader : ModuleRunBase
        {
            Transfer m_module;
            public Run_WaitLoader(Transfer module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_WaitLoader run = new Run_WaitLoader(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunWaitLoader();
            }
        }
        #endregion
    }
}
