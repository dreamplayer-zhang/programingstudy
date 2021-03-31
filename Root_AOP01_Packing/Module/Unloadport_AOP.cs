using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class Unloadport_AOP : ModuleBase, IWTRChild
    {
        ALID m_alid_WaferExist;
        public void SetAlarm()
        {
            m_alid_WaferExist.Run(true, "Wafer Exist Error");
        }
        #region ToolBox
        Axis m_axis;
        DIO_I[] m_diDoor = new DIO_I[2];
        DIO_I[] m_diCheck = new DIO_I[3]; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetAxis(ref m_axis, this, "Door");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoor[0], this, "Door Close");
            p_sInfo = m_toolBox.GetDIO(ref m_diDoor[1], this, "Door Open");
            p_sInfo = m_toolBox.GetDIO(ref m_diCheck[0], this, "Check0");
            p_sInfo = m_toolBox.GetDIO(ref m_diCheck[1], this, "Check1");
            p_sInfo = m_toolBox.GetDIO(ref m_diCheck[2], this, "Check2");
            if (bInit)
            {
                InitPos(); 
            }
        }
        #endregion

        #region Check
        public enum eCheck
        {
            Empty,
            Exist,
            Check
        }
        eCheck _eCheck = eCheck.Check; 
        public eCheck p_eCheck
        {
            get { return _eCheck; }
            set
            {
                if (_eCheck == value) return;
                _eCheck = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region Door
        public enum eDoor
        {
            Running,
            Close,
            Open,
            Error
        }
        eDoor _eDoor = eDoor.Running; 
        public eDoor p_eDoor
        {
            get { return _eDoor; }
            set
            {
                if (_eDoor == value) return;
                _eDoor = value;
                OnPropertyChanged(); 
            }
        }

        public enum ePos
        {
            Close,
            Open
        }
        void InitPos()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }


        public string RunDoor(ePos ePos)
        {
            m_axis.StartMove(ePos);
            string sRun = m_axis.WaitReady();
            if (sRun != "OK") return sRun;
            Thread.Sleep(100); 
            switch (ePos)
            {
                case ePos.Close: if (p_eDoor != eDoor.Close) return "Door Close Sensor Error"; break;
                case ePos.Open: if (p_eDoor != eDoor.Open) return "Door Open Sensor Error"; break;
            }
            return "OK"; 
        }
        #endregion

        #region Thread Check
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThread()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start(); 
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(3000); 
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                int nDoor = m_diDoor[1].p_bIn ? 2 : 0;
                if (m_diDoor[0].p_bIn) nDoor++;
                p_eDoor = (eDoor)nDoor;
                int nCheck = 0; 
                for (int n = 0; n < 3; n++)
                {
                    if (m_diCheck[n].p_bIn) nCheck++;
                }
                switch (nCheck)
                {
                    case 0: p_eCheck = eCheck.Empty; break;
                    case 3: p_eCheck = eCheck.Exist; break;
                    default: p_eCheck = eCheck.Check; break; 
                }
            }
        }
        #endregion

        #region InfoWafer
        public InfoWafer p_infoWafer { get; set; }
        public void ReadInfoWafer_Registry() { }
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

        public List<string> p_asChildSlot { get { return null; } }

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
            return "Not Enable"; 
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            if (p_eCheck != eCheck.Empty) return p_id + " IsPutOK : Check Sensor Detected"; 
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null) infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            return RunDoor(ePos.Open); 
        }

        public string AfterGet(int nID)
        {
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            return RunDoor(ePos.Close); 
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return "OK";
        }

        public bool IsWaferExist(int nID)
        {
            return (p_eCheck != eCheck.Empty); 
        }

        InfoWafer.WaferSize m_waferSize;
        public void RunTreeTeach(Tree tree)
        {
            m_waferSize.RunTreeTeach(tree.GetTree(p_id, false));
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
            m_waferSize.RunTree(tree.GetTree("Wafer Size", false), true);
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

        public Unloadport_AOP(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitThread();
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

        #region ModuleRun
        ModuleRunBase m_runDoor; 
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            m_runDoor = AddModuleRunList(new Run_Door(this), true, "Door Open Close");
        }

        public class Run_Delay : ModuleRunBase
        {
            Unloadport_AOP m_module;
            public Run_Delay(Unloadport_AOP module)
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

        public class Run_Door : ModuleRunBase
        {
            Unloadport_AOP m_module;
            public Run_Door(Unloadport_AOP module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public ePos m_ePos = ePos.Close; 
            public override ModuleRunBase Clone()
            {
                Run_Door run = new Run_Door(m_module);
                run.m_ePos = m_ePos;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_ePos = (ePos)tree.Set(m_ePos, m_ePos, "Door", "Door Open", bVisible);
            }

            public override string Run()
            {
                return m_module.RunDoor(m_ePos); 
            }
        }
        #endregion

    }
}
