using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_Pine2.Module
{
    public class MGZ_EV : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_conveyor.GetTools(m_toolBox, this, bInit);
            m_elevator.GetTools(m_toolBox, this, bInit); 
            if (bInit) InitTools();
        }

        void InitTools()
        {
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
                    case eStep.Ready: m_pine2.m_display.Write(m_nID, "REDY"); break;
                    case eStep.Load: m_pine2.m_display.Write(m_nID, "LOAD"); break;
                    case eStep.Run: m_pine2.m_display.Write(m_nID, "RUN "); break;
                    case eStep.Unload: m_pine2.m_display.Write(m_nID, "UNLD"); break;
                }
            }
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

            public void RunSwitch(MGZ_EV module, int nBlink)
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

            public string WaitSwitch()
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
                return (sw.ElapsedMilliseconds < 1500) ? "OK" : "Cancel";
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
            public enum eMGZ
            {
                Up,
                Down
            }
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
                toolBox.GetDIO(ref m_diProduct, module, "Product", Enum.GetNames(typeof(eMGZ)));
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

            public string MoveConveyor(eMGZ eMGZ, bool bWait = true)
            {
                m_axis.StartMove((eMGZ == eMGZ.Up) ? ePos.ConveyorUp : ePos.ConveyorDown);
                if (bWait == false) return "OK";
                return m_axis.WaitReady();
            }

            double m_dSlot = 6000; 
            public string MoveTransfer(eMGZ eMGZ, int nSlot, bool bWait = true)
            {
                m_axis.StartMove((eMGZ == eMGZ.Up) ? ePos.TransferUp : ePos.TransferDown, -nSlot * m_dSlot);
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
            public string WaitProduct(eMGZ eMGZ)
            {
                StopWatch sw = new StopWatch();
                int msWait = (int)(1000 * m_secProduct); 
                while (sw.ElapsedMilliseconds < msWait)
                {
                    Thread.Sleep(10);
                    if (m_diProduct.ReadDI(eMGZ)) return "OK";
                }
                return "Wait Product Timeout"; 
            }

            public bool IsProduct(eMGZ eMGZ)
            {
                return m_diProduct.ReadDI(eMGZ); 
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

        #region override
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
        }
        #endregion

        #region Load
        string StartLoad()
        {
            if (m_elevator.IsProduct(Elevator.eMGZ.Up)) return "Magazine Up Sensor Checked";
            if (m_elevator.IsProduct(Elevator.eMGZ.Down)) return "Magazine Down Sensor Checked";
            p_eStep = eStep.Load;
            return StartRun(m_runLoad);
        }

        public string RunLoad()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    if (Run(RunLoad(Elevator.eMGZ.Up))) return p_sInfo;
                    if (Run(RunLoad(Elevator.eMGZ.Down))) return p_sInfo;
                    break;
                case Pine2.eRunMode.Stack:
                    if (Run(RunLoad(Elevator.eMGZ.Up))) return p_sInfo;
                    break; 
            }
            p_eStep = eStep.Run; 
            return "OK";
        }

        string RunLoad(Elevator.eMGZ eMGZ)
        {
            if (Run(m_elevator.MoveConveyor(eMGZ))) return p_sInfo;
            if (Run(m_conveyor.WaitSwitch())) return p_sInfo;
            if (m_conveyor.IsCheckExist() == false) return "OK"; 
            if (Run(m_elevator.RunAlign(false))) return p_sInfo;
            m_conveyor.RunMove(Conveyor.eMove.Forward);
            if (Run(m_elevator.WaitProduct(eMGZ))) return p_sInfo;
            m_conveyor.RunMoveStop();
            if (Run(m_elevator.RunAlign(true))) return p_sInfo;
            return "OK";
        }
        #endregion

        #region Unload
        string StartUnload()
        {
            p_eStep = eStep.Unload; 
            return StartRun(m_runUnload);
        }

        public string RunUnload()
        {
            switch (m_pine2.p_eMode)
            {
                case Pine2.eRunMode.Magazine:
                    if (Run(RunUnload(Elevator.eMGZ.Down))) return p_sInfo;
                    if (Run(RunUnload(Elevator.eMGZ.Up))) return p_sInfo;
                    break;
                case Pine2.eRunMode.Stack:
                    if (Run(RunUnload(Elevator.eMGZ.Up))) return p_sInfo;
                    break;
            }
            p_eStep = eStep.Ready; 
            return "OK"; 
        }

        string RunUnload(Elevator.eMGZ eMGZ)
        {
            if (m_elevator.IsProduct(eMGZ) == false) return "OK"; 
            if (Run(m_elevator.RunAlign(true))) return p_sInfo;
            if (Run(m_elevator.MoveConveyor(eMGZ))) return p_sInfo;
            if (Run(m_elevator.RunAlign(false))) return p_sInfo;
            m_conveyor.RunMove(Conveyor.eMove.Backward);
            Thread.Sleep(1000);
            if (Run(m_conveyor.WaitUnload())) return p_sInfo; 
            return "OK";
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_elevator.RunTree(tree.GetTree("Elevator")); 
        }
        #endregion

        int m_nID = 1;
        Pine2 m_pine2; 
        public MGZ_EV(string id, int nID, IEngineer engineer, Pine2 pine2)
        {
            m_nID = nID + 1;
            p_id = id + m_nID.ToString();
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
        protected override void InitModuleRuns()
        {
            m_runLoad = AddModuleRunList(new Run_Load(this), false, "Load Magazine or Stack");
            m_runUnload = AddModuleRunList(new Run_Unload(this), false, "Unload Magazine or Stack");
            AddModuleRunList(new Run_Align(this), false, "Run Align");
        }

        public class Run_Load : ModuleRunBase
        {
            MGZ_EV m_module;
            public Run_Load(MGZ_EV module)
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
                return m_module.RunLoad();
            }
        }

        public class Run_Unload : ModuleRunBase
        {
            MGZ_EV m_module;
            public Run_Unload(MGZ_EV module)
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
            MGZ_EV m_module;
            public Run_Align(MGZ_EV module)
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
        #endregion
    }
}
