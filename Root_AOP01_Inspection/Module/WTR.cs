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
        DIO_O m_doPellicleBlow;
        DIO_O m_doGlassBlow;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doPellicleBlow, this, "Pellicle Blow");
            p_sInfo = m_toolBox.Get(ref m_doGlassBlow, this, "Glass Blow");
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

        public int m_teachCleanPellicle = -1;
        public int m_teachCleanGlass = -1;
        public int m_CleanCount = -1;
        public string m_extentionlength = "0";
        public string m_CleanSpeed = "7";
        public string m_OriginSpeed = "30";
        void RunTreeClean(Tree tree)
        {
            m_teachCleanPellicle = tree.Set(m_teachCleanPellicle, m_teachCleanPellicle, "Pellicle Clean Teach", "RTR Pellicle Clean Index");
            m_teachCleanGlass = tree.Set(m_teachCleanGlass, m_teachCleanGlass, "Glass Clean Teach", "RTR Glass Clean Index");
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
            AddModuleRunList(new Run_CleanPellicle(this), false, "RTR Run Clean Pellicle Side");
            AddModuleRunList(new Run_CleanGlass(this), false, "RTR run Clean Glass Side");
        }

        public class Run_CleanPellicle : ModuleRunBase
        {
            WTR m_module;
            public Run_CleanPellicle(WTR module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_CleanPellicle run = new Run_CleanPellicle(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachPellicleClean = m_module.m_teachCleanPellicle;
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
                    m_module.m_doPellicleBlow.Write(true); //Blow On
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
                    m_module.m_doPellicleBlow.Write(false); //Blow off
                }
                return "OK";

                //if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachClean, 1, 1))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //m_module.m_doClean.Write(true);
                //if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachClean, 1))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //Thread.Sleep(1000);
                //if (m_module.Run(m_module.WriteCmd(eCmd.Retraction))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //m_module.m_doClean.Write(false); 
                //return "OK";
            }
        }
        public class Run_CleanGlass : ModuleRunBase
        {
            WTR m_module;
            public Run_CleanGlass(WTR module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_CleanGlass run = new Run_CleanGlass(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachGlassClean = m_module.m_teachCleanGlass;
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
                    m_module.m_doGlassBlow.Write(true); //Blow On
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
                    m_module.m_doGlassBlow.Write(false); //Blow off
                }
                return "OK";

                //if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachClean, 1, 1))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //m_module.m_doClean.Write(true);
                //if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachClean, 1))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //Thread.Sleep(1000);
                //if (m_module.Run(m_module.WriteCmd(eCmd.Retraction))) return p_sInfo;
                //if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                //m_module.m_doClean.Write(false); 
                //return "OK";
            }
        }

        #endregion
    }
}
