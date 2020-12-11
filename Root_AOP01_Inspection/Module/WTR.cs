﻿using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System.Threading;

namespace Root_AOP01_Inspection.Module
{
    public class WTR : WTR_RND
    {
        #region ToolBox
        DIO_O m_doClean; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doClean, this, "Cleaner");
            base.GetTools(bInit);
        }
        #endregion

        #region Arm
        protected override void InitArms(string id, IEngineer engineer)
        {
            m_dicArm.Add(eArm.Lower, new Arm(id, eArm.Lower, this, engineer, false, false));
            m_dicArm.Add(eArm.Upper, new Arm(id, eArm.Upper, this, engineer, false, false));
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeClean(tree.GetTree("Setup", false).GetTree("Teach", false).GetTree("Clean Uint",false));
        }

        public int m_teachClean = -1;
        public int m_extentionlength = 0;
        void RunTreeClean(Tree tree)
        {
            m_teachClean = tree.Set(m_teachClean, m_teachClean, "Clean Teach", "RTR Clean Index");
            m_extentionlength = tree.Set(m_extentionlength, m_extentionlength, "Extention length", "RTR Clean Extention Length");
        }
        #endregion

        public WTR(string id, IEngineer engineer) : base(id, engineer)
        {
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            base.InitModuleRuns();
            AddModuleRunList(new Run_Clean(this), false, "WTR Run Clean");
        }

        public class Run_Clean : ModuleRunBase
        {
            WTR m_module;
            public Run_Clean(WTR module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Clean run = new Run_Clean(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachClean = m_module.m_teachClean;
                if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachClean, 1, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                m_module.m_doClean.Write(true);
                if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachClean, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                Thread.Sleep(1000);
                if (m_module.Run(m_module.WriteCmd(eCmd.Retraction))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                m_module.m_doClean.Write(false); 
                return "OK";
            }
        }
        #endregion
    }
}
