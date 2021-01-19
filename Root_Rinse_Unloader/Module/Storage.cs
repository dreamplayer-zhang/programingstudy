﻿using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class Storage : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            foreach (Magazine magazine in m_aMagazine) magazine.GetTools(m_toolBox);
            m_stack.GetTools(m_toolBox);
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Elevator");
            if (bInit)
            {
                InitPosElevator();
            }
        }
        #endregion

        #region Magazine
        public class Magazine : NotifyProperty
        {
            DIO_I m_diCheck;
            DIO_IO m_dioClamp;
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck, m_storage, m_id + ".Check");
                m_storage.p_sInfo = toolBox.Get(ref m_dioClamp, m_storage, m_id + ".Clamp");
            }

            bool _bCheck = false;
            public bool p_bCheck
            {
                get { return _bCheck; }
                set
                {
                    if (_bCheck == value) return;
                    _bCheck = value;
                    OnPropertyChanged();
                }
            }

            bool _bClamp = false;
            public bool p_bClamp
            {
                get { return _bClamp; }
                set
                {
                    if (_bClamp == value) return;
                    _bClamp = value;
                    OnPropertyChanged();
                }
            }

            public void CheckSensor()
            {
                p_bCheck = m_diCheck.p_bIn;
                p_bClamp = !m_dioClamp.p_bIn;
            }

            public void RunClamp(bool bClamp)
            {
                m_dioClamp.Write(bClamp && p_bCheck);
            }

            string m_id;
            Storage m_storage;
            public Magazine(string id, Storage storage)
            {
                m_id = id;
                m_storage = storage;
            }
        }

        List<Magazine> m_aMagazine = new List<Magazine>();
        void InitMagazine()
        {
            for (int n = 0; n < 4; n++) m_aMagazine.Add(new Magazine("Magazine" + n.ToString(), this));
        }

        public string RunClamp(bool bClamp, double secDelay)
        {
            foreach (Magazine magazine in m_aMagazine) magazine.RunClamp(bClamp);
            Thread.Sleep((int)secDelay);
            foreach (Magazine magazine in m_aMagazine)
            {
                if (magazine.p_bCheck && (magazine.p_bClamp != bClamp)) return "Clamp Sensor Error";
            }
            return "OK";
        }
        #endregion

        #region Stack
        public class Stack : NotifyProperty
        {
            DIO_I m_diLevel;
            DIO_I[] m_diCheck = new DIO_I[4];
            public void GetTools(ToolBox toolBox)
            {
                m_storage.p_sInfo = toolBox.Get(ref m_diLevel, m_storage, m_id + ".Level");
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck[0], m_storage, m_id + ".Check0");
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck[1], m_storage, m_id + ".Check1");
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck[2], m_storage, m_id + ".Check2");
                m_storage.p_sInfo = toolBox.Get(ref m_diCheck[3], m_storage, m_id + ".Check3");
            }

            bool _bLevel = false;
            public bool p_bLevel
            {
                get { return _bLevel; }
                set
                {
                    if (_bLevel == value) return;
                    _bLevel = value;
                    OnPropertyChanged();
                }
            }

            bool _bCheck = false;
            public bool p_bCheck
            {
                get { return _bCheck; }
                set
                {
                    if (_bCheck == value) return;
                    _bCheck = value;
                    OnPropertyChanged();
                }
            }

            public void CheckSensor()
            {
                p_bLevel = m_diLevel.p_bIn;
                p_bCheck = m_diCheck[0].p_bIn || m_diCheck[1].p_bIn || m_diCheck[2].p_bIn || m_diCheck[3].p_bIn;
            }

            string m_id;
            Storage m_storage;
            public Stack(string id, Storage storage)
            {
                m_id = id;
                m_storage = storage;
            }
        }
        Stack m_stack;
        void InitStack()
        {
            m_stack = new Stack("Stack", this);
        }
        #endregion

        #region Elevator
        Axis m_axis;
        public enum ePos
        {
            Magazine0,
            Magazine1,
            Magazine2,
            Magazine3,
            Stack
        }
        void InitPosElevator()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        int m_dZ = 6;
        public string MoveMagazine(ePos ePos, int iIndex)
        {
            if ((iIndex < 0) || (iIndex >= 20)) return "Invalid Index";
            m_axis.StartMove(ePos, iIndex * m_dZ);
            return m_axis.WaitReady();
        }

        public string MoveStack()
        {
            m_axis.StartMove(ePos.Stack);
            return m_axis.WaitReady();
        }

        double m_posStackReady = 0;
        double m_fJogScale = 1;
        public string MoveStackReady()
        {
            if (m_posStackReady != m_axis.p_posCommand) MoveStack();
            if (m_stack.p_bLevel)
            {
                m_axis.Jog(-m_fJogScale);
                while (m_stack.p_bLevel && (EQ.IsStop() == false)) Thread.Sleep(10);
            }
            m_axis.Jog(m_fJogScale);
            while (!m_stack.p_bLevel && (EQ.IsStop() == false)) Thread.Sleep(10);
            m_posStackReady = m_axis.p_posCommand;
            return "OK";
        }

        public bool p_bIsEnablePick
        {
            get { return Math.Abs(m_posStackReady - m_axis.p_posCommand) < 10; }
        }

        void RunTreeElevator(Tree tree)
        {
            m_dZ = tree.Set(m_dZ, m_dZ, "dZ", "Magazine Slot Pitch (pulse)");
            m_fJogScale = tree.Set(m_fJogScale, m_fJogScale, "Jog Scale", "Jog Move Scale (0 ~ 1)");
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
                foreach (Magazine magazine in m_aMagazine) magazine.CheckSensor();
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
            p_sInfo = base.StateHome();
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
            RunTreeElevator(tree.GetTree("Elevator"));
        }
        #endregion

        RinseU m_rinse;
        public Storage(string id, IEngineer engineer, RinseU rinse)
        {
            p_id = id;
            m_rinse = rinse;
            InitMagazine();
            InitStack();
            InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
        }

        #region ModuleRun
        public string StartMoveStackReady()
        {
            StartRun(m_runReady.Clone());
            return "OK";
        }

        public string StartMoveNextMagazine()
        {
            Run_RunNextMagazine run = (Run_RunNextMagazine)m_runNextMagazine.Clone();
            return StartRun(run); //forget
        }

        public string RunNextMagazine()
        {
            m_rinse.p_iMagazine++; //forget Check Magagine
            ePos eMGZ = (ePos)(m_rinse.p_iMagazine / 20);
            int iMGZ = m_rinse.p_iMagazine % 20;
            return MoveMagazine(eMGZ, iMGZ); 
        }

        ModuleRunBase m_runReady;
        ModuleRunBase m_runNextMagazine;
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_MovePos(this), false, "Move Elevator Position");
            AddModuleRunList(new Run_Clamp(this), false, "Run Clamp");
            m_runReady = AddModuleRunList(new Run_StackReady(this), false, "Run Stack Move Ready Position");
            m_runNextMagazine = AddModuleRunList(new Run_RunNextMagazine(this), false, "Run Magazine");
        }

        public class Run_MovePos : ModuleRunBase
        {
            Storage m_module;
            public Run_MovePos(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            ePos m_ePos = ePos.Stack;
            int m_iIndex = 0;
            public override ModuleRunBase Clone()
            {
                Run_MovePos run = new Run_MovePos(m_module);
                run.m_ePos = m_ePos;
                run.m_iIndex = m_iIndex;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Pos", "Elevator Position", bVisible);
                m_iIndex = tree.Set(m_iIndex, m_iIndex, "Index", "Magazine Index", bVisible && (m_ePos != ePos.Stack));
            }

            public override string Run()
            {
                if (m_ePos == ePos.Stack) return m_module.MoveStack();
                return m_module.MoveMagazine(m_ePos, m_iIndex);
            }
        }

        public class Run_Clamp : ModuleRunBase
        {
            Storage m_module;
            public Run_Clamp(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bClamp = false;
            double m_secDelay = 4;
            public override ModuleRunBase Clone()
            {
                Run_Clamp run = new Run_Clamp(m_module);
                run.m_bClamp = m_bClamp;
                run.m_secDelay = m_secDelay;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bClamp = tree.Set(m_bClamp, m_bClamp, "Clamp", "Run Clamp", bVisible);
                m_secDelay = tree.Set(m_secDelay, m_secDelay, "Delay", "Run Clamp Delay (sec)", bVisible);
            }

            public override string Run()
            {
                return m_module.RunClamp(m_bClamp, m_secDelay);
            }
        }

        public class Run_StackReady : ModuleRunBase
        {
            Storage m_module;
            public Run_StackReady(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_StackReady run = new Run_StackReady(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.MoveStackReady();
            }
        }

        public class Run_RunNextMagazine : ModuleRunBase
        {
            Storage m_module;
            public Run_RunNextMagazine(Storage module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_RunNextMagazine run = new Run_RunNextMagazine(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunNextMagazine();
            }
        }
        #endregion
    }
}