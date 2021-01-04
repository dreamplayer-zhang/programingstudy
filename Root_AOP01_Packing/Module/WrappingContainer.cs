using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_AOP01_Packing.Module
{
    public class WrappingContainer : ModuleBase
    {
        #region ToolBox
        //DIO_I m_diPlaced;
        //DIO_I m_diPresent;
        //DIO_I m_diLoad;
        //DIO_I m_diUnload;
        //DIO_I m_diDoorOpen;
        //DIO_I m_diDocked;
        //OHT m_OHT;
        public override void GetTools(bool bInit)
        {
            //p_sInfo = m_toolBox.Get(ref m_diPlaced, this, "Place");
            //p_sInfo = m_toolBox.Get(ref m_diPresent, this, "Present");
            //p_sInfo = m_toolBox.Get(ref m_diLoad, this, "Load");
            //p_sInfo = m_toolBox.Get(ref m_diUnload, this, "Unload");
            //p_sInfo = m_toolBox.Get(ref m_diDoorOpen, this, "DoorOpen");
            //p_sInfo = m_toolBox.Get(ref m_diDocked, this, "Docked");
            //p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            //p_sInfo = m_toolBox.Get(ref m_OHT, this, p_infoCarrier, "OHT", m_diPlaced, m_diPresent);
            if (bInit)
            {
                //m_rs232.OnReceive += M_rs232_OnReceive;
                //m_rs232.p_bConnect = true;
            }
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

        public WrappingContainer(string id, IEngineer engineer)
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
        }

        public class Run_Delay : ModuleRunBase
        {
            WrappingContainer m_module;
            public Run_Delay(WrappingContainer module)
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
        #endregion
    }
}
