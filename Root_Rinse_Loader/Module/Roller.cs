using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Rinse_Loader.Module
{
    public class Roller : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axisRotate[0], this, "Rotate0");
            p_sInfo = m_toolBox.Get(ref m_axisRotate[1], this, "Rotate1");
            foreach (Line line in m_aLine) line.GetTools(m_toolBox);
            if (bInit)
            {
                m_axisRotate[1].SetSpeed("Jog", m_axisRotate[0].GetSpeedValue("Jog")); 
            }
        }
        #endregion

        #region Line
        public class Line
        {
            DIO_I[] m_diCheck = new DIO_I[2];
            public void GetTools(ToolBox toolBox)
            {
                m_roller.p_sInfo = toolBox.Get(ref m_diCheck[0], m_roller, m_id + ".Check0");
                m_roller.p_sInfo = toolBox.Get(ref m_diCheck[1], m_roller, m_id + ".Check1");
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
        #endregion

        #region Rotate
        Axis[] m_axisRotate = new Axis[2];

        public string RunRotate(bool bRotate)
        {
            if (bRotate)
            {
                m_axisRotate[0].Jog(m_rinse.p_fRotateSpeed);
                m_axisRotate[1].Jog(m_rinse.p_fRotateSpeed);
            }
            else
            {
                m_axisRotate[0].StopAxis();
                m_axisRotate[1].StopAxis();
            }
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
            m_axisRotate[0].ServoOn(true);
            m_axisRotate[1].ServoOn(true);
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
        }
        #endregion

        RinseL m_rinse;
        public Roller(string id, IEngineer engineer, RinseL rinse)
        {
            p_id = id;
            m_rinse = rinse;
            InitILines();
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
                case RinseL.eRunMode.Magazine:
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
            AddModuleRunList(new Run_Rotate(this), false, "Roller Rotate");
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
        #endregion
    }
}
