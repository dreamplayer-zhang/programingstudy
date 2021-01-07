using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class TapePacker : ModuleBase, IWTRChild
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_cartridge.GetTools(m_toolBox, bInit);
            m_roller.GetTools(m_toolBox, bInit);
            m_head.GetTools(m_toolBox, bInit);
            m_stage.GetTools(m_toolBox, bInit); 
        }
        #endregion

        #region Solvalue
        public class Solvalue
        {
            public DIO_I2O2 m_dio;
            public double m_secTimeout = 5;

            public void GetTools(ToolBox toolBox, string sOff, string sOn, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_dio, m_packer, m_id, sOff, sOn);
                if (bInit) m_dio.Write(false); 
            }

            public string RunSol(bool bOn, double secSleep = 0)
            {
                m_dio.Write(bOn);
                Thread.Sleep(500);
                string sWait = m_dio.WaitDone(m_secTimeout);
                if (sWait != "OK") return sWait; 
                Thread.Sleep((int)(1000 * secSleep));
                return "OK"; 
            }

            public void RunTree(Tree tree)
            {
                m_secTimeout = tree.Set(m_secTimeout, m_secTimeout, m_id, "Solvalue Move Timeout (sec)"); 
            }

            TapePacker m_packer;
            public string m_id;
            public Solvalue(string id, TapePacker packer, double secTimeout)
            {
                m_id = id; 
                m_packer = packer;
                m_secTimeout = secTimeout; 
            }
        }

        public List<Solvalue> m_aSolvalve = new List<Solvalue>(); 
        Solvalue GetNewSolvalve(string id, double secTimeout)
        {
            Solvalue sol = new Solvalue(id, this, secTimeout);
            m_aSolvalve.Add(sol);
            return sol; 
        }

        public List<string> p_asSol
        {
            get
            {
                List<string> asSol = new List<string>();
                foreach (Solvalue sol in m_aSolvalve) asSol.Add(sol.m_id);
                return asSol; 
            }
        }

        public Solvalue GetSolvalve(string sSol)
        {
            foreach (Solvalue sol in m_aSolvalve)
            {
                if (sol.m_id == sSol) return sol; 
            }
            return null; 
        }
        #endregion

        #region Cartridge
        public class Cartridge
        {
            Axis m_axis;
            Solvalue m_solCutter;
            DIO_I[] m_diCheck = new DIO_I[2];
            Solvalue m_solLock;
            Solvalue m_solStop;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axis, m_packer, m_id);
                m_solCutter.GetTools(toolBox, "Backward", "Forward", bInit);
                m_packer.p_sInfo = toolBox.Get(ref m_diCheck[0], m_packer, "Check0");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheck[1], m_packer, "Check1");
                m_solLock.GetTools(toolBox, "Unlock", "Lock", bInit);
                m_solStop.GetTools(toolBox, "Rotate", "Stop", bInit);
                if (bInit)
                {
                    foreach (Solvalue sol in m_aSolvalve) sol.m_dio.Write(false); 
                    InitPos(); 
                }
            }

            List<Solvalue> m_aSolvalve = new List<Solvalue>(); 
            void InitSolvalve()
            {
                m_solCutter = m_packer.GetNewSolvalve(m_id + ".Cutter", 5);
                m_aSolvalve.Add(m_solCutter);
                m_solLock = m_packer.GetNewSolvalve(m_id + ".Lock", 5);
                m_aSolvalve.Add(m_solLock);
                m_solStop = m_packer.GetNewSolvalve(m_id + ".Stop", 5);
                m_aSolvalve.Add(m_solStop);
            }

            public enum ePos
            {
                Refill,
                Ready,
                Run
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos))); 
            }

            public string RunMove(ePos ePos)
            {
                m_axis.StartMove(ePos);
                return m_axis.WaitReady(); 
            }

            public string RunCutter()
            {
                if (m_packer.Run(m_solCutter.RunSol(true))) return m_packer.p_sInfo; 
                Thread.Sleep(1000);
                return m_solCutter.RunSol(false);
            }

            public string RunLock(bool bLock)
            {
                if (bLock && IsCheck() == false) return "Check Sensor not Detected"; 
                return m_solLock.RunSol(bLock); 
            }

            public string RunStop(bool bStop)
            {
                return m_solStop.RunSol(bStop); 
            }

            public bool IsCheck()
            {
                return m_diCheck[0].p_bIn && m_diCheck[1].p_bIn; 
            }

            public void RunTree(Tree tree)
            {
                foreach (Solvalue sol in m_aSolvalve) sol.RunTree(tree); 
            }

            string m_id;
            TapePacker m_packer; 
            public Cartridge(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
                InitSolvalve(); 
            }
        }

        public Cartridge m_cartridge; 
        #endregion

        #region Roller
        public class Roller
        {
            Solvalue m_solUp;
            Solvalue m_solPush;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_solUp.GetTools(toolBox, "Down", "Up", bInit);
                m_solPush.GetTools(toolBox, "Back", "Push", bInit);
                if (bInit)
                {
                    RunPush(false); 
                }
            }

            List<Solvalue> m_aSolvalve = new List<Solvalue>();
            void InitSolvalve()
            {
                m_solUp = m_packer.GetNewSolvalve(m_id + ".Down", 5);
                m_aSolvalve.Add(m_solUp);
                m_solPush = m_packer.GetNewSolvalve(m_id + ".Push", 5);
                m_aSolvalve.Add(m_solPush);
            }

            public string RunPush(bool bPush)
            {
                if (m_packer.Run(m_solPush.RunSol(false))) return m_packer.p_sInfo;
                if (bPush)
                {
                    if (m_packer.Run(m_solUp.RunSol(true))) return m_packer.p_sInfo;
                    return m_solPush.RunSol(true, 1);
                }
                else
                {
                    return m_solUp.RunSol(false); 
                }
            }

            public void RunTree(Tree tree)
            {
                foreach (Solvalue sol in m_aSolvalve) sol.RunTree(tree);
            }

            string m_id;
            TapePacker m_packer;
            public Roller(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
                InitSolvalve(); 
            }
        }
        public Roller m_roller; 
        #endregion

        #region Head
        public class Head
        {
            Solvalue m_solHead;
            Solvalue m_solPicker;
            DIO_I m_diOverload;
            DIO_I m_diCheckCover;
            DIO_IO m_dioVacuum;
            DIO_O m_doBlow; 
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_solHead.GetTools(toolBox, "Up", "Down", bInit);
                m_solPicker.GetTools(toolBox, "Up", "Down", bInit);
                m_packer.p_sInfo = toolBox.Get(ref m_diOverload, m_packer, "Overload");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheckCover, m_packer, "Check Cover");
                m_packer.p_sInfo = toolBox.Get(ref m_dioVacuum, m_packer, "Vacuum");
                m_packer.p_sInfo = toolBox.Get(ref m_doBlow, m_packer, "Blow");
                if (bInit)
                {
                    foreach (Solvalue sol in m_aSolvalve) sol.m_dio.Write(false);
                }
            }

            List<Solvalue> m_aSolvalve = new List<Solvalue>();
            void InitSolvalve()
            {
                m_solHead = m_packer.GetNewSolvalve(m_id + ".Head", 5);
                m_aSolvalve.Add(m_solHead);
                m_solPicker = m_packer.GetNewSolvalve(m_id + ".Picker", 5);
                m_aSolvalve.Add(m_solPicker);
            }

            public string Run(bool bHeadDown, bool bPickerDown)
            {
                m_solPicker.m_dio.Write(bPickerDown);
                string sRun = RunHead(bHeadDown);
                if (sRun != "OK")
                {
                    m_solPicker.m_dio.Write(false);
                    RunHead(false); 
                    return sRun;
                }
                return m_solPicker.m_dio.WaitDone(m_solPicker.m_secTimeout); 
            }

            string RunHead(bool bDown)
            {
                if (bDown == false) return m_solHead.RunSol(bDown);
                DIO_I2O2 dio = m_solHead.m_dio; 
                dio.Write(bDown);
                Thread.Sleep(100);
                int msWait = (int)(1000 * m_solHead.m_secTimeout);
                while (dio.p_bDone != true)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return m_id + " EQ Stop";
                    if (m_diOverload.p_bIn)
                    {
                        dio.Write(false);
                        return "Overload Sensor Checked"; 
                    }
                    if (dio.m_swWrite.ElapsedMilliseconds > msWait) return dio.m_id + " Solvalve Move Timeout";
                }
                return "OK";
            }

            double m_secVac = 0.5; 
            double m_secBlow = 0.5; 
            public string RunVacuum(bool bOn)
            {
                m_dioVacuum.Write(bOn);
                if (bOn == false)
                {
                    m_doBlow.Write(true);
                    Thread.Sleep((int)(1000 * m_secBlow));
                    m_doBlow.Write(false);
                    return "OK";
                }
                int msVac = (int)(1000 * m_secVac);
                while (m_dioVacuum.p_bIn != bOn)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return m_id + " EQ Stop";
                    if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
                }
                return "OK";
            }

            public void RunTree(Tree tree)
            {
                m_secVac = tree.Set(m_secVac, m_secVac, "Vacuum", "Vacuum On Wait (sec)");
                m_secBlow = tree.Set(m_secBlow, m_secBlow, "Blow", "Vacuum Off Blow Time (sec)");
                foreach (Solvalue sol in m_aSolvalve) sol.RunTree(tree);
            }

            string m_id;
            TapePacker m_packer;
            public Head(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
                InitSolvalve(); 
            }
        }

        public Head m_head; 
        #endregion

        #region Stage
        public class Stage
        {
            public Axis m_axis;
            DIO_I m_diCheck;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                m_packer.p_sInfo = toolBox.Get(ref m_axis, m_packer, "Rotate");
                m_packer.p_sInfo = toolBox.Get(ref m_diCheck, m_packer, "Check");
                if (bInit) { }
            }

            public bool IsCheck()
            {
                return m_diCheck.p_bIn; 
            }

            const double c_fPpR = 2621440;
            public double m_degReady = 0;
            public double m_degAttach = 13; 
            public string RunMove(double fDeg)
            {
                double fNow = m_axis.p_posCommand; 
                double fPulse = fDeg * c_fPpR / 360;
                while ((fPulse - fNow) > c_fPpR) fNow += c_fPpR;
                while ((fNow - fPulse) > c_fPpR) fNow -= c_fPpR; 
                if (Math.Abs(fPulse - fNow) > c_fPpR / 2)
                {
                    if (fPulse > fNow) fNow += c_fPpR;
                    else fNow -= c_fPpR; 
                }
                m_axis.SetCommandPosition(fNow);
                m_axis.StartMove(fPulse);
                return m_axis.WaitReady();
            }

            int m_nRound = 2;
            double m_degCut = 10; 
            double m_vDeg = 90;
            double m_secAcc = 0.7; 
            public string RunTaping()
            {
                double fPulse = m_degCut * c_fPpR / 360;
                double vPulse = m_vDeg * c_fPpR / 360;
                m_axis.StartMove(fPulse + c_fPpR * m_nRound, vPulse, m_secAcc, m_secAcc);
                string sRun = m_axis.WaitReady();
                m_axis.SetCommandPosition(fPulse);
                return sRun; 
            }

            public string RunRotate(double fDeg)
            {
                double fPulse = fDeg * c_fPpR / 360;
                double vPulse = m_vDeg * c_fPpR / 360;
                double fNow = m_axis.p_posCommand;
                m_axis.StartMove(fPulse + fNow, vPulse, m_secAcc, m_secAcc);
                return m_axis.WaitReady();
            }

            public void RunTree(Tree tree)
            {
                m_degReady = tree.Set(m_degReady, m_degReady, "Ready", "Ready Position (Deg)");
                m_degAttach = tree.Set(m_degAttach, m_degAttach, "Attach", "Attach Position (Deg)");
                m_nRound = tree.Set(m_nRound, m_nRound, "Round", "Taping Rotate Round");
                m_degCut = tree.Set(m_degCut, m_degCut, "Cut", "Cut Position (Deg)");
                m_vDeg = tree.Set(m_vDeg, m_vDeg, "Speed", "Taping Rotate Speed (Deg / sec)");
                m_secAcc = tree.Set(m_secAcc, m_secAcc, "Acc", "Taping Rotate Acceleration Time (sec)");
            }

            public string BeforeHome()
            {
                m_axis.ServoOn(true);
                Thread.Sleep(100);
                m_axis.Jog(0.3);
                Thread.Sleep(1200);
                m_axis.StopAxis();
                Thread.Sleep(100);
                return "OK"; 
            }

            string m_id;
            TapePacker m_packer;
            public Stage(string id, TapePacker packer)
            {
                m_id = id;
                m_packer = packer;
            }
        }

        public Stage m_stage;
        #endregion

        #region Process State
        public enum eProcess
        {
            Empty,
            Case,
            Opening,
            Opened,
            Reticle,
            Packing,
            Done
        }
        eProcess _eProcess = eProcess.Empty; 
        public eProcess p_eProcess
        {
            get { return _eProcess; }
            set
            {
                if (_eProcess == value) return;
                _eProcess = value;
                if (m_reg != null) m_reg.Write("Process", (int)value);
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region InfoWafer
        string m_sInfoWafer = "";
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get { return _infoWafer; }
            set
            {
                m_sInfoWafer = (value == null) ? "" : value.p_id;
                _infoWafer = value;
                if (m_reg != null) m_reg.Write("sInfoWafer", m_sInfoWafer);
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadInfoWafer_Registry()
        {
            m_reg = new Registry(p_id + ".InfoWafer");
            m_sInfoWafer = m_reg.Read("sInfoWafer", m_sInfoWafer);
            p_eProcess = (eProcess)m_reg.Read("Process", (int)p_eProcess); 
            p_infoWafer = m_engineer.ClassHandler().GetGemSlot(m_sInfoWafer);
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get { return _bLock; }
            set
            {
                if (_bLock == value) return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false) return false;
                Thread.Sleep(100);
            }
            return true;
        }

        List<string> _asChildSlot = new List<string>();
        void InitChildSlot()
        {
            _asChildSlot.Add("Case");
            _asChildSlot.Add("Reticle");
        }
        public List<string> p_asChildSlot { get { return _asChildSlot; } }

        public InfoWafer GetInfoWafer(int nID)
        {
            return p_infoWafer;
        }

        public void SetInfoWafer(int nID, InfoWafer infoWafer)
        {
            p_infoWafer = infoWafer;
        }

        public string IsGetOK(int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer == null) return p_id + " IsGetOK - InfoWafer not Exist";
            switch (p_eProcess)
            {
                case eProcess.Case: if (nID == 0) return "OK"; break;
                case eProcess.Reticle: if (nID == 1) return "OK"; break;
                case eProcess.Done: if (nID == 0) return "OK"; break;
            }
            return p_id + " IsGetOK - Process " + p_eProcess.ToString();
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_infoWafer != null) return p_id + " IsPutOK - InfoWafer Exist";
            switch (p_eProcess)
            {
                case eProcess.Empty: if (nID == 0) return "OK"; break;
                case eProcess.Opened: if (nID == 1) return "OK"; break; 
            }
            return p_id + " IsPutOK - Process " + p_eProcess.ToString();
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            //if (p_infoWafer == null) return m_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null) return p_id + " BeforePut : InfoWafer != null";
            return CheckGetPut();
        }

        public string AfterGet(int nID)
        {
            switch (p_eProcess)
            {
                case eProcess.Case: if (nID == 0) p_eProcess = eProcess.Empty; break;
                case eProcess.Done: if (nID == 0) p_eProcess = eProcess.Empty; break;
                case eProcess.Reticle: if (nID == 1) p_eProcess = eProcess.Opened; break; 
            }
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            switch (p_eProcess)
            {
                case eProcess.Empty: if (nID == 0) p_eProcess = eProcess.Case; break;
                case eProcess.Opened: if (nID == 1) p_eProcess = eProcess.Reticle; break; 
            }
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return "OK";
        }

        public bool IsWaferExist(int nID)
        {
            return (p_infoWafer != null);
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
        }
        #endregion

        #region Functions
        public string RunCoverOpen()
        {
            if (p_eProcess != eProcess.Case) return "Process not Case : " + p_eProcess.ToString();
            p_eProcess = eProcess.Opening;
            if (Run(m_head.RunVacuum(false))) return p_sInfo;
            if (Run(m_head.Run(false, false))) return p_sInfo;
            if (Run(m_head.Run(true, true))) return p_sInfo;
            if (Run(m_head.RunVacuum(true))) return p_sInfo;
            if (Run(m_head.Run(false, true))) return p_sInfo;
            p_eProcess = eProcess.Opened; 
            return "OK";
        }

        public string RunCoverClose()
        {
            if (Run(m_head.Run(true, true))) return p_sInfo;
            if (Run(m_head.RunVacuum(false))) return p_sInfo;
            if (Run(m_head.Run(true, false))) return p_sInfo;
            return "OK"; 
        }

        public string RunHeadUp()
        {
            if (Run(m_head.RunVacuum(false))) return p_sInfo;
            if (Run(m_head.Run(false, false))) return p_sInfo;
            return "OK"; 
        }

        public string RunTaping()
        {
            if (p_eProcess != eProcess.Reticle) return "Process not Reticle : " + p_eProcess.ToString();
            p_eProcess = eProcess.Packing;
            if (Run(RunCoverClose())) return p_sInfo;
            if (Run(m_stage.RunMove(m_stage.m_degReady))) return p_sInfo;
            if (Run(m_cartridge.RunMove(Cartridge.ePos.Run))) return p_sInfo;
            if (Run(m_stage.RunRotate(m_stage.m_degAttach))) return p_sInfo;
            if (Run(m_roller.RunPush(true))) return p_sInfo;
            if (Run(m_cartridge.RunMove(Cartridge.ePos.Ready))) return p_sInfo;
            if (Run(m_stage.RunTaping())) return p_sInfo; 
            if (Run(m_cartridge.RunCutter())) return p_sInfo;
            if (Run(m_stage.RunRotate(180))) return p_sInfo;
            if (Run(RunHeadUp())) return p_sInfo;
            if (Run(m_stage.RunMove(m_stage.m_degReady))) return p_sInfo;
            p_eProcess = eProcess.Done;
            return "OK";
        }
        #endregion

        #region Override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
            m_cartridge.RunTree(tree.GetTree("Cartridge")); 
            m_roller.RunTree(tree.GetTree("Roller"));
            m_head.RunTree(tree.GetTree("Head"));
            m_stage.RunTree(tree.GetTree("Stage"));
        }

        public override void Reset()
        {
            base.Reset();
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
            m_stage.BeforeHome(); 
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            if (p_eState == eState.Ready) m_stage.RunMove(m_stage.m_degReady);
            return p_sInfo;
        }
        #endregion

        public TapePacker(string id, IEngineer engineer)
        {
            InitChildSlot();
            m_cartridge = new Cartridge("Cartridge", this);
            m_roller = new Roller("Roller", this);
            m_head = new Head("Head", this);
            m_stage = new Stage("Stage", this);
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Solvalve(this), false, "Run Solvalve");
            AddModuleRunList(new Run_Rotate(this), false, "Run Rotate");
            AddModuleRunList(new Run_Cover(this), false, "Run Cover Open, Close, Head Up");
            AddModuleRunList(new Run_Taping(this), true, "Run Taping");
        }

        public class Run_Delay : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Delay(TapePacker module)
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
                Thread.Sleep((int)(1000 * m_secDelay));
                return "OK";
            }
        }

        public class Run_Solvalve : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Solvalve(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            string m_sSol = ""; 
            bool m_bOn = false; 
            public override ModuleRunBase Clone()
            {
                Run_Solvalve run = new Run_Solvalve(m_module);
                run.m_sSol = m_sSol;
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sSol = tree.Set(m_sSol, m_sSol, m_module.p_asSol, "SolValve", "Run SolValve", bVisible);
                m_bOn = tree.Set(m_bOn, m_bOn, "On", "Run SolValue On/Off", bVisible); 
            }

            public override string Run()
            {
                Solvalue sol = m_module.GetSolvalve(m_sSol);
                if (sol == null) return "Invalid Solvalve Name";
                sol.m_dio.Write(m_bOn); 
                return "OK";
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Rotate(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fDeg = 0;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Rotate", "Rotate Degree (Degree)", bVisible);
            }

            public override string Run()
            {
                return m_module.m_stage.RunMove(m_fDeg); 
            }
        }

        public class Run_Cover : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Cover(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eCover
            { 
                Open,
                Close,
                HeadUp
            }
            eCover m_eCover = eCover.Open; 
            public override ModuleRunBase Clone()
            {
                Run_Cover run = new Run_Cover(m_module);
                run.m_eCover = m_eCover;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eCover = (eCover)tree.Set(m_eCover, m_eCover, "Cover", "Run Cover", bVisible);
            }

            public override string Run()
            {
                switch (m_eCover)
                {
                    case eCover.Open: return m_module.RunCoverOpen();
                    case eCover.Close: return m_module.RunCoverClose();
                    case eCover.HeadUp: return m_module.RunHeadUp();
                }
                return "OK";
            }
        }

        public class Run_Taping : ModuleRunBase
        {
            TapePacker m_module;
            public Run_Taping(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Taping run = new Run_Taping(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunTaping();
            }
        }
        #endregion
    }
}
