using Root_EFEM.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Root_AOP01_Inspection.Module
{
    public class WTRCleanUnit : RTR_RND
    {
        #region ToolBox
        public DIO_I m_diExistRTR;
        DIO_O m_doTopBlow;
        DIO_O m_doBottomBlow;
        Axis m_axisZ;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doTopBlow, this, "Top Blow");
            p_sInfo = m_toolBox.Get(ref m_doBottomBlow, this, "Bottom Blow");
            p_sInfo = m_toolBox.Get(ref m_axisZ, this, "Clean Unit Z Axis");
            p_sInfo = m_toolBox.Get(ref m_diExistRTR, this, "Reticle Exist on RTR");
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
        public string m_extentionlength = "0";
        public string m_CleanSpeed = "7";
        
        void RunTreeClean(Tree tree)
        {
            m_teachCleanTop = tree.Set(m_teachCleanTop, m_teachCleanTop, "Top Clean Teach", "RTR Top Clean Index");
            m_teachCleanBottom = tree.Set(m_teachCleanBottom, m_teachCleanBottom, "Bottom Clean Teach", "RTR Bottom Clean Index");
            m_teachReticleFlip = tree.Set(m_teachReticleFlip, m_teachReticleFlip, "Vision Reticle Flip Top to Bottom", "Vision Reticle Flip Top to Bottom");
            m_extentionlength = tree.Set(m_extentionlength, m_extentionlength, "Extention length", "Clean Extention Length");
            m_CleanSpeed = tree.Set(m_CleanSpeed, m_CleanSpeed, "Clean Speed", "RTR Clean Speed");
            m_OriginSpeed = tree.Set(m_OriginSpeed, m_OriginSpeed, "Origin Speed", "RTR Origin Speed");
        }
        #endregion

        public WTRCleanUnit(string id, IEngineer engineer) : base(id, engineer)
        {
        }

        #region Flip
        public string StartFlip()
        {
            Run_Flip run = (Run_Flip)m_runFlip.Clone();
            StartRun(run); 
            return "OK"; 
        }
        #endregion

        #region ModuleRun
        ModuleRunBase m_runFlip; 
        protected override void InitModuleRuns()
        {
            base.InitModuleRuns();
            AddModuleRunList(new Run_Clean(this), true, "RTR Run Clean");
            m_runFlip = AddModuleRunList(new Run_Flip(this), true, "Vision Reticle Flip Top to Bottom");
        }

        public class Run_Clean : ModuleRunBase
        {
            WTRCleanUnit m_module;
            public Run_Clean(WTRCleanUnit module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            string m_sThickness = "3mm";
            string m_sCleanPlane = "Top";
            string m_sCleanCount = "0";
            public override ModuleRunBase Clone()
            {
                Run_Clean run = new Run_Clean(m_module);
                return run;
            }
            public List<string> m_asThicness = new List<string>() { "3mm", "5mm" };
            public List<string> m_asCleanPlane = new List<string>() { "Top", "Bottom" };
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sThickness = tree.Set(m_sThickness, m_sThickness, m_asThicness, "Reticle Thickness", "Reticle Thickness", bVisible);
                m_sCleanPlane = tree.Set(m_sCleanPlane, m_sCleanPlane, m_asCleanPlane, "Clean Plane", "Clean Plane", bVisible);
                m_sCleanCount = tree.Set(m_sCleanCount, m_sCleanCount, "Clean Count", "Clean Count", bVisible);
            }
            public override string Run()
            {
                if (EQ.p_bSimulate) return "OK";
                int teachTopClean = m_module.m_teachCleanTop;
                int nClenaCount = Int32.Parse(m_sCleanCount);
                string sRMove = m_module.m_extentionlength;
                string sCleanSpeed = m_module.m_CleanSpeed;
                string sOriginSpeed = m_module.m_OriginSpeed;
                int teachBottomClean = m_module.m_teachCleanBottom;

                if (nClenaCount > 0)
                {
                    if (m_sCleanPlane == "Top")
                    {
                        if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachTopClean, 1, 1))) return p_sInfo; //Move to Ready of Teach
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachTopClean, 1))) return p_sInfo; //Move to Teach
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        if(m_sThickness =="3mm")
                        {
                            if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, "0", "0", "2", "0", "0"))) return p_sInfo; //3mm Reticle Move
                            if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        }
                        else if(m_sThickness == "5mm")
                        {

                        }
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
                    else if (m_sCleanPlane == "Bottom")
                    {
                        if (m_module.Run(m_module.WriteCmd(eCmd.PutReady, teachBottomClean, 1, 1))) return p_sInfo; //Move to Ready of Teach
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        if (m_module.Run(m_module.WriteCmd(eCmd.Extend, teachBottomClean, 1))) return p_sInfo; //Move to Teach
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        m_module.m_doBottomBlow.Write(true); //Blow On
                        if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sCleanSpeed))) return p_sInfo; //Clean Speed Set
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        for (int i = 0; i < nClenaCount; i++)
                        {
                            if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Claen Move Front
                            if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo; //Claen Move Front
                            sRMove = "-" + sRMove;
                            if (m_module.Run(m_module.WriteCmdManualMove(eCmd.ManualMove, sRMove, "0", "0", "0", "0"))) return p_sInfo; //Clean Move Back
                            if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        }
                        if (m_module.Run(m_module.WriteCmdSetSpeed(eCmd.SetSpeed, sOriginSpeed))) return p_sInfo; //Origin Speed Set
                        if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                        m_module.m_doBottomBlow.Write(false); //Blow off
                    }
                }
                return "OK";
            }
        }

        public MainVision p_mainvision
        {
            get
            {
                AOP01_Handler handler = (AOP01_Handler)m_engineer.ClassHandler();
                return (MainVision)handler.m_mainVision;
            }
        }

        public class Run_Flip : ModuleRunBase
        {
            public WTRCleanUnit m_module;
            public Run_Flip(WTRCleanUnit module)
            {
                m_module = module;
                InitModuleRun(module);
            }
            string m_sFlip = "Flip";
            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
                m_sFlip = tree.Set(m_sFlip, m_sFlip, "Flip", "Reticle Flip Glass to Bottom", bVisible, true);
            }
            public override ModuleRunBase Clone()
            {
                Run_Flip run = new Run_Flip(m_module);
                return run;
            }
            public override string Run()
            {
                m_bDoflip = true;
                int sReticleFlip = m_module.m_teachReticleFlip;
                if (EQ.p_bSimulate) return "OK";
                while (m_module.p_mainvision.IsBusy()) Thread.Sleep(10);
                IWTRChild child = m_module.GetChild("MainVision");
                if (m_module.p_mainvision.Run(child.BeforeGet(0))) return p_sInfo;
                while (m_module.p_mainvision.IsBusy()) Thread.Sleep(10);
                int posWTR = child.GetTeachWTR(m_module.m_dicArm[0].p_infoWafer);
                if (m_module.Run(m_module.WriteCmd(eCmd.Get, posWTR, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                m_module.m_dicArm[0].p_infoWafer = child.GetInfoWafer(0);
                if (m_module.Run(m_module.WriteCmd(eCmd.Put, sReticleFlip, 1))) return p_sInfo;
                if (m_module.Run(m_module.WaitReply(m_module.m_secMotion))) return p_sInfo;
                child.SetInfoWafer(0, m_module.m_dicArm[0].p_infoWafer);
                return "OK";
            }
        }
        #endregion
    }
}
