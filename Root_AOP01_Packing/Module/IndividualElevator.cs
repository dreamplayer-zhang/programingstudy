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
    public class IndividualElevator : ModuleBase, IWTRChild
    {
        public IndividualElevator(string id, IEngineer engineer)
        {
            m_waferSize = new InfoWafer.WaferSize(id, false, false);
            base.InitBase(id, engineer);
            InitThreadCheck();
            InitALID();
        }

        #region ToolBox
        Axis m_axis;
        DIO_I[] m_diCheck = new DIO_I[3];
        DIO_I[] m_diProtection = new DIO_I[2];
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Elevator");
            p_sInfo = m_toolBox.Get(ref m_diCheck[0], this, "Check 0");
            p_sInfo = m_toolBox.Get(ref m_diCheck[1], this, "Check 1");
            p_sInfo = m_toolBox.Get(ref m_diCheck[2], this, "Check 2");
            p_sInfo = m_toolBox.Get(ref m_diProtection[0], this, "Protection 0");
            p_sInfo = m_toolBox.Get(ref m_diProtection[1], this, "Protection 1");
            if (bInit)
            {
                InitPosElevator();
            }
        }
        #endregion

        #region ALID
        ALID m_alidElevator;
        void InitALID() 
        {
            m_alidElevator = m_gaf.GetALID(this, "Elevator Module", "Elevator Module Error");
        }
        #endregion

        #region Sensor
        public bool p_bCheck
        {
            get
            {
                return _bCheck;
            }
            set
            {
                if (_bCheck == value)
                    return;
                _bCheck = value;
                OnPropertyChanged();
            }
        }
        bool _bCheck = false; 

        public bool p_bProtection
        {
            get
            {
                return _bProtection;
            }
            set
            {
                if (_bProtection == value)
                    return;
                _bProtection = value;
                m_axis.StopAxis();
                p_infoWafer = null;
                OnPropertyChanged();
            }
        }
        bool _bProtection = false; 

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
            Thread.Sleep(3000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                p_bCheck = (m_diCheck[0].p_bIn && m_diCheck[1].p_bIn && m_diCheck[2].p_bIn);
                p_bProtection = (m_diProtection[0].p_bIn || m_diProtection[1].p_bIn); 

                if(p_bProtection)
                {
                    this.p_eState = eState.Error;
                    EQ.p_bStop = true;
                    m_alidElevator.Run(p_bProtection, "Please Check The Individual Elevator");

                }

            }
        }
        #endregion

        #region Elevator
        public enum ePos
        {
            Top,
            Bottom
        }
        void InitPosElevator()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string MoveElevator(int nLevel)
        {
            p_infoWafer = null;
            m_axis.StartMove(GetPos(nLevel));
            return m_axis.WaitReady();
        }

        double GetPos(int nLevel)
        {
            if (nLevel < 0)
                nLevel = 0;
            if (nLevel > 7)
                nLevel = 7;
            double pos0 = m_axis.GetPosValue(ePos.Bottom);
            double pos7 = m_axis.GetPosValue(ePos.Top);
            return nLevel * (pos7 - pos0) / 7 + pos0;
        }
        #endregion

        #region Mapping


        
        public string RunMapping()
        {
            p_infoWafer = null;
            // State Home에 프로텍션 체크넣어야됨
            
            if (Run(m_axis.StartMove(ePos.Bottom))) return p_sInfo;
            if (Run(m_axis.WaitReady())) return p_sInfo;
            for(int n=0; n<8; n++)
            {
                Thread.Sleep(200);
                if (p_bCheck) 
                {
                    if(Run(m_axis.StartShift(65000))) return p_sInfo;
                    if(Run(m_axis.WaitReady())) return p_sInfo;
                    p_infoWafer = new InfoWafer(p_id, 0, m_engineer);
                    return "OK";
                }
                Run(m_axis.StartShift(65000));
                Run(m_axis.WaitReady());
                //m_axis.StartMove()
            }
            return "No Case";          
        }
        #endregion

        #region InfoWafer
        InfoWafer _infoWafer = null;
        public InfoWafer p_infoWafer
        {
            get
            {
                return _infoWafer;
            }
            set
            {
                _infoWafer = value;
                OnPropertyChanged();
            }
        }

        public void ReadInfoWafer_Registry()
        {
        }
        #endregion

        #region IWTRChild
        bool _bLock = false;
        public bool p_bLock
        {
            get
            {
                return _bLock;
            }
            set
            {
                if (_bLock == value)
                    return;
                _bLock = value;
            }
        }

        bool IsLock()
        {
            for (int n = 0; n < 10; n++)
            {
                if (p_bLock == false)
                    return false;
                Thread.Sleep(100);
            }
            return true;
        }

        public List<string> p_asChildSlot
        {
            get
            {
                return null;
            }
        }

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
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer == null)
                return p_id + " IsGetOK - InfoWafer not Exist";
            return "OK";
        }

        public string IsPutOK(InfoWafer infoWafer, int nID)
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            if (p_infoWafer != null)
                return p_id + " IsPutOK - InfoWafer Exist";
            if (m_waferSize.GetData(infoWafer.p_eSize).m_bEnable == false)
                return p_id + " not Enable Wafer Size";
            return "OK";
        }

        public int GetTeachWTR(InfoWafer infoWafer = null)
        {
            if (infoWafer == null)
                infoWafer = p_infoWafer;
            return m_waferSize.GetData(infoWafer.p_eSize).m_teachWTR;
        }

        public string BeforeGet(int nID)
        {
            if (p_infoWafer == null)
                return p_id + " BeforeGet : InfoWafer = null";
            return CheckGetPut();
        }

        public string BeforePut(int nID)
        {
            if (p_infoWafer != null)
                return p_id + " BeforePut : InfoWafer != null";
            return CheckGetPut();
        }

        public string AfterGet(int nID)
        {
            return CheckGetPut();
        }

        public string AfterPut(int nID)
        {
            return "OK";
        }

        string CheckGetPut()
        {
            if (p_eState != eState.Ready)
                return p_id + " eState not Ready";
            return "OK";
        }

        public bool IsWaferExist(int nID)
        {
            return (p_infoWafer != null);
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
            p_infoWafer = null;
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }
        #endregion

       

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
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Delay(this), true, "Just Time Delay");
            AddModuleRunList(new Run_Mapping(this), true, "Mapping");
        }

        public class Run_Delay : ModuleRunBase
        {
            IndividualElevator m_module;
            public Run_Delay(IndividualElevator module)
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

        public class Run_Mapping : ModuleRunBase
        {
            IndividualElevator m_module;
            public Run_Mapping(IndividualElevator module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Mapping run = new Run_Mapping(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {

            }

            public override string Run()
            {
                return RunMapping();
            }
            public string RunMapping()
            {
                m_module.p_infoWafer = null;
                // State Home에 프로텍션 체크넣어야됨

                if (m_module.Run(m_module.m_axis.StartMove(ePos.Bottom)))
                    return p_sInfo;
                p_nProgress += 10;
                if (m_module.Run(m_module.m_axis.WaitReady()))
                    return p_sInfo;
                p_nProgress += 10;
                for (int n = 0; n < 8; n++)
                {
                    Thread.Sleep(200);
                    if (m_module.p_bCheck)
                    {
                        p_nProgress = 80;
                        if (m_module.Run(m_module.m_axis.StartShift(65000)))
                            return p_sInfo;
                        p_nProgress = 90;
                        if (m_module.Run(m_module.m_axis.WaitReady()))
                            return p_sInfo;
                        p_nProgress = 100;
                        m_module.p_infoWafer = new InfoWafer(p_id, 0, m_module.m_engineer);
                        return "OK";
                    }
                    m_module.Run(m_module.m_axis.StartShift(65000));
                    p_nProgress += 5;
                    m_module.Run(m_module.m_axis.WaitReady());
                    p_nProgress += 5;
                }
                return "No Case";
            }
        }
        #endregion

    }
}

