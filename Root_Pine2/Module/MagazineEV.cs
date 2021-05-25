using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Pine2.Module
{
    public class MagazineEV : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_conveyor.GetTools(m_toolBox, this, bInit);
            m_elevator.GetTools(m_toolBox, this, bInit); 
        }
        #endregion

        #region eStep
        public enum eStep
        {
            Ready,
            Load,
            Run,
            Unload,
        }
        eStep _eStep = eStep.Ready; 
        public eStep p_eStep
        {
            get { return _eStep; }
            set
            {
                if (_eStep == value) return; 
                _eStep = value; 
                switch (value)
                {
                    case eStep.Ready: p_sLED = "REDY"; break;
                    case eStep.Load: p_sLED = "LOAD"; break;
                    case eStep.Run: p_sLED = "RUN "; break;
                    case eStep.Unload: p_sLED = "UNLD"; break;
                }
            }
        }

        int m_nUnitLED = 0; 
        string _sLED = "";
        public string p_sLED
        {
            get { return _sLED; }
            set
            {
                if (_sLED == value) return;
                _sLED = value;
                m_pine2.m_display.Write(m_nUnitLED, value); 
            }
        }

        void RunTreeLED(Tree tree)
        {
            m_nUnitLED = tree.Set(m_nUnitLED, m_nUnitLED, "Unit", "LED Display Modbus Unit ID");
        }
        #endregion

        #region Conveyor
        public class Conveyor
        {
            public enum eMove
            {
                Backward,
                Forward
            }
            enum eCheck
            {
                Unload,
                Mid,
                Inside
            }
            DIO_IO m_dioSwitch;
            DIO_Os m_doMove;
            DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetDIO(ref m_dioSwitch, module, "Switch", false);
                toolBox.GetDIO(ref m_doMove, module, "Move", Enum.GetNames(typeof(eMove)));
                toolBox.GetDIO(ref m_diCheck, module, "Check", Enum.GetNames(typeof(eCheck)));
            }

            public void RunSwitch(MagazineEV module, int nBlink)
            {
                if (module.p_eStep == eStep.Ready)
                {
                    if (IsCheckExist()) module.StartLoad(); 
                }
                switch (module.p_eStep)
                {
                    case eStep.Load: m_dioSwitch.Write((nBlink <= 0) || (nBlink == 2)); break;
                    default: m_dioSwitch.Write(nBlink <= 1); break;
                }
            }

            public string WaitSwitch(ref double msWait)
            {
                while (m_dioSwitch.p_bIn == false)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
                StopWatch sw = new StopWatch();
                while (m_dioSwitch.p_bIn)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                msWait = sw.ElapsedMilliseconds; 
                return "OK";
            }

            public void RunMove(eMove eMove)
            {
                m_doMove.AllOff(); 
                Thread.Sleep(100);
                m_doMove.Write(eMove);
            }

            public void RunMoveStop()
            {
                m_doMove.AllOff(); 
            }

            public bool IsCheckExist()
            {
                for (int n = 0; n < 3; n++)
                {
                    if (m_diCheck.ReadDI(n)) return true; 
                }
                return false; 
            }

            public bool IsCheckUnload()
            {
                return m_diCheck.ReadDI(eCheck.Unload); 
            }

            public string WaitUnload()
            {
                while (m_diCheck.ReadDI(eCheck.Unload) == false)
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop"; 
                    if (IsCheckExist() == false)
                    {
                        RunMoveStop();
                        return "OK"; 
                    }
                }
                RunMoveStop(); 
                while (IsCheckExist())
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                return "OK"; 
            }
        }
        Conveyor m_conveyor = new Conveyor(); 
        #endregion

        #region Elevator
        public class Elevator
        {
            Axis m_axis; 
            DIO_Os m_doAlign;
            DIO_Is m_diAlign;
            DIO_Is m_diProduct;
            DIO_I m_diProtrude;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, module, "Elevator");
                toolBox.GetDIO(ref m_doAlign, module, "Align", new string[2] { "Backward", "Forward" });
                toolBox.GetDIO(ref m_diAlign, module, "Align", new string[4] { "Backward", "95", "74, 77", "Check" });
                toolBox.GetDIO(ref m_diProduct, module, "Product", Enum.GetNames(typeof(InfoStrip.eMagazinePos)));
                toolBox.GetDIO(ref m_diProtrude, module, "Protrude");
                if (bInit)
                {
                    InitPos(); 
                }
            }

            #region Elevator
            public enum ePos
            {
                ConveyorUp,
                ConveyorDown,
                TransferUp,
                TransferDown,
                Stack
            }
            void InitPos()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            }

            public string MoveConveyor(InfoStrip.eMagazinePos eMagazinePos, bool bWait = true)
            {
                m_axis.StartMove((eMagazinePos == InfoStrip.eMagazinePos.Up) ? ePos.ConveyorUp : ePos.ConveyorDown);
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }

            double m_dSlot = 6000;
            public string MoveTransfer(InfoStrip infoStrip, bool bWait = true)
            {
                ePos ePos = (infoStrip.p_eMagazinePos == InfoStrip.eMagazinePos.Up) ? ePos.TransferUp : ePos.TransferDown;
                m_axis.StartMove(ePos, -m_dSlot * infoStrip.p_nStrip);
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }

            public string MoveStack(bool bWait = true)
            {
                m_axis.StartMove(ePos.Stack);
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }
            #endregion

            #region Align
            double m_secAlign = 5; 
            public string RunAlign(bool bAlign)
            {
                m_doAlign.Write("Backward", !bAlign);
                m_doAlign.Write("Forward", bAlign);
                StopWatch sw = new StopWatch();
                int msAlign = (int)(1000 * m_secAlign); 
                while (sw.ElapsedMilliseconds < msAlign)
                {
                    Thread.Sleep(10);
                    if (bAlign && IsAlignOn()) return "OK";
                    if (!bAlign && IsAlignOff()) return "OK";
                }
                return "Align Timeout";
            }

            bool IsAlignOn()
            {
                if (m_diAlign.ReadDI("Backward")) return false;
                if (m_diAlign.ReadDI("Check") == false) return false;
                return m_diAlign.ReadDI("95") || m_diAlign.ReadDI("74, 77"); 
            }

            bool IsAlignOff()
            {
                if (m_diAlign.ReadDI("95")) return false;
                if (m_diAlign.ReadDI("74, 77")) return false;
                if (m_diAlign.ReadDI("Check")) return false;
                return m_diAlign.ReadDI("Backward");
            }
            #endregion

            #region Product & Protrude
            double m_secProduct = 10; 
            public string WaitProduct(InfoStrip.eMagazinePos eMagazinePos)
            {
                StopWatch sw = new StopWatch();
                int msWait = (int)(1000 * m_secProduct); 
                while (sw.ElapsedMilliseconds < msWait)
                {
                    Thread.Sleep(10);
                    if (m_diProduct.ReadDI(eMagazinePos)) return "OK";
                }
                return "Wait Product Timeout"; 
            }

            public bool IsProduct(InfoStrip.eMagazinePos eMagazinePos)
            {
                return m_diProduct.ReadDI(eMagazinePos); 
            }

            public bool IsProtrude()
            {
                return m_diProtrude.p_bIn; 
            }
            #endregion

            public void RunTree(Tree tree)
            {
                m_dSlot = tree.Set(m_dSlot, m_dSlot, "Slot Interval", "Magazine Slot Interval (pulse)");
                m_secAlign = tree.Set(m_secAlign, m_secAlign, "Align Timeout", "Align Timeout (sec)");
                m_secProduct = tree.Set(m_secProduct, m_secProduct, "Wait Product", "Wait Product (sec)"); 
            }
        }
        Elevator m_elevator = new Elevator();
        #endregion

        #region Stack
        public class Stack : NotifyProperty
        {
            InfoStrip.eResult _eResult = InfoStrip.eResult.Init;
            public InfoStrip.eResult p_eResult
            {
                get { return _eResult; }
                set
                {
                    if (_eResult == value) return;
                    _eResult = value;
                    OnPropertyChanged();
                    WriteResultLED();
                }
            }

            public void WriteResultLED()
            {
                switch (p_eResult)
                {
                    case InfoStrip.eResult.Init: m_magazineEV.p_sLED = "INIT"; break;
                    case InfoStrip.eResult.Good: m_magazineEV.p_sLED = "GOOD"; break;
                    case InfoStrip.eResult.XOut: m_magazineEV.p_sLED = "XOUT"; break;
                    case InfoStrip.eResult.Rework: m_magazineEV.p_sLED = "WORK"; break;
                    case InfoStrip.eResult.Error: m_magazineEV.p_sLED = "ERRR"; break;
                    case InfoStrip.eResult.Paper: m_magazineEV.p_sLED = "PAPR"; break;
                }
            }

            int _nStack = 0;
            public int p_nStack
            {
                get { return _nStack; }
                set
                {
                    _nStack = value;
                    OnPropertyChanged();
                }
            }

            int p_lStack { get { return m_magazineEV.m_pine2.p_lStack; } }

            public void PutInfoStrip(InfoStrip infoStrip)
            {
                p_nStack++;
                if (p_nStack >= p_lStack) m_magazineEV.StartUnload(); 
            }

            MagazineEV m_magazineEV; 
            public Stack(MagazineEV magazineEV)
            {
                m_magazineEV = magazineEV;
            }
        }
        public Stack m_stack = null;
        #endregion

        #region Magazine
        public class Magazine
        {
            List<InfoStrip> m_aInfoStrip = new List<InfoStrip>();
            private void InfoStrip_OnDispose(InfoStrip infoStrip)
            {
                m_aInfoStrip.Remove(infoStrip); 
            }

            public InfoStrip GetInfoStrip(bool bPeek)
            {
                if (m_qInfoStrip.Count == 0) return null;
                if (bPeek) return m_qInfoStrip.Peek(); 
                InfoStrip infoStrip = m_qInfoStrip.Dequeue();
                m_aInfoStrip.Add(infoStrip);
                return infoStrip; 
            }

            public void PutInfoStrip(InfoStrip infoStrip)
            {
                m_aInfoStrip.Remove(infoStrip);
                m_magazineEV.CheckMagazineDone(); 
            }

            public bool IsDone()
            {
                if (m_qInfoStrip.Count > 0) return false;
                if (m_aInfoStrip.Count > 0) return false;
                return true; 
            }

            MagazineEV m_magazineEV; 
            Queue<InfoStrip> m_qInfoStrip = new Queue<InfoStrip>(); 
            public Magazine(MagazineEV magazineEV, InfoStrip.eMagazinePos eMagazinePos)
            {
                m_magazineEV = magazineEV; 
                for (int n = 0; n < 20; n++)
                {
                    InfoStrip infoStrip = new InfoStrip(magazineEV.p_eMagazine, eMagazinePos, n);
                    infoStrip.OnDispose += InfoStrip_OnDispose;
                    m_qInfoStrip.Enqueue(infoStrip); 
                }
            }
        }
        Dictionary<InfoStrip.eMagazinePos, Magazine> m_aMagazine = new Dictionary<InfoStrip.eMagazinePos, Magazine>(); 
        void InitMagazine()
        {
            m_aMagazine.Add(InfoStrip.eMagazinePos.Up, null);
            m_aMagazine.Add(InfoStrip.eMagazinePos.Down, null); 
        }
        #endregion

        #region InfoStrip
        public InfoStrip GetInfoStrip(bool bPeek)
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: return null;
                case Pine2.eRunMode.Magazine:
                    InfoStrip infoStrip = GetInfoStrip(m_aMagazine[InfoStrip.eMagazinePos.Down], bPeek);
                    if (infoStrip != null) return infoStrip; 
                    return GetInfoStrip(m_aMagazine[InfoStrip.eMagazinePos.Up], bPeek);
            }
            return null; 
        }

        InfoStrip GetInfoStrip(Magazine magazine, bool bPeek)
        {
            if (magazine == null) return null;
            return magazine.GetInfoStrip(bPeek); 
        }

        public void PutInfoStrip(InfoStrip infoStrip)
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: m_stack?.PutInfoStrip(infoStrip); break;
                case Pine2.eRunMode.Magazine: m_aMagazine[infoStrip.p_eMagazinePos]?.PutInfoStrip(infoStrip); break;
            }
        }

        public void CheckMagazineDone()
        {
            foreach (Magazine magazine in m_aMagazine.Values)
            {
                if ((magazine != null) && (magazine.IsDone() == false)) return; 
            }
            StartUnload();
        }
        #endregion

        #region override
        public override string StateReady()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack: break; 
            }
            if (EQ.p_eState != EQ.eState.Run) return "OK";
            return "OK";
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
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

        #region StartRun
        public void StartRun()
        {
            if (EQ.p_bPickerSet) return;
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Stack:
                    m_elevator.MoveStack();
                    break;
                case Pine2.eRunMode.Magazine:
                    InfoStrip infoStrip = GetInfoStrip(true);
                    if (infoStrip == null) return;
                    m_elevator.MoveTransfer(infoStrip);
                    break; 
            }
        }
        #endregion

        #region Load
        string StartLoad()
        {
            if (m_elevator.IsProduct(InfoStrip.eMagazinePos.Up)) return "Magazine Up Sensor Checked";
            if (m_elevator.IsProduct(InfoStrip.eMagazinePos.Down)) return "Magazine Down Sensor Checked";
            p_eStep = eStep.Load;
            return StartRun(m_runLoad);
        }

        public string RunLoad()
        {
            double msWait = 0;
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    if (Run(m_conveyor.WaitSwitch(ref msWait))) return p_sInfo;
                    if (msWait > 1500) return "Cancel";
                    if (Run(RunLoad(InfoStrip.eMagazinePos.Up))) return p_sInfo;
                    m_aMagazine[InfoStrip.eMagazinePos.Up] = new Magazine(this, InfoStrip.eMagazinePos.Up);
                    if (Run(m_conveyor.WaitSwitch(ref msWait))) return p_sInfo;
                    if (msWait <= 1500)
                    {
                        if (Run(RunLoad(InfoStrip.eMagazinePos.Down))) return p_sInfo;
                        m_aMagazine[InfoStrip.eMagazinePos.Down] = new Magazine(this, InfoStrip.eMagazinePos.Down);
                    }
                    MoveTransfer();
                    break;
                case Pine2.eRunMode.Stack:
                    if (Run(m_conveyor.WaitSwitch(ref msWait))) return p_sInfo;
                    if (msWait > 1500) return "Cancel"; 
                    if (Run(RunLoad(InfoStrip.eMagazinePos.Up))) return p_sInfo;
                    if (Run(m_elevator.MoveStack())) return p_sInfo;
                    m_stack = new Stack(this);
                    break; 
            }
            p_eStep = eStep.Run; 
            return "OK";
        }

        string RunLoad(InfoStrip.eMagazinePos eMagazinePos)
        {
            if (Run(m_elevator.MoveConveyor(eMagazinePos))) return p_sInfo;
            if (m_conveyor.IsCheckExist() == false) return "OK"; 
            if (Run(m_elevator.RunAlign(false))) return p_sInfo;
            m_conveyor.RunMove(Conveyor.eMove.Forward);
            if (Run(m_elevator.WaitProduct(eMagazinePos))) return p_sInfo;
            m_conveyor.RunMoveStop();
            if (Run(m_elevator.RunAlign(true))) return p_sInfo;
            return "OK";
        }
        #endregion

        #region Unload
        public string StartUnload()
        {
            Thread.Sleep(100); 
            p_eStep = eStep.Unload; 
            return StartRun(m_runUnload);
        }

        public string RunUnload()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    if (Run(RunUnload(InfoStrip.eMagazinePos.Down))) return p_sInfo;
                    m_aMagazine[InfoStrip.eMagazinePos.Down] = null; 
                    if (Run(RunUnload(InfoStrip.eMagazinePos.Up))) return p_sInfo;
                    m_aMagazine[InfoStrip.eMagazinePos.Up] = null; 
                    break;
                case Pine2.eRunMode.Stack:
                    if (Run(RunUnload(InfoStrip.eMagazinePos.Up))) return p_sInfo;
                    m_stack = null; 
                    break;
            }
            p_eStep = eStep.Ready; 
            return "OK"; 
        }

        string RunUnload(InfoStrip.eMagazinePos eMagazinePos)
        {
            if (m_elevator.IsProduct(eMagazinePos) == false) return "OK"; 
            if (Run(m_elevator.RunAlign(true))) return p_sInfo;
            if (Run(m_elevator.MoveConveyor(eMagazinePos))) return p_sInfo;
            if (Run(m_elevator.RunAlign(false))) return p_sInfo;
            m_conveyor.RunMove(Conveyor.eMove.Backward);
            Thread.Sleep(1000);
            if (Run(m_conveyor.WaitUnload())) return p_sInfo;
            m_stack?.WriteResultLED(); 
            return "OK";
        }
        #endregion

        #region Move Transfer
        public string StartMoveTransfer(InfoStrip infoStrip)
        {
            Run_MoveTransfer run = (Run_MoveTransfer)m_runMoveTransfer.Clone();
            run.m_infoStrip = infoStrip; 
            return StartRun(run); 
        }

        public string MoveTransfer(bool bWait = true)
        {
            InfoStrip infoStrip = GetInfoStrip(true);
            if (infoStrip == null) return "InfoStrip not Found";
            return m_elevator.MoveTransfer(infoStrip, bWait); 
        }

        public string MoveTransfer(InfoStrip infoStrip, bool bWait = true)
        {
            if (infoStrip == null) return "InfoStrip not Found";
            return m_elevator.MoveTransfer(infoStrip, bWait);
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeLED(tree.GetTree("LED")); 
            m_elevator.RunTree(tree.GetTree("Elevator"));
        }
        #endregion

        InfoStrip.eMagazine p_eMagazine { get; set; }
        Pine2 m_pine2; 
        public MagazineEV(InfoStrip.eMagazine eMagazine, IEngineer engineer, Pine2 pine2)
        {
            InitMagazine(); 
            p_eMagazine = eMagazine;
            p_id = eMagazine.ToString(); 
            m_nUnitLED = (int)eMagazine + 1; 
            m_pine2 = pine2; 
            base.InitBase(p_id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        ModuleRunBase m_runLoad;
        ModuleRunBase m_runUnload;
        ModuleRunBase m_runMoveTransfer; 
        protected override void InitModuleRuns()
        {
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Load Magazine or Stack");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Unload Magazine or Stack");
            AddModuleRunList(new Run_Align(this), false, "Run Align");
            AddModuleRunList(new Run_MoveStack(this), false, "Move Stack Position");
            m_runMoveTransfer = AddModuleRunList(new Run_MoveTransfer(this), false, "Move Transfer Position");
        }

        public class Run_Load : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Load(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Load run = new Run_Load(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                string sRun = m_module.RunLoad();
                switch (sRun)
                {
                    case "OK": return "OK";
                    case "Cancel":
                        m_module.p_eStep = eStep.Ready;
                        return "OK";
                    default:
                        m_module.p_eStep = eStep.Ready;
                        return sRun; 
                }
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Unload(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Unload run = new Run_Unload(m_module);
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

        public class Run_Align : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_Align(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bAlign = false; 
            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                run.m_bAlign = m_bAlign; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bAlign = tree.Set(m_bAlign, m_bAlign, "Align", "Run Align", bVisible); 
            }

            public override string Run()
            {
                return m_module.m_elevator.RunAlign(m_bAlign); 
            }
        }

        public class Run_MoveStack : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_MoveStack(MagazineEV module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_MoveStack run = new Run_MoveStack(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.m_elevator.MoveStack();
            }
        }

        public class Run_MoveTransfer : ModuleRunBase
        {
            MagazineEV m_module;
            public Run_MoveTransfer(MagazineEV module)
            {
                m_module = module;
                m_infoStrip = new InfoStrip(module.p_eMagazine, InfoStrip.eMagazinePos.Up, 0); 
                InitModuleRun(module);
            }

            public InfoStrip m_infoStrip;
            public override ModuleRunBase Clone()
            {
                Run_MoveTransfer run = new Run_MoveTransfer(m_module);
                run.m_infoStrip = m_infoStrip.Clone(); 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                if (m_infoStrip == null) return;
                m_infoStrip.RunTreeMagazine(tree, bVisible); 
            }

            public override string Run()
            {
                if (m_infoStrip == null) return m_module.MoveTransfer();
                else return m_module.MoveTransfer(m_infoStrip); 
            }
        }
        #endregion
    }
}
