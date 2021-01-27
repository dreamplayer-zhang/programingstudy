using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class Roller : ModuleBase
    {
        #region eStep
        public enum eStep
        {
            Empty,
            Exist,
            Arrived,
            Align,
            Send,
            Picker,
        }
        eStep _eStep = eStep.Empty;
        public eStep p_eStep
        {
            get { return _eStep; }
            set
            {
                if (_eStep == value) return;
                _eStep = value;
                OnPropertyChanged(); 
                switch (value)
                {
                    case eStep.Empty:
                        foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty;
                        break; 
                }
            }
        }
        #endregion

        #region ToolBox
        Axis m_axisAlign; 
        DIO_I2O m_dioStopperUP; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate[0], this, "Rotate0");
            p_sInfo = m_toolBox.Get(ref m_axisRotate[1], this, "Rotate1");
            p_sInfo = m_toolBox.Get(ref m_axisAlign, this, "Align"); 
            p_sInfo = m_toolBox.Get(ref m_dioStopperUP, this, "StopperUp", "Down", "Up"); 
            foreach (Line line in m_aLine) line.GetTools(m_toolBox);
            if (bInit) 
            {
                InitPosWidth(); 
            }
        }
        #endregion

        #region Rotate
        double m_fJogScale = 1;
        Axis[] m_axisRotate = new Axis[2];

        public string RunRotate(bool bRotate)
        {
            if (bRotate)
            {
                m_axisRotate[0].Jog(m_fJogScale);
                m_axisRotate[1].Jog(m_fJogScale);
            }
            else
            {
                m_axisRotate[0].StopAxis();
                m_axisRotate[1].StopAxis();
            }
            return "OK";
        }

        void RunTreeRotate(Tree tree)
        {
            m_fJogScale = tree.Set(m_fJogScale, m_fJogScale, "Speed", "Rotate Speed (Scale)");
        }
        #endregion

        #region Align Axis
        public enum ePos
        {
            W70,
            W100,
        }
        void InitPosWidth()
        {
            m_axisAlign.AddPos(Enum.GetNames(typeof(ePos)));
        }

        string RunMoveAlign(double fWidth)
        {
            double fW70 = m_axisAlign.GetPosValue(ePos.W70);
            double fW100 = m_axisAlign.GetPosValue(ePos.W100);
            double dPos = (fW100 - fW70) * (fWidth - 70) / 30;
            m_axisAlign.StartMove(dPos);
            return m_axisAlign.WaitReady();
        }

        public string RunMoveAlign(bool bAlign)
        {
            double fWidth = m_rinse.p_widthStrip;
            if (bAlign == false) fWidth += 10000; 
            return RunMoveAlign(fWidth); 
        }
        #endregion

        #region Stopper
        public string RunStopperUp(bool bUp)
        {
            return m_dioStopperUP.RunSol(bUp); 
        }
        #endregion

        #region Line
        public class Line : NotifyProperty
        {
            DIO_I[] m_diCheck = new DIO_I[3];
            public DIO_I2O[] m_dioAlignUp = new DIO_I2O[2]; 
            public void GetTools(ToolBox toolBox)
            {
                m_roller.p_sInfo = toolBox.Get(ref m_diCheck[0], m_roller, m_id + ".Start");
                m_roller.p_sInfo = toolBox.Get(ref m_diCheck[1], m_roller, m_id + ".Mid");
                m_roller.p_sInfo = toolBox.Get(ref m_diCheck[2], m_roller, m_id + ".Arrived");
                m_roller.p_sInfo = toolBox.Get(ref m_dioAlignUp[0], m_roller, m_id + ".AlignL_Up", "Down", "Up");
                m_roller.p_sInfo = toolBox.Get(ref m_dioAlignUp[1], m_roller, m_id + ".AlignR_Up", "Down", "Up");
            }

            public enum eSensor
            {
                Empty,
                Exist,
                Arrived
            }
            eSensor _eSensor = eSensor.Empty; 
            public eSensor p_eSensor
            {
                get { return _eSensor; }
                set
                {
                    if (_eSensor == value) return;
                    _eSensor = value;
                    OnPropertyChanged();
                }
            }

            public void CheckSensor()
            {
                switch (p_eSensor)
                {
                    case eSensor.Empty:
                        //if (m_diCheck[0].p_bIn || m_diCheck[1].p_bIn) p_eSensor = eSensor.Exist;
                        if (m_diCheck[0].p_bIn) p_eSensor = eSensor.Exist;
                        break;
                    case eSensor.Exist:
                        if (m_diCheck[2].p_bIn) p_eSensor = eSensor.Arrived;
                        break;
                }
            }

            string m_id;
            Roller m_roller;
            public Line(string id, Roller roller)
            {
                m_id = id;
                m_roller = roller;
            }
        }

        List<Line> m_aLine = new List<Line>();
        void InitILines()
        {
            for (int n = 0; n < 4; n++) m_aLine.Add(new Line("Line" + n.ToString(), this));
        }

        public string RunAlignerUp(bool bUp)
        {
            foreach (Line line in m_aLine)
            {
                line.m_dioAlignUp[0].Write(bUp);
                line.m_dioAlignUp[1].Write(bUp);
            }
            Thread.Sleep(100);
            StopWatch sw = new StopWatch();
            int msTimeout = (int)(1000 * m_aLine[0].m_dioAlignUp[0].m_secTimeout); 
            while (sw.ElapsedMilliseconds < msTimeout)
            {
                Thread.Sleep(10);
                if (IsAlignerUpDone()) return "OK";
            }
            return "AlignerUp Timeout"; 
        }

        bool IsAlignerUpDone()
        {
            foreach (Line line in m_aLine)
            {
                if ((line.m_dioAlignUp[0].p_bDone == false) || (line.m_dioAlignUp[1].p_bDone == false)) return false; 
            }
            return true; 
        }
        #endregion

        #region Check Thread
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(2000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                foreach (Line line in m_aLine) line.CheckSensor(); 
                if ((EQ.p_eState == EQ.eState.Run) && (m_qModuleRun.Count == 0))
                {
                    switch (p_eStep)
                    {
                        case eStep.Empty:
                            foreach (Line line in m_aLine)
                            {
                                if (line.p_eSensor != Line.eSensor.Empty)
                                {
                                    p_eStep = eStep.Exist;
                                    RunStopperUp(true);
                                }
                            }
                            break;
                        case eStep.Exist:
                            int nExist = 0;
                            foreach (Line line in m_aLine)
                            {
                                if (line.p_eSensor == Line.eSensor.Exist) nExist++;
                            }
                            if (nExist == 0) p_eStep = eStep.Arrived;
                            break;
                        case eStep.Arrived:
                            p_eStep = eStep.Align;
                            break;
                        case eStep.Align:
                            p_sInfo = RunAlign();
                            break;
                        case eStep.Send:
                            int nArrived = 0;
                            foreach (Line line in m_aLine)
                            {
                                if (line.p_eSensor == Line.eSensor.Arrived) nArrived++;
                            }
                            if (nArrived == 0) p_eStep = eStep.Empty;
                            break;
                    }
                }
            }
        }
        #endregion

        #region Align
        double m_secArrived = 1;
        public List<bool> m_bExist = new List<bool>();
        public string RunAlign()
        {
            try
            {
                Thread.Sleep((int)(1000 * m_secArrived)); 
                if (Run(RunRotate(false))) return p_sInfo;
                if (m_rinse.p_eMode == RinseU.eRunMode.Magazine)
                {
                    if (Run(RunMoveAlign(false))) return p_sInfo;
                    if (Run(RunAlignerUp(true))) return p_sInfo;
                    if (Run(RunMoveAlign(true))) return p_sInfo;
                    if (Run(RunMoveAlign(false))) return p_sInfo;
                    if (Run(RunAlignerUp(false))) return p_sInfo;
                }
                for (int n = 0; n < 4; n++) m_bExist[n] = m_aLine[n].p_eSensor != Line.eSensor.Empty;
                switch (m_rinse.p_eMode)
                {
                    case RinseU.eRunMode.Magazine:
                        RunStopperUp(false);
                        while (m_rail.p_eState != eState.Ready)
                        {
                            Thread.Sleep(10);
                            if (EQ.IsStop()) return "EQ Stop";
                        }
                        m_rail.StartRun(m_bExist);
                        RunRotate(true);
                        Thread.Sleep(100);
                        p_eStep = eStep.Send;
                        foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty;
                        break;
                    case RinseU.eRunMode.Stack:
                        p_eStep = eStep.Picker;
                        break;
                }
                return "OK";
            }
            finally { RunAlignerUp(false); }
        }

        void RunTreeAlign(Tree tree)
        {
            m_secArrived = tree.Set(m_secArrived, m_secArrived, "Arrived", "Arrived Delay (sec)");
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
            RunStopperUp(false); 
            RunAlignerUp(false); 
            m_axisRotate[0].ServoOn(true);
            m_axisRotate[1].ServoOn(true);
            StateHome(m_axisAlign); 
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeRotate(tree.GetTree("Rotate"));
            RunTreeAlign(tree.GetTree("Align")); 
        }
        #endregion

        RinseU m_rinse;
        Rail m_rail;
        public Roller(string id, IEngineer engineer, RinseU rinse, Rail rail)
        {
            while (m_bExist.Count < 4) m_bExist.Add(false);
            p_id = id;
            m_rinse = rinse;
            m_rail = rail; 
            InitILines();
            InitBase(id, engineer);

            InitThreadCheck(); 
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
            base.ThreadStop();
        }

        #region StartRun
        public void StartRun()
        {
            p_eStep = eStep.Empty; 
            RunStopperUp(true);
            RunAlignerUp(false); 
            RunRotate(true);
            foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty; 
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Rotate(this), false, "Roller Rotate");
            AddModuleRunList(new Run_StopperUp(this), false, "Roller StopperUp");
            AddModuleRunList(new Run_AlignerUp(this), false, "Roller AlignerUp");
            AddModuleRunList(new Run_Align(this), false, "Roller Align");
        }

        public class Run_Rotate : ModuleRunBase
        {
            Roller m_module;
            public Run_Rotate(Roller module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bRotate = false;
            public override ModuleRunBase Clone()
            {
                Run_Rotate run = new Run_Rotate(m_module);
                run.m_bRotate = m_bRotate;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRotate = tree.Set(m_bRotate, m_bRotate, "Rotate", "Rotate Roller", bVisible);
            }

            public override string Run()
            {
                return m_module.RunRotate(m_bRotate);
            }
        }

        public class Run_StopperUp : ModuleRunBase
        {
            Roller m_module;
            public Run_StopperUp(Roller module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bUp = false;
            public override ModuleRunBase Clone()
            {
                Run_StopperUp run = new Run_StopperUp(m_module);
                run.m_bUp = m_bUp;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bUp = tree.Set(m_bUp, m_bUp, "StopperUp", "Stopper Up Down", bVisible);
            }

            public override string Run()
            {
                return m_module.RunStopperUp(m_bUp); 
            }
        }

        public class Run_AlignerUp : ModuleRunBase
        {
            Roller m_module;
            public Run_AlignerUp(Roller module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bUp = false;
            public override ModuleRunBase Clone()
            {
                Run_AlignerUp run = new Run_AlignerUp(m_module);
                run.m_bUp = m_bUp;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bUp = tree.Set(m_bUp, m_bUp, "AlignerUp", "Aligner Up Down", bVisible);
            }

            public override string Run()
            {
                return m_module.RunAlignerUp(m_bUp);
            }
        }

        public class Run_Align : ModuleRunBase
        {
            Roller m_module;
            public Run_Align(Roller module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Align run = new Run_Align(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                m_module.RunMoveAlign(true);
                return m_module.RunMoveAlign(false);
            }
        }
        #endregion
    }
}
