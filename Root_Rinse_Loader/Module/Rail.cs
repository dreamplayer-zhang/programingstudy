using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Loader.Module
{
    public class Rail : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axisRotate, this, "Rotate");
            p_sInfo = m_toolBox.GetAxis(ref m_axisWidth, this, "Width");
            foreach (Line line in m_aLine) line.GetTools(m_toolBox);
            if (bInit)
            {
                InitPosWidth();
            }
        }
        #endregion

        #region GAF
        ALID m_alidAxis;
        void InitALID()
        {
            m_alidAxis = m_gaf.GetALID(this, "Rotate Axis Alarm", "Rotate Axis Alarm");
        }
        #endregion

        #region Line
        public class Line
        {
            public DIO_I[] m_diCheck = new DIO_I[3]; 
            public void GetTools(ToolBox toolBox)
            {
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[0], m_rail, m_id + ".Start");
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[1], m_rail, m_id + ".Mid");
                m_rail.p_sInfo = toolBox.GetDIO(ref m_diCheck[2], m_rail, m_id + ".Arrived");
            }

            public bool m_bExist = false; 
            public void CheckSensor()
            {
                if (m_diCheck[0].p_bIn) m_bExist = true; 
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

        public void CheckStrip(bool bCheck)
        {
            if (bCheck)
            {
                string sSend = "";
                foreach (Line line in m_aLine) sSend += line.m_bExist ? 'O' : '.';
                m_rinse.AddStripSend(sSend);
            }
            foreach (Line line in m_aLine) line.m_bExist = false;
        }

        public bool IsStartOn()
        {
            foreach (Line line in m_aLine)
            {
                if (line.m_diCheck[2].p_bIn) return true;
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
            if (bRotate) m_axisRotate.Jog(m_rinse.p_fRotateSpeed, "Move");
            else m_axisRotate.StopAxis(); 
            return "OK"; 
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
                for (int n = 0; n < 2; n++) if (line.m_diCheck[n].p_bIn) return "Check Strip";
            }
            m_axisRotate.ServoOn(true); 
            p_sInfo = base.StateHome(m_axisWidth);
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            RunRotate(false); 
            base.Reset();
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
                m_alidAxis.p_bSet = m_axisRotate.p_sensorAlarm; 
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }
        #endregion

        RinseL m_rinse; 
        public Rail(string id, IEngineer engineer, RinseL rinse)
        {
            p_id = id;
            m_rinse = rinse;
            InitALID(); 
            InitLines(); 
            InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            RunRotate(false);
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
            switch (m_rinse.p_eMode)
            {
                case RinseL.eRunMode.Magazine:
                    RunMoveWidth(m_rinse.p_widthStrip);
                    RunRotate(true); 
                    break;
                case RinseL.eRunMode.Stack:
                    RunRotate(false); 
                    break; 
            }
        }
        #endregion

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_MoveWidth(this), false, "Move Rail Width");
            AddModuleRunList(new Run_Rotate(this), false, "Rail Rotate");
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
        #endregion
    }
}
