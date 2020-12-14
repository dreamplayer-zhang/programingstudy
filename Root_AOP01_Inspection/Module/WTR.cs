using Root_EFEM.Module;
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
        DIO_O m_doTopBlow;
        DIO_O m_doBottomBlow;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doTopBlow, this, "Top Blow");
            p_sInfo = m_toolBox.Get(ref m_doBottomBlow, this, "Bottom Blow");
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

        public int m_teachCleanTop = -1;
        public int m_teachCleanBottom = -1;
        public int m_CleanCount = -1;
        public string m_extentionlength = "0";
        public string m_CleanSpeed = "7";
        public string m_OriginSpeed = "30";
        void RunTreeClean(Tree tree)
        {
            m_teachCleanTop = tree.Set(m_teachCleanTop, m_teachCleanTop, "Top Clean Teach", "RTR Top Clean Index");
            m_teachCleanBottom = tree.Set(m_teachCleanBottom, m_teachCleanBottom, "Bottom Clean Teach", "RTR Bottom Clean Index");
            m_CleanCount = tree.Set(m_CleanCount, m_CleanCount, "Clean Count", "Clean Count");
            m_extentionlength = tree.Set(m_extentionlength, m_extentionlength, "Extention length", "Clean Extention Length");
            m_CleanSpeed = tree.Set(m_CleanSpeed, m_CleanSpeed, "Clean Speed", "RTR Clean Speed");
            m_OriginSpeed = tree.Set(m_OriginSpeed, m_OriginSpeed, "Origin Speed", "RTR Origin Speed");
        }
        #endregion

        public WTR(string id, IEngineer engineer) : base(id, engineer)
        {
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            base.InitModuleRuns();
            AddModuleRunList(new Run_CleanTop(this), false, "RTR Run Clean Top Side");
            AddModuleRunList(new Run_CleanBottom(this), false, "RTR run Clean Bottom Side");
        }

        public class Run_CleanTop : ModuleRunBase
        {
            WTR m_module;
            public Run_CleanTop(WTR module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            string m_sCleanTop = "TopClean";
            public override ModuleRunBase Clone()
            {
                Run_CleanTop run = new Run_CleanTop(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sCleanTop = tree.Set(m_sCleanTop, m_sCleanTop, "TopClean", "TopClean", bVisible, true);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachPellicleClean = m_module.m_teachCleanTop;
                int nClenaCount = m_module.m_CleanCount;
                string sRMove = m_module.m_extentionlength;
                string sCleanSpeed = m_module.m_CleanSpeed;
                string sOriginSpeed = m_module.m_OriginSpeed;
                if (nClenaCount > 0)
                {
                    if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachPellicleClean, 1, 1))) return p_sInfo; //Move to Ready of Teach
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachPellicleClean, 1))) return p_sInfo; //Move to Teach
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    m_module.m_doTopBlow.Write(true); //Blow On
                    if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sCleanSpeed))) return p_sInfo; //Clean Speed Set
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    for (int i = 0; i < nClenaCount; i++)
                    {
                        if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Claen Move Front
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        sRMove = "-" + sRMove;
                        if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Clean Move Back
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    }
                    if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sOriginSpeed))) return p_sInfo; //Origin Speed Set
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    m_module.m_doTopBlow.Write(false); //Blow off
                }
                return "OK";
            }
        }
        public class Run_CleanBottom : ModuleRunBase
        {
            WTR m_module;
            public Run_CleanBottom(WTR module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            string m_sCleanGlass = "GlassClean";
            public override ModuleRunBase Clone()
            {
                Run_CleanBottom run = new Run_CleanBottom(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sCleanGlass = tree.Set(m_sCleanGlass, m_sCleanGlass, "GlassClean", "GlassClean", bVisible, true);
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachGlassClean = m_module.m_teachCleanBottom;
                int nClenaCount = m_module.m_CleanCount;
                string sRMove = m_module.m_extentionlength;
                string sCleanSpeed = m_module.m_CleanSpeed;
                string sOriginSpeed = m_module.m_OriginSpeed;
                if (nClenaCount > 0)
                {
                    if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachGlassClean, 1, 1))) return p_sInfo; //Move to Ready of Teach
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachGlassClean, 1))) return p_sInfo; //Move to Teach
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    m_module.m_doBottomBlow.Write(true); //Blow On
                    if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sCleanSpeed))) return p_sInfo; //Clean Speed Set
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    for (int i = 0; i < nClenaCount; i++)
                    {
                        if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Claen Move Front
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        sRMove = "-" + sRMove;
                        if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Clean Move Back
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    }
                    if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sOriginSpeed))) return p_sInfo; //Origin Speed Set
                    if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                    m_module.m_doBottomBlow.Write(false); //Blow off
                }
                return "OK";
            }
        }

        #endregion
    }
}
