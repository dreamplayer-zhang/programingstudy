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
            p_sInfo = m_toolBox.GetDIO(ref m_diPusherOverload, this, "PusherOverload");
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
        ALID m_alidArrived;
        ALID m_alidPusher; 
        void InitALID()
        {
            m_alidArrived = m_gaf.GetALID(this, "Arrived", "Arrived Sensor Timeout");
            m_alidPusher = m_gaf.GetALID(this, "Pusher", "Pusher Error");
        }
        #endregion

        #region Line
        public class Line : NotifyProperty
        {
            DIO_I[] m_diCheck = new DIO_I[3];
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
                        if (m_diCheck[2].p_bIn)
                        {
                            p_eSensor = eSensor.Arrived;
                        }
                        break;
                }
                return "OK";
            }

            public bool IsArriveDone()
            {
                return m_diCheck[1].p_bIn == false; 
            }

            string m_id;
            Rail m_rail;
            public Line(string id, Rail rail)
            {
                m_id = id;
                m_rail = rail;
            }
        }

        List<Line> m_aLine = new List<Line>();
        void InitLines()
        {
            for (int n = 0; n < 4; n++) m_aLine.Add(new Line("Line" + n.ToString(), this));
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
            if (bRotate) m_axisRotate.Jog(m_rinse.p_fRotateSpeed);
            else m_axisRotate.StopAxis(); 
            return "OK";
        }
        #endregion

        #region Pusher
        DIO_I2O m_dioPusher;
        DIO_I2O2 m_dioPusherDown;
        DIO_I m_diPusherOverload; 

        public string RunPusherDown(bool bDown)
        {
            return m_dioPusherDown.RunSol(bDown); 
        }

        public string RunPusher()
        {
            try
            {
                if (Run(m_storage.RunMoveMagazine())) return p_sInfo; 
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
                    if (m_diPusherOverload.p_bIn)
                    {
                        m_dioPusher.Write(false);
                        return "Run Pusher overload Check Error";
                    }
                }
                if (Run(m_dioPusher.RunSol(false))) return p_sInfo;
                if (Run(RunPusherDown(false))) return p_sInfo;
                foreach (Line line in m_aLine) line.p_eSensor = Line.eSensor.Empty;
                m_storage.StartMoveNextMagazine();
                return "OK";
            }
            finally
            {
                m_dioPusherDown.Write(false);
                m_dioPusher.Write(false); 
            }
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
            p_sInfo = base.StateHome(m_axisWidth);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region Run Run
        List<bool> m_bExist; 
        public string StartRun(List<bool> bExist)
        {
            m_bExist = bExist; 
            StartRun(m_runRun); 
            return "OK";
        }

        public string RunRun(double secArrive)
        {
            while (IsExist() == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop"; 
            }
            while (IsArrived() == false)
            {
                Thread.Sleep(10);
                if (EQ.IsStop()) return "EQ Stop";
            }
            Thread.Sleep((int)(1000 * secArrive));
            foreach (Line line in m_aLine)
            {
                if (line.IsArriveDone() == false)
                {
                    m_alidArrived.p_bSet = true; 
                    return "Arrive Done Error";
                }
            }
            string sRun = RunPusher();
            m_alidPusher.p_bSet = (sRun != "OK"); 
            return sRun; 
        }

        bool IsExist()
        {
            int[] nExist = { 0, 0 }; 
            for (int n = 0; n < m_bExist.Count; n++)
            {
                m_aLine[n].CheckSensor();
                if (m_bExist[n])
                {
                    nExist[0]++;
                    if (m_aLine[n].p_eSensor != Line.eSensor.Empty) nExist[1]++; 
                }
            }
            return nExist[0] == nExist[1];
        }

        bool IsArrived()
        {
            int[] nExist = { 0, 0 };
            for (int n = 0; n < m_bExist.Count; n++)
            {
                m_aLine[n].CheckSensor();
                if (m_bExist[n])
                {
                    nExist[0]++;
                    if (m_aLine[n].p_eSensor == Line.eSensor.Arrived) nExist[1]++;
                }
            }
            return nExist[0] == nExist[1];
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
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
                    RunPusherDown(false);
                    RunRotate(true);
                    break;
                case RinseU.eRunMode.Stack:
                    RunPusherDown(true); 
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

        public class Run_Run : ModuleRunBase
        {
            Rail m_module;
            public Run_Run(Rail module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            double m_secArrive = 2;
            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                run.m_secArrive = m_secArrive; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_secArrive = tree.Set(m_secArrive, m_secArrive, "Arrive", "Arrive Delay (sec)", bVisible); 
            }

            public override string Run()
            {
                return m_module.RunRun(m_secArrive);
            }
        }
        #endregion

    }
}
