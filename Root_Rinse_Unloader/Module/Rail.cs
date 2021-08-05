using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class Rail : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.GetAxis(ref m_axisWidth, this, "Width");
            p_sInfo = m_toolBox.GetDIO(ref m_dioPusher, this, "Pusher", "Back", "Push");
            p_sInfo = m_toolBox.GetDIO(ref m_dioPusherDown, this, "PusherDown", "Up", "Down");
            p_sInfo = m_toolBox.GetDIO(ref m_diPusherOverload, this, "PusherOverload", new string[4] { "1", "2", "3", "4" });
            foreach (Line line in m_aLine) line.GetTools(m_toolBox);
            if (bInit)
            {
                InitALID(); 
                m_dioPusherDown.Write(true); 
                InitPosWidth();
            }
        }
        #endregion

        #region GAF
        ALID m_alidHome;
        ALID m_alidArrived;
        ALID m_alidPusher;
        ALID m_alidAxis;
        void InitALID()
        {
            m_alidHome = m_gaf.GetALID(this, "Home Error", "Home Error");
            m_alidArrived = m_gaf.GetALID(this, "Arrived", "Arrived Sensor Timeout");
            m_alidPusher = m_gaf.GetALID(this, "Pusher", "Pusher Error");
            m_alidAxis = m_gaf.GetALID(this, "Rotate Axis Alarm", "Rotate Axis Alarm");
        }
        #endregion

        #region Line
        public class Line : NotifyProperty
        {
            public DIO_I[] m_diCheck = new DIO_I[3];
            public void GetTools(ToolBox toolBox)
            {
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[0], m_rail, m_id + ".Start");
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[1], m_rail, m_id + ".Mid");
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[2], m_rail, m_id + ".Arrived");
            }

            public enum eSensor
            {
                Empty,
                Exist,
                Arrived,
                Push
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

            public string CheckSensor()
            {
                switch (p_eSensor)
                {
                    case eSensor.Empty:
                        if (m_diCheck[0].p_bIn || m_diCheck[1].p_bIn) p_eSensor = eSensor.Exist;
                        break;
                    case eSensor.Exist:
                        if (m_diCheck[2].p_bIn) p_eSensor = eSensor.Arrived;
                        break;
                    case eSensor.Arrived:
                        if (m_diCheck[1].p_bIn == false) p_eSensor = eSensor.Push;
                        break; 
                }
                return "OK";
            }

            public void CheckInitRunSensor()
            {
                if (p_eSensor == eSensor.Empty) return;
                if (m_diCheck[0].p_bIn) return;
                if (m_diCheck[1].p_bIn) return;
                if (m_diCheck[2].p_bIn) return;
                p_eSensor = eSensor.Empty; 
            }

            string m_id;
            Rail m_rail;
            public Line(string id, Rail rail)
            {
                m_id = id;
                m_rail = rail;
            }
        }

        public List<Line> m_aLine = new List<Line>();
        void InitLines()
        {
            for (int n = 0; n < 4; n++) m_aLine.Add(new Line("Line" + n.ToString(), this));
        }

        public bool IsStripExist()
        {
            foreach (Line line in m_aLine)
            {
                for (int n = 0; n < 3; n++)
                {
                    if (line.m_diCheck[n].p_bIn) return true; 
                }
            }
            return false; 
        }
        #endregion

        #region Width
        Axis m_axisWidth;
        public enum ePos
        {
            W75,
            W85
        }
        void InitPosWidth()
        {
            m_axisWidth.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string RunMoveWidth(double fWidth)
        {
            double fW75 = m_axisWidth.GetPosValue(ePos.W75);
            double fW85 = m_axisWidth.GetPosValue(ePos.W85);
            double dPos = (fW85 - fW75) * (fWidth - 75) / 10 + fW75;
            m_axisWidth.StartMove(dPos);
            return m_axisWidth.WaitReady();
        }
        #endregion

        #region Rotate
        Axis m_axisRotate;

        public string RunRotate(bool bRotate)
        {
            m_alidAxis.p_bSet = m_axisRotate.p_sensorAlarm;
            if (bRotate) m_axisRotate.Jog(m_rinse.p_fRotateSpeed, "Move");
            else m_axisRotate.StopAxis(); 
            return "OK";
        }
        #endregion

        #region Pusher
        DIO_I2O m_dioPusher;
        public DIO_I2O2 m_dioPusherDown;
        DIO_Is m_diPusherOverload;

        public string RunPusherDown(bool bDown)
        {
            return m_dioPusherDown.RunSol(bDown); 
        }

        public string RunPusher()
        {
            try
            {
                if (Run(m_storage.MoveMagazine(false))) return p_sInfo; 
                if (Run(m_dioPusher.RunSol(false))) return p_sInfo;
                if (Run(RunPusherDown(true))) return p_sInfo;
                while (m_storage.IsBusy())
                {
                    Thread.Sleep(10);
                    if (EQ.IsStop()) return "EQ Stop";
                }
                m_dioPusher.Write(true);
                int msTimeout = (1000 * m_dioPusher.m_secTimeout);
                while (m_dioPusher.p_bDone == false)
                {
                    Thread.Sleep(10);
                    if (m_dioPusher.m_swWrite.ElapsedMilliseconds > msTimeout) return "Run Push Timeout";
                    if (IsPusherOverload())
                    {
                        m_dioPusher.Write(false);
                        return "Run Pusher overload Check Error";
                    }
                }
                m_dioPusher.Write(false);
                Thread.Sleep(400); 
                if (Run(RunPusherDown(false))) return p_sInfo;
                if (Run(m_dioPusher.RunSol(false))) return p_sInfo;
                foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty;
                foreach (Line line in m_aLine)
                {
                    if (line.m_diCheck[2].p_bIn) return "Check Strip after Push"; 
                }
                m_storage.StartMoveMagazine(true);
                return "OK";
            }
            finally
            {
                m_dioPusherDown.Write(false);
                m_dioPusher.Write(false);
            }
        }

        bool IsPusherOverload()
        {
            for (int n = 0; n < 4; n++)
            {
                if (m_diPusherOverload.ReadDI(n)) return true; 
            }
            return false;
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
            foreach (Line line in m_aLine)
            {
                foreach (DIO_I di in line.m_diCheck)
                {
                    if (di.p_bIn)
                    {
                        m_alidHome.Run(true, "Check Strip");
                        return "Check Strip";
                    }
                }
            }
            m_axisRotate.ServoOn(true);
            p_sInfo = base.StateHome(m_axisWidth);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            m_alidHome.Run(p_sInfo != "OK", p_sInfo);
            m_dioPusher.Write(false);
            RunPusherDown(m_rinse.p_eMode == RinseU.eRunMode.Stack);
            return p_sInfo;
        }

        public override void Reset()
        {
            foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty; 
            base.Reset();
        }
        #endregion

        #region Run Run
        List<bool> m_bExist = new List<bool>(); 
        public string StartRun(List<bool> bExist)
        {
            m_bExist = bExist;
            while (m_bExist.Count < 4) m_bExist.Add(true); 
            StartRun(m_runRun); 
            return "OK";
        }

        double m_secWaitPush = 4;
        double m_secArrive = 2;
        double m_secArriveTimeout = 8;
        public string RunRun()
        {
            foreach (Line line in m_aLine) line.CheckInitRunSensor();
            StopWatch sw = new StopWatch();
            int msWaitPush = (int)(1000 * m_secWaitPush);
            if (Run(RunPusherDown(false))) return p_sInfo;
            RunRotate(true);
            while (IsExist() == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            RunRotate(true);
            while (sw.ElapsedMilliseconds < msWaitPush) Thread.Sleep(10);
            int nRotate = 0; 
            while (IsArrived() == false)
            {
                if (nRotate < (sw.ElapsedMilliseconds / 500))
                {
                    RunRotate(true);
                    nRotate++; 
                }
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            RunRotate(true);
            sw.Start();
            int msArriveTimeout = (int)(1000 * m_secArriveTimeout);
            while (IsReadyPush() == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
                if (sw.ElapsedMilliseconds > msArriveTimeout)
                {
                    m_alidArrived.Run(true, "Arrive Timeout"); 
                    RunRotate(false);
                    return "Arrive Timeout";
                }
            }
            Thread.Sleep((int)(1000 * m_secArrive));
            string sRun = RunPusher();
            if (sRun != "OK") m_alidPusher.Run(true, sRun); 
            RunRotate(false);
            return sRun;
        }

        bool IsExist()
        {
            foreach (Line line in m_aLine)
            {
                line.CheckSensor(); 
                if (line.p_eSensor != Line.eSensor.Empty) return true; 
            }
            return false; 
        }

        bool IsArrived()
        {
            foreach (Line line in m_aLine)
            {
                line.CheckSensor(); 
                switch (line.p_eSensor)
                {
                    case Line.eSensor.Exist: return false;
                }
            }
            return true;
        }

        public bool IsArriveOn()
        {
            foreach (Line line in m_aLine)
            {
                if (line.m_diCheck[2].p_bIn) return true; 
            }
            return false; 
        }

        bool IsReadyPush()
        {
            foreach (Line line in m_aLine)
            {
                line.CheckSensor();
                if (line.p_eSensor == Line.eSensor.Arrived) return false;
            }
            return true;
        }

        void RunTreePush(Tree tree)
        {
            m_secWaitPush = tree.Set(m_secWaitPush, m_secWaitPush, "Move", "Wait Push (sec)");
            m_secArrive = tree.Set(m_secArrive, m_secArrive, "Arrive", "Wait Arrive (sec)");
            m_secArriveTimeout = tree.Set(m_secArriveTimeout, m_secArriveTimeout, "Arrive Timeout", "Arrive Timeout (sec)");
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            RunTreePush(tree.GetTree("Time Wait")); 
            base.RunTree(tree);
        }
        #endregion

        RinseU m_rinse;
        Storage m_storage; 
        public Rail(string id, IEngineer engineer, RinseU rinse, Storage storage)
        {
            p_id = id;
            m_rinse = rinse;
            m_storage = storage; 
            InitLines();
            InitBase(id, engineer);
            EQ.m_EQ.OnChanged += M_EQ_OnChanged;
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            if (eEQ != _EQ.eEQ.State) return;
            if (value != EQ.eState.Run) RunRotate(false);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region StartRun
        public void StartRun()
        {
            switch (m_rinse.p_eMode)
            {
                case RinseU.eRunMode.Magazine:
                    RunMoveWidth(m_rinse.p_widthStrip);
                    if (RunPusherDown(false) != "OK") m_alidPusher.p_bSet = true; 
                    RunRotate(true);
                    break;
                case RinseU.eRunMode.Stack:
                    if (RunPusherDown(true) != "OK") m_alidPusher.p_bSet = true; 
                    RunRotate(false);
                    break;
            }
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runRun; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_MoveWidth(this), false, "Move Rail Width");
            AddModuleRunList(new Run_Rotate(this), false, "Rail Rotate");
            AddModuleRunList(new Run_PusherDown(this), false, "Pusher Down");
            AddModuleRunList(new Run_Pusher(this), false, "Pusher Down");
            m_runRun = AddModuleRunList(new Run_Run(this), false, "Rail Run");
        }

        public class Run_MoveWidth : ModuleRunBase
        {
            Rail m_module;
            public Run_MoveWidth(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_fWidth = 77;
            public override ModuleRunBase Clone()
            {
                Run_MoveWidth run = new Run_MoveWidth(m_module);
                run.m_fWidth = m_fWidth;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_fWidth = tree.Set(m_fWidth, m_fWidth, "Width", "Rail Width (mm)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunMoveWidth(m_fWidth);
            }
        }

        public class Run_Rotate : ModuleRunBase
        {
            Rail m_module;
            public Run_Rotate(Rail module)
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
                m_bRotate = tree.Set(m_bRotate, m_bRotate, "Rotate", "Rotate Rail", bVisible);
            }

            public override string Run()
            {
                return m_module.RunRotate(m_bRotate);
            }
        }

        public class Run_PusherDown : ModuleRunBase
        {
            Rail m_module;
            public Run_PusherDown(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bDown = false;
            bool m_bRepeat = false; 
            public override ModuleRunBase Clone()
            {
                Run_PusherDown run = new Run_PusherDown(m_module);
                run.m_bDown = m_bDown;
                run.m_bRepeat = m_bRepeat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRepeat = tree.Set(m_bRepeat, m_bRepeat, "Repeat", "Pusher Up Down Repeat", bVisible);
                m_bDown = tree.Set(m_bDown, m_bDown, "Down", "Pusher Down", bVisible && (m_bRepeat == false));
            }

            public override string Run()
            {
                bool bDown = true; 
                while (m_bRepeat)
                {
                    m_module.RunPusherDown(bDown);
                    Thread.Sleep(1000);
                    bDown = !bDown;
                    if (EQ.IsStop()) return "EQ Stop"; 
                }
                return m_module.RunPusherDown(m_bDown);
            }
        }

        public class Run_Pusher : ModuleRunBase
        {
            Rail m_module;
            public Run_Pusher(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bRepeat = false;
            public override ModuleRunBase Clone()
            {
                Run_Pusher run = new Run_Pusher(m_module);
                run.m_bRepeat = m_bRepeat;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bRepeat = tree.Set(m_bRepeat, m_bRepeat, "Repeat", "Pusher Up Down Repeat", bVisible);
            }

            public override string Run()
            {
                m_module.RunPusher();
                while (m_bRepeat)
                {
                    Thread.Sleep(1000);
                    m_module.RunPusher(); 
                    if (EQ.IsStop()) return "OK";
                }
                return "OK";
            }
        }
        public class Run_Run : ModuleRunBase
        {
            Rail m_module;
            public Run_Run(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunRun();
            }
        }
        #endregion

    }
}
