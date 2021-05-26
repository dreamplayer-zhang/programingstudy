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
            void InitPosition()
            {
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

            public string RunMove(InfoStrip.eMagazine ePos, bool bWait = true)
            {
                Axis axis = m_axis[GetAxisID(ePos)]; 
                axis.StartMove(ePos);
                return bWait ? axis.WaitReady() : "OK";
            }

            public void Reset()
            {
                m_axis[0].StartMove(0);
                m_axis[1].StartMove(0);
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
                m_gripper.GetTools(toolBox, module, bInit);
                m_pusher.GetTools(toolBox, module, bInit);
                if (bInit) InitPosition();
            }

            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(InfoStrip.eMagazine)));
                m_axisWidth.AddPos(Enum.GetNames(typeof(eWidth)));
                
            }
            #endregion

            #region Axis
            double m_dPulse = 0;
            public InfoStrip.eMagazine m_ePosDst = InfoStrip.eMagazine.Magazine0; 
            public string RunMove(InfoStrip.eMagazine ePos, bool bGripPos, bool bWait = true)
            {
                if (m_pusher.p_bLock) return "Lock by Sorter Picker";
                if (m_gripper.p_bLock) return "Lock by Loader Picker";
                m_ePosDst = ePos; 
                m_axis.StartMove(ePos, bGripPos ? 0 : m_dPulse); 
                return bWait ? m_axis.WaitReady() : "OK";
            }
            #endregion

            #region Width
            public enum eWidth
            {
                mm70,
                mm90,
            }
            public string RunWidth(double fWidth, bool bWait = true)
            {
                double f70 = m_axisWidth.GetPosValue(eWidth.mm70);
                double f90 = m_axisWidth.GetPosValue(eWidth.mm90);
                double dPos = (f90 - f70) * (fWidth - 70) / 10 + f70;
                m_axisWidth.StartMove(dPos);
                return bWait ? m_axisWidth.WaitReady() : "OK";
            }
            #endregion

            public void Reset(Pine2 pine)
            {
                m_gripper.Reset();
                m_pusher.Reset();
                RunWidth(pine.p_widthStrip);
            }

            public void RunTree(Tree tree)
            {
                m_dPulse = tree.Set(m_dPulse, m_dPulse, "dPulse", "Distance between Buffer (pulse)"); 
            }

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
                DIO_I2O m_dioPusher;
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
        }
        public Buffer m_buffer = new Buffer();
        #endregion

        #region RunLoad
        public string RunLoad()
        {
            InfoStrip infoStrip = m_magazineEV.GetInfoStrip(true);
            if (infoStrip == null) return "";
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, true, false))) return p_sInfo;
            if (Run(m_magazineEV.RunMove(infoStrip))) return p_sInfo;
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, true, true))) return p_sInfo;
            m_buffer.m_pusher.p_bEnable = true; 
            try
            {
                if (Run(m_buffer.m_gripper.RunGripperReady(Buffer.Gripper.eGripper.Grip))) return p_sInfo;
                if (Run(m_loaderPusher.RunPusher(infoStrip.p_eMagazine))) return p_sInfo;
                if (Run(m_buffer.m_gripper.RunGripper())) return p_sInfo;
                infoStrip = m_magazineEV.GetInfoStrip(false);
                if (m_buffer.m_gripper.IsExist()) m_buffer.m_gripper.p_infoStrip = infoStrip;
                else infoStrip.Dispose(); 
                return m_buffer.m_pusher.WaitUnlock();
            }
            finally
            {
                m_buffer.m_gripper.RunGripperReady(Buffer.Gripper.eGripper.Ready); 
            }
        }
        #endregion

        #region RunUnload
        public string RunUnload()
        {
            InfoStrip infoStrip = m_buffer.m_pusher.p_infoStrip; 
            if (infoStrip == null) return "";
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, true, false))) return p_sInfo;
            if (Run(m_magazineEV.RunMove(infoStrip))) return p_sInfo;
            if (Run(m_buffer.RunMove(infoStrip.p_eMagazine, true, true))) return p_sInfo;
            m_buffer.m_gripper.p_bEnable = true;
            if (Run(m_buffer.m_pusher.RunPusher())) return p_sInfo;
            m_magazineEV.PutInfoStrip(infoStrip);
            m_buffer.m_pusher.p_infoStrip = null; 
            return m_buffer.m_gripper.WaitUnlock(); 
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            m_loaderPusher.Reset();
            m_buffer.Reset(m_pine2); 
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
            base.InitBase(id, engineer); 
        }

        public override void ThreadStop()
        {
            Reset(); 
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_ChangeWidth(this), false, "Run Change Width");
            AddModuleRunList(new Run_LoaderPusher(this), false, "Run Load Pusher");
            AddModuleRunList(new Run_Grip(this), false, "Run Grip Strip");
            AddModuleRunList(new Run_Pusher(this), false, "Run Push Strip");
            AddModuleRunList(new Run_RunLoad(this), false, "Run Load to Transfer");
            AddModuleRunList(new Run_RunUnload(this), false, "Run Unload to Magazine");
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
                return m_module.m_buffer.m_gripper.RunGripper(); 
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
                return m_module.m_buffer.m_pusher.RunPusher(); 
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
        #endregion
    }
}
