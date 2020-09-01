using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root.Module
{
    public class TestMars : ModuleBase
    {
        #region ToolBox
        TCPIPClient m_tcpEFEM;
        TCPIPClient m_tcpVision;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpEFEM, this, "TCP EFEM");
            p_sInfo = m_toolBox.Get(ref m_tcpVision, this, "TCP Vision");
        }
        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        #region DateTime
        public string p_sDateTime
        {
            get
            {
                DateTime dt = DateTime.Now;
                string sTime = dt.Hour.ToString("00:") + dt.Minute.ToString("00:") + dt.Second.ToString("00") + "." + dt.Millisecond.ToString("000");
                return dt.Year.ToString("0000/") + dt.Month.ToString("00/") + dt.Day.ToString("00\t") + sTime; 
            }
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeSetup(tree.GetTree("Setup", false));
        }

        void RunTreeSetup(Tree tree)
        {
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate) return "OK";
            p_eState = eState.Ready;
            return "OK";
        }
        #endregion

        public TestMars(string id, IEngineer engineer)
        {
            InitBase(id, engineer); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_SendPRC(this), false, "Send PRC Commend");
            AddModuleRunList(new Run_SendXFR(this), false, "Send XFR Commend");
            AddModuleRunList(new Run_SendFNC(this), false, "Send FNC Commend");
            AddModuleRunList(new Run_SendLEH(this), false, "Send LEH Commend");
            AddModuleRunList(new Run_SendCFG(this), false, "Send CFG Commend");
            AddModuleRunList(new Run_SendReset(this), false, "Send Reset Commend");
            AddModuleRunList(new Run_SendVision(this), false, "Send Vision Commend");
        }

        public class Run_SendPRC : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendPRC(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
                m_asLog[0] = "2020/08/21";
                m_asLog[1] = "09:02:45.123";
                m_asLog[2] = "'Aligner'";
                m_asLog[3] = "'PRC'";
                m_asLog[4] = "'Process'";
                m_asLog[5] = "'Start'";
                m_asLog[6] = "'Material'";
                m_asLog[7] = "'Wafer'";
                m_asLog[8] = "5";
                m_asLog[9] = "'Lot ID'";
                m_asLog[10] = "'Recipe ID'";
                m_asLog[11] = "0";
                m_asLog[12] = "0";
                m_asLog[13] = "'Step Name'";
                m_asLog[14] = "$";
            }

            bool m_bStart = true; 
            string[] m_asLog = new string[15]; 
            public override ModuleRunBase Clone()
            {
                Run_SendPRC run = new Run_SendPRC(m_module);
                for (int n = 0; n < 15; n++) run.m_asLog[n] = m_asLog[n];
                run.m_bStart = m_bStart; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bStart = tree.Set(m_bStart, m_bStart, "Start", "Status", bVisible);
                m_asLog[5] = m_bStart ? "'Start'" : "'End'";
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                for (int n = 2; n < 15; n++) sLog += "\t" + m_asLog[n];
                m_module.m_tcpEFEM.Send(sLog); 
                return "OK";
            }
        }

        public class Run_SendXFR : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendXFR(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
                m_asLog[0] = "2020/08/21";
                m_asLog[1] = "09:02:45.123";
                m_asLog[2] = "'WTR'";
                m_asLog[3] = "'XFR'";
                m_asLog[4] = "'Get'";
                m_asLog[5] = "'Start'";
                m_asLog[6] = "'Material'";
                m_asLog[7] = "'Wafer'";
                m_asLog[8] = "'Lot ID'";
                m_asLog[9] = "'Aligner'";
                m_asLog[10] = "1";
                m_asLog[11] = "'UpperArm'";
                m_asLog[12] = "1";
                m_asLog[13] = "$";
            }

            bool m_bStart = true;
            string[] m_asLog = new string[14];
            public override ModuleRunBase Clone()
            {
                Run_SendXFR run = new Run_SendXFR(m_module);
                for (int n = 0; n < 14; n++) run.m_asLog[n] = m_asLog[n];
                run.m_bStart = m_bStart;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bStart = tree.Set(m_bStart, m_bStart, "Start", "Status", bVisible);
                m_asLog[5] = m_bStart ? "'Start'" : "'End'";
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                for (int n = 2; n < 14; n++) sLog += "\t" + m_asLog[n];
                m_module.m_tcpEFEM.Send(sLog);
                return "OK";
            }
        }

        public class Run_SendFNC : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendFNC(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
                m_asLog[0] = "2020/08/21";
                m_asLog[1] = "09:02:45.123";
                m_asLog[2] = "'Aligner'";
                m_asLog[3] = "'FNC'";
                m_asLog[4] = "'MoveAxisZ'";
                m_asLog[5] = "'Start'";
                m_asLog[6] = "'Material'";
                m_asLog[7] = "'Wafer'";
                m_asLog[8] = "$";
            }

            bool m_bStart = true;
            string[] m_asLog = new string[15];
            public override ModuleRunBase Clone()
            {
                Run_SendFNC run = new Run_SendFNC(m_module);
                for (int n = 0; n < 9; n++) run.m_asLog[n] = m_asLog[n];
                run.m_bStart = m_bStart;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bStart = tree.Set(m_bStart, m_bStart, "Start", "Status", bVisible);
                m_asLog[5] = m_bStart ? "'Start'" : "'End'";
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                for (int n = 2; n < 9; n++) sLog += "\t" + m_asLog[n];
                m_module.m_tcpEFEM.Send(sLog);
                return "OK";
            }
        }

        public class Run_SendLEH : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendLEH(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
                m_asLog[0] = "2020/08/21";
                m_asLog[1] = "09:02:45.123";
                m_asLog[2] = "'Aligner'";
                m_asLog[3] = "'LEH'";
                m_asLog[4] = "'CarrierLoad'";
                m_asLog[5] = "'Lot ID'";
                m_asLog[6] = "'Recipe ID'";
                m_asLog[7] = "'Flow Info'";
                m_asLog[8] = "'Carrier ID'";
                m_asLog[9] = "$";
            }

            enum eEvent
            {
                CarrierLoad,
                ProcessJobStart,
                ProcessJobEnd,
                CarrierUnload
            }

            eEvent m_eEvent = eEvent.CarrierLoad;
            string[] m_asLog = new string[15];
            public override ModuleRunBase Clone()
            {
                Run_SendLEH run = new Run_SendLEH(m_module);
                for (int n = 0; n < 10; n++) run.m_asLog[n] = m_asLog[n];
                run.m_eEvent = m_eEvent;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eEvent = (eEvent)tree.Set(m_eEvent, m_eEvent, "Event", "Status", bVisible);
                m_asLog[4] = '\'' + m_eEvent.ToString() + '\''; 
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                for (int n = 2; n < 10; n++) sLog += "\t" + m_asLog[n];
                m_module.m_tcpEFEM.Send(sLog);
                return "OK";
            }
        }

        public class Run_SendCFG : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendCFG(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
                m_asLog[0] = "2020/08/21";
                m_asLog[1] = "09:02:45.123";
                m_asLog[2] = "'Aligner'";
                m_asLog[3] = "'CFG'";
                m_asLog[4] = "'Category'";
                m_asLog[5] = "'Config ID'";
                m_asLog[6] = "'Value'";
                m_asLog[7] = "'Unit'";
                m_asLog[8] = "'ECID'";
                m_asLog[9] = "$";
            }

            enum eEvent
            {
                CarrierLoad,
                ProcessJobStart,
                ProcessJobEnd,
                CarrierUnload
            }

            string[] m_asLog = new string[15];
            public override ModuleRunBase Clone()
            {
                Run_SendCFG run = new Run_SendCFG(m_module);
                for (int n = 0; n < 10; n++) run.m_asLog[n] = m_asLog[n];
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                for (int n = 2; n < 10; n++) sLog += "\t" + m_asLog[n];
                m_module.m_tcpEFEM.Send(sLog);
                return "OK";
            }
        }

        public class Run_SendReset : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendReset(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_SendReset run = new Run_SendReset(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                string sLog = m_module.p_sDateTime;
                m_module.m_tcpEFEM.Send(sLog + "\t0\tReset");
                return "OK";
            }
        }

        public class Run_SendVision : ModuleRunBase
        {
            TestMars m_module;
            public Run_SendVision(TestMars module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            enum eSend
            {
                Start,
                End,
                Reset
            }
            eSend m_eSend = eSend.Start; 
            public override ModuleRunBase Clone()
            {
                Run_SendVision run = new Run_SendVision(m_module);
                run.m_eSend = m_eSend; 
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_eSend = (eSend)tree.Set(m_eSend, m_eSend, "Send", "Send Cmd", bVisible);
            }

            public override string Run()
            {
                switch (m_eSend)
                {
                    case eSend.Start: m_module.m_tcpVision.Send("ModuleID:Vision,LogType:PRC,EventID:Process,Status:Start,Time:" + m_module.p_sDateTime + ",WaferID:logtest_2,RecipeName:MarLogTest,StepNo:0,SlotNo:2,LotID:lotID"); break;
                    case eSend.End: m_module.m_tcpVision.Send("ModuleID:Vision,LogType:PRC,EventID:Process,Status:End,Time:" + m_module.p_sDateTime + ",WaferID:logtest_2,RecipeName:MarLogTest,StepNo:0,SlotNo:2,LotID:lotID"); break;
                    case eSend.Reset: m_module.m_tcpVision.Send("ModuleID:Vision,LogType:Reset"); break;
                }
                return "OK";
            }
        }
        #endregion
    }
}
