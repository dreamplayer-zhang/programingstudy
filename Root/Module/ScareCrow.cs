using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Light;
using RootTools.Memory;
using RootTools.Module;
using RootTools.RADS;
using RootTools.RTC5s.LaserBright;
using RootTools.Trees;
using System.Threading;

namespace Root.Module
{
    public class ScareCrow : ModuleBase
    {
        #region ToolBox
        DIO_I m_diTest;
        NamedPipe m_namePipe;
        LightSet m_light;
        RS232 m_rs232WTR;
        RS232 m_rs232LP;
        //        Camera_Basler m_camBasler;
        MemoryPool m_memoryPool;
        Laser_Bright m_laser;
        RADSControl m_RADSControl;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_diTest, this, "Test");
            p_sInfo = m_toolBox.Get(ref m_namePipe, this, "Pipe");
            p_sInfo = m_toolBox.Get(ref m_light, this);
            p_sInfo = m_toolBox.Get(ref m_rs232WTR, this, "WTR");
            p_sInfo = m_toolBox.Get(ref m_rs232LP, this, "LP");
            //            p_sInfo = m_toolBox.Get(ref m_camBasler, this, "Basler");
            p_sInfo = m_toolBox.Get(ref m_memoryPool, this, "Memory");
            p_sInfo = m_toolBox.Get(ref m_laser, this, "Laser");
            p_sInfo = m_toolBox.Get(ref m_RADSControl, this, "RADSControl");
        }
        #endregion

        public ScareCrow(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
            //
        }

        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Test(this), false, "Desc Test");
        }

        public class Run_Test : ModuleRunBase
        {
            ScareCrow m_module;
            public Run_Test(ScareCrow module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            bool m_bTest = false;
            public override ModuleRunBase Clone()
            {
                Run_Test run = new Run_Test(m_module);
                run.m_bTest = m_bTest;
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_bTest = tree.Set(m_bTest, false, "Test", "Run Test or Not", bVisible);
            }

            public override string Run()
            {
                m_log.Info(p_id + " : Test Start");
                Thread.Sleep(2000);
                m_log.Info(p_id + " : Test End");
                return "OK";
            }
        }
    }
}
