using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class TapePacker : ModuleBase
    {
        #region ToolBox
        DIO_I2O m_dioHead;
        DIO_I2O m_dioPad;
        DIO_I2O m_dioLidGuide;
        DIO_I m_diPressCheck;
        DIO_I m_diTopCoverCheck;
        DIO_IO m_dioVacuum;
        DIO_O m_doBlow;

        Axis m_axisRotate;
        DIO_I2O m_dioAlign;
        DIO_I m_diAlignCheck;
        DIO_I m_diCaseCheck;
        DIO_I2O m_dioPress;

        DIO_I2O m_dioCartridge;
        DIO_I2O m_dioCutter;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioHead, this, "Head", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioPad, this, "Pad", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_dioLidGuide, this, "LidGuide", "Release", "Hold");
            p_sInfo = m_toolBox.Get(ref m_diPressCheck, this, "PressCheck");
            p_sInfo = m_toolBox.Get(ref m_diTopCoverCheck, this, "TopCoverCheck");
            p_sInfo = m_toolBox.Get(ref m_dioVacuum, this, "Vacuum");
            p_sInfo = m_toolBox.Get(ref m_doBlow, this, "Blow");

            p_sInfo = m_toolBox.Get(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.Get(ref m_dioAlign, this, "Align", "Backward", "Align");
            p_sInfo = m_toolBox.Get(ref m_diAlignCheck, this, "AlignCheck");
            p_sInfo = m_toolBox.Get(ref m_diCaseCheck, this, "CaseCheck");
            p_sInfo = m_toolBox.Get(ref m_dioPress, this, "Press", "Backward", "Press");

            p_sInfo = m_toolBox.Get(ref m_dioCartridge, this, "Cartrige", "Backward", "Forward");
            p_sInfo = m_toolBox.Get(ref m_dioCutter, this, "Cutter", "Backward", "Forward");

            if (bInit) InitTools(); 
        }

        void InitTools()
        {
            m_dioHead.Write(false);
            m_dioPad.Write(false);
            m_dioLidGuide.Write(false);
            m_dioVacuum.Write(false);
            m_doBlow.Write(false);
            m_dioAlign.Write(false);
            m_dioCartridge.Write(false);
            m_dioCutter.Write(false); 
        }
        #endregion

        #region DIO Function
        string RunSol(DIO_I2O dio, bool bOn, double sWait)
        {
            dio.Write(bOn);
            Thread.Sleep(500);
            int msWait = (int)(1000 * sWait);
            while (dio.p_bDone != true)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (dio.m_swWrite.ElapsedMilliseconds > msWait) return dio.m_id + " Sol Valve Move Timeout";
            }
            return "OK"; 
        }
        #endregion

        #region Head Function
        string RunHead(bool bOn)
        {
            return RunSol(m_dioHead, bOn, m_sSolHead); 
        }

        string RunPad(bool bOn)
        {
            return RunSol(m_dioPad, bOn, m_sSolPad);
        }

        string RunLidGuide(bool bOn)
        {
            return RunSol(m_dioLidGuide, bOn, m_sSolLidGuide);
        }

        string RunVacuum(bool bOn)
        {
            m_dioVacuum.Write(bOn);
            if (bOn == false)
            {
                m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_sBlow));
                m_doBlow.Write(false);
                return "OK"; 
            }
            int msVac = (int)(1000 * m_sVac);
            while (m_dioVacuum.p_bIn != bOn)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return p_id + " EQ Stop";
                if (m_dioVacuum.m_swWrite.ElapsedMilliseconds > msVac) return "Vacuum Sensor Timeout";
            }
            return "OK";
        }

        public string RunCoverOpen()
        {
            m_dioPad.Write(false);
            if (Run(RunHead(false))) return p_sInfo;
            if (Run(RunLidGuide(false))) return p_sInfo;
            m_dioPad.Write(true);
            if (Run(RunHead(true))) return p_sInfo;
            if (Run(RunPad(true))) return p_sInfo; 
            if (Run(RunVacuum(true))) return p_sInfo;
            if (Run(RunHead(false))) return p_sInfo;
            if (Run(RunLidGuide(true))) return p_sInfo;
            return "OK"; 
        }

        public string RunCoverClose()
        {
            if (Run(RunLidGuide(false))) return p_sInfo;
            m_dioPad.Write(true);
            if (Run(RunHead(true))) return p_sInfo;
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(RunPad(false))) return p_sInfo;
            return "OK"; 
        }

        public string RunHeadUp()
        {
            if (Run(RunVacuum(false))) return p_sInfo;
            if (Run(RunPad(false))) return p_sInfo;
            if (Run(RunHead(false))) return p_sInfo;
            return "OK"; 
        }
        #endregion

        #region Stage Function
        //int m_lRotate = 2621440;
        public string RunAlign(bool bOn)
        {
            if (Run(RunSol(m_dioAlign, bOn, m_sSolAlign))) return p_sInfo;
            if (m_diAlignCheck.p_bIn != bOn) return "Align Check Sensor not Detected"; 
            return "OK"; 
        }

        double m_fRotateReady = -10; 
        public string RotateReady()
        {
            double fPulse = m_axisRotate.p_posCommand;
            while (fPulse > 2621440) fPulse -= 2621440;
            m_axisRotate.SetCommandPosition(fPulse);
            //fPulse = m_axisRotate.p_posActual;
            //while (fPulse > 2621440) fPulse -= 2621440;
            //m_axisRotate.SetActualPosition(fPulse);
            m_axisRotate.StartMove(m_fRotateReady * 2621440.0 / 360.0); 
            return m_axisRotate.WaitReady(); 
        }
        public string RotateTapingReady()
        {
            double fPulse = m_axisRotate.p_posCommand;
            while (fPulse > 2621440) fPulse -= 2621440;
            m_axisRotate.SetCommandPosition(fPulse);
            //fPulse = m_axisRotate.p_posActual;
            //while (fPulse > 2621440) fPulse -= 2621440;
            //m_axisRotate.SetActualPosition(fPulse);
            double fMovepos = 90 * 2621440.0 / 360.0;
            m_axisRotate.StartMove(fMovepos);
            return m_axisRotate.WaitReady();
        }

        public string Rotate(double fDeg, double v, double acc)
        {
            double fPulse = fDeg * 2621440.0 / 360.0;
            double vPulse = v * 2621440.0 / 360.0;
            m_axisRotate.StartMove(fPulse, vPulse, acc, acc);
            return m_axisRotate.WaitReady();
        }

        public string RunPress(bool bOn)
        {
            m_dioPress.Write(bOn);
            Thread.Sleep(1000); 
            return "OK";
        }

        void RunTreeRotate(Tree tree)
        {
            m_fRotateReady = tree.Set(m_fRotateReady, m_fRotateReady, "Ready", "Rotate Ready Position (Deg)"); 
        }
        #endregion

        #region Cartridge Function
        public string RunCartridge(bool bOn)
        {
            return RunSol(m_dioCartridge, bOn, m_sSolCartridge); 
        }

        public string RunCutter()
        {
            if (Run(RunSol(m_dioCutter, true, m_sSolCutter))) return p_sInfo;
            Thread.Sleep(1000); 
            if (Run(RunSol(m_dioCutter, false, m_sSolCutter))) return p_sInfo;
            return "OK"; 
        }
        #endregion

        #region Timeout
        double m_sSolHead = 5;
        double m_sSolPad = 5;
        double m_sSolLidGuide = 5;
        double m_sVac = 2;
        double m_sBlow = 0.5;
        double m_sSolAlign = 5;
        double m_sSolPress = 5;
        double m_sSolCartridge = 5;
        double m_sSolCutter = 5; 
        void RunTreeDIOWait(Tree tree)
        {
            m_sSolHead = tree.Set(m_sSolHead, m_sSolHead, "Head", "Sol Value Move Wait (sec)");
            m_sSolPad = tree.Set(m_sSolPad, m_sSolPad, "Pad", "Sol Value Move Wait (sec)");
            m_sSolLidGuide = tree.Set(m_sSolLidGuide, m_sSolLidGuide, "Lid Guide", "Sol Value Move Wait (sec)");
            m_sVac = tree.Set(m_sVac, m_sVac, "Vac", "Vacuum Sensor Wait (sec)");
            m_sBlow = tree.Set(m_sBlow, m_sBlow, "Blow", "Blow Time (sec)");
            m_sSolAlign = tree.Set(m_sSolAlign, m_sSolAlign, "Align", "Sol Value Move Wait (sec)");
            m_sSolPress = tree.Set(m_sSolPress, m_sSolPress, "Press", "Sol Value Move Wait (sec)");

            m_sSolCartridge = tree.Set(m_sSolCartridge, m_sSolCartridge, "Cartridge", "Sol Value Move Wait (sec)");
            m_sSolCutter = tree.Set(m_sSolCutter, m_sSolCutter, "Cutter", "Sol Value Move Wait (sec)");
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
            RunTreeRotate(tree.GetTree("Rotate", false));
            RunTreeDIOWait(tree.GetTree("Timeout", false)); 
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
            m_axisRotate.ServoOn(true);
            Thread.Sleep(100);
            m_axisRotate.Jog(0.3);
            Thread.Sleep(1200);
            m_axisRotate.StopAxis();
            Thread.Sleep(100);
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            if (p_eState == eState.Ready) RotateReady(); 
            return p_sInfo;
        }
        #endregion

        public TapePacker(string id, IEngineer engineer)
        {
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
            AddModuleRunList(new Run_SolValve(this), false, "Run SolValve");
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

        public class Run_SolValve : ModuleRunBase
        {
            TapePacker m_module;
            public Run_SolValve(TapePacker module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eSolValve
            {
                Cartridge,
                Head,
                Pad,
            }
            eSolValve m_eSol = eSolValve.Cartridge;
            bool m_bOn = false; 
            public override ModuleRunBase Clone()
            {
                Run_SolValve run = new Run_SolValve(m_module);
                run.m_eSol = m_eSol;
                run.m_bOn = m_bOn;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eSol = (eSolValve)tree.Set(m_eSol, m_eSol, "SolValve", "Run SolValve", bVisible);
                m_bOn = tree.Set(m_bOn, m_bOn, "On", "Run SolValue On/Off", bVisible); 
            }

            public override string Run()
            {
                switch (m_eSol)
                {
                    case eSolValve.Cartridge: return m_module.RunCartridge(m_bOn);
                    case eSolValve.Head: return m_module.RunHead(m_bOn);
                    case eSolValve.Pad: return m_module.RunPad(m_bOn); 
                }
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
            double m_v = 120;
            double m_acc = 2;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_fDeg = m_fDeg;
                run.m_v = m_v;
                run.m_acc = m_acc;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Rotate", "Rotate Degree (Degree)", bVisible);
                m_v = tree.Set(m_v, m_v, "Velocity", "Rotate Velocity (Degree/sec)", bVisible);
                m_acc = tree.Set(m_acc, m_acc, "Acceleration", "Rotate Acceleration Time (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.Rotate(m_fDeg, m_v, m_acc); 
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

            double m_sCartride = 1;
            double m_fDeg = 720;
            double m_v = 120;
            double m_acc = 2; 
            public override ModuleRunBase Clone()
            {
                Run_Taping run = new Run_Taping(m_module);
                run.m_sCartride = m_sCartride;
                run.m_fDeg = m_fDeg;
                run.m_v = m_v;
                run.m_acc = m_acc; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sCartride = tree.Set(m_sCartride, m_sCartride, "Cartridge", "Cartridge Push Time (sec)", bVisible);
                RunTreeRotate(tree.GetTree("Rotate", true, bVisible), bVisible); 
            }

            void RunTreeRotate(Tree tree, bool bVisible)
            {
                m_fDeg = tree.Set(m_fDeg, m_fDeg, "Rotate", "Rotate Degree (Degree)", bVisible);
                m_v = tree.Set(m_v, m_v, "Velocity", "Rotate Velocity (Degree/sec)", bVisible);
                m_acc = tree.Set(m_acc, m_acc, "Acceleration", "Rotate Acceleration Time (sec)", bVisible);
            }

            public override string Run()
            {
                for (int i = 0; i < 1; i++)
                {
                    if (m_module.Run(m_module.RunCoverClose())) return p_sInfo;
                    if (m_module.Run(m_module.RotateReady())) return p_sInfo;
                    if (m_module.Run(m_module.RunPress(true))) return p_sInfo;
                    if (m_module.Run(m_module.RunCartridge(true))) return p_sInfo;
                    //Thread.Sleep((int)(1000 * m_sCartride));
                    if (m_module.Run(m_module.Rotate(13.0, m_v, m_acc))) return p_sInfo;
                    if (m_module.Run(m_module.RunCartridge(false))) return p_sInfo;
                    if (m_module.Run(m_module.Rotate(m_fDeg, m_v, m_acc))) return p_sInfo;
                    if (m_module.Run(m_module.RunCutter())) return p_sInfo;
                    if (m_module.Run(m_module.Rotate(m_fDeg+180.0, m_v, m_acc))) return p_sInfo;
                    if (m_module.Run(m_module.RotateReady())) return p_sInfo;
                    //if (m_module.Run(m_module.RunCoverClose())) return p_sInfo;
                    //if (m_module.Run(m_module.RotateReady())) return p_sInfo;
                    //if (m_module.Run(m_module.RunPress(true))) return p_sInfo;
                    //if (m_module.Run(m_module.RunCartridge(true))) return p_sInfo;
                    //Thread.Sleep((int)(1000 * m_sCartride));
                    //if (m_module.Run(m_module.RunCartridge(false))) return p_sInfo;
                    //if (m_module.Run(m_module.Rotate(m_fDeg, m_v, m_acc))) return p_sInfo;
                    //if (m_module.Run(m_module.RunCutter())) return p_sInfo;
                    //if (m_module.Run(m_module.Rotate(m_fDeg + 360, m_v, m_acc))) return p_sInfo;
                    //if (m_module.Run(m_module.RunPress(false))) return p_sInfo;
                    //if (m_module.Run(m_module.RotateReady())) return p_sInfo;
                    //if (m_module.Run(m_module.RunHeadUp())) return p_sInfo;
                }
                return "OK";
            }
        }
        #endregion
    }
}
