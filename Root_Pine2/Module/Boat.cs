using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.ComponentModel;

namespace Root_Pine2.Module
{
    public class Boat : NotifyProperty
    {
        #region Step
        public enum eStep
        {
            Init,
            Ready,
            Run,
            Done,
            RunReady,
        }
        eStep _eStep = eStep.Init;
        public eStep p_eStep
        {
            get { return _eStep; }
            set
            {
                if (_eStep == value) return;
                _eStep = value;
                OnPropertyChanged();
                if (value == eStep.RunReady) m_bgwRunReady.RunWorkerAsync();
            }
        }

        public bool IsEnableRun()
        {
            return (p_eStep == eStep.Ready) && (p_infoStrip != null) && m_bCleamDone;
        }

        public bool IsDone()
        {
            return p_eStep == eStep.Done; 
        }

        BackgroundWorker m_bgwRunReady = new BackgroundWorker();
        private void M_bgwRunReady_DoWork(object sender, DoWorkEventArgs e)
        {
            m_axis.StartMove(m_boats.p_ePosLoad);
            p_eStep = (m_axis.WaitReady() == "OK") ? eStep.Ready : eStep.Init;
        }
        #endregion

        #region Camera Offset
        public RPoint[] m_aCam2Offset = new RPoint[2] { new RPoint(), new RPoint() };
        public RPoint[] m_aCam3Offset = new RPoint[3] { new RPoint(), new RPoint(), new RPoint() };
        public void RunTreeCamOffset(Tree tree)
        {
            m_aCam2Offset[0] = tree.GetTree("2 Line").Set(m_aCam2Offset[0], m_aCam2Offset[0], "0", "Camera X Offset (um)");
            m_aCam2Offset[1] = tree.GetTree("2 Line").Set(m_aCam2Offset[1], m_aCam2Offset[1], "1", "Camera X Offset (um)");
            m_aCam3Offset[0] = tree.GetTree("3 Line").Set(m_aCam3Offset[0], m_aCam3Offset[0], "0", "Camera X Offset (um)");
            m_aCam3Offset[1] = tree.GetTree("3 Line").Set(m_aCam3Offset[1], m_aCam3Offset[1], "1", "Camera X Offset (um)");
            m_aCam3Offset[2] = tree.GetTree("3 Line").Set(m_aCam3Offset[2], m_aCam3Offset[2], "2", "Camera X Offset (um)");
        }
        #endregion

        #region ToolBox
        public Axis m_axis;
        DIO_O m_doVacuumPump;
        public DIO_Os m_doVacuum;
        public DIO_O m_doBlow;
        DIO_I2O m_dioRollerDown;
        DIO_O m_doRollerPusher;
        DIO_O m_doCleanerBlow;
        DIO_O m_doCleanerSuction;
        public DIO_O m_doTriggerSwitch; 
        public void GetTools(ToolBox toolBox, Boats boats, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, boats, p_id + ".Snap");
            toolBox.GetDIO(ref m_doVacuumPump, boats, p_id + ".Vacuum Pump");
            toolBox.GetDIO(ref m_doVacuum, boats, p_id + ".Vacuum", new string[4] { "1", "2", "3", "4" });
            toolBox.GetDIO(ref m_doBlow, boats, p_id + ".Blow");
            toolBox.GetDIO(ref m_dioRollerDown, boats, p_id + ".Roller", "Up", "Down");
            toolBox.GetDIO(ref m_doRollerPusher, boats, p_id + ".Roller Pusher");
            toolBox.GetDIO(ref m_doCleanerBlow, boats, p_id + ".Cleaner Blow");
            toolBox.GetDIO(ref m_doCleanerSuction, boats, p_id + ".Cleaner Suction");
            toolBox.GetDIO(ref m_doTriggerSwitch, boats, p_id + ".Trigger Switch");
            if (bInit) InitPosition();
        }
        #endregion

        #region Axis
        public enum ePos
        {
            Handler,
            Vision,
            SnapStart,
            CleanStart,
            CleanEnd,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            m_axis.AddSpeed("Snap");
            m_axis.AddSpeed("Clean");
        }

        public string RunMove(ePos ePos, bool bWait = true)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        double[] m_pSnap = new double[2] { 0, 0 }; 
        void CalcSnapPos(Vision2D.Recipe.Snap snapData, int yOffset)
        {
            CalcAccDist();
            double pStart = m_axis.GetPosValue(ePos.SnapStart) + m_yScale * snapData.m_dpAxis.Y;
            double pEnd = pStart + m_yScale * m_mmSnap;
            //double pEnd = m_pSnap[0] + m_yScale * m_mmSnap;
            double dpAcc = m_yScale * m_mmAcc; 
            switch (snapData.m_eDirection)
            {
                case Vision2D.Recipe.Snap.eDirection.Forward:
                    m_pSnap[0] = pStart - dpAcc + yOffset;
                    m_pSnap[1] = pEnd + dpAcc + yOffset;
                    m_axis.m_trigger.m_aPos[0] = pStart + yOffset;
                    m_axis.m_trigger.m_aPos[1] = pEnd + yOffset + 100;
                    break;
                case Vision2D.Recipe.Snap.eDirection.Backward:
                    m_pSnap[0] = pEnd + dpAcc + yOffset;
                    m_pSnap[1] = pStart - dpAcc + yOffset;
                    m_axis.m_trigger.m_aPos[0] = pStart + yOffset - 100;
                    m_axis.m_trigger.m_aPos[1] = pEnd + yOffset;
                    break;
            }
        }

        void CalcSnapPos(Vision3D.Recipe.Snap snapData, int yOffset)
        {
            CalcAccDist();
            double pStart = m_axis.GetPosValue(ePos.SnapStart) + m_yScale * snapData.m_dpAxis.Y;
            double pEnd = pStart + m_yScale * m_mmSnap;
            //double pEnd = m_pSnap[0] + m_yScale * m_mmSnap;
            double dpAcc = m_yScale * m_mmAcc;
            switch (snapData.m_eDirection)
            {
                case Vision3D.Recipe.Snap.eDirection.Forward:
                    m_pSnap[0] = pStart - dpAcc + yOffset;
                    m_pSnap[1] = pEnd + dpAcc + yOffset;
                    m_axis.m_trigger.m_aPos[0] = pStart + yOffset;
                    m_axis.m_trigger.m_aPos[1] = pEnd + yOffset + 100;
                    break;
                case Vision3D.Recipe.Snap.eDirection.Backward:
                    m_pSnap[0] = pEnd + dpAcc + yOffset;
                    m_pSnap[1] = pStart - dpAcc + yOffset;
                    m_axis.m_trigger.m_aPos[0] = pStart + yOffset - 100;
                    m_axis.m_trigger.m_aPos[1] = pEnd + yOffset;
                    break;
            }
        }

        double m_yScale = 10000;    // upulse
        double m_mmSnap = 300;
        double m_mmAcc = 20;

        public void CalcAccDist()
        {
            Axis.Speed SnapSpeed = m_axis.GetSpeedValue("Snap");
            double dVel = SnapSpeed.m_v / m_yScale;    // 최종 속도 (등속도)       [mm/s]
            double dSec = SnapSpeed.m_acc;             // 가속하는데 걸리는 시간   [s]

            double dAcc = dVel / dSec;
            m_mmAcc = 0.5 * dAcc * dSec * dSec;         // 가속하는 거리 [mm]
        }

        public string RunMoveSnapStart(Vision2D.Recipe.Snap snapData, int yOffset, bool bWait = true)
        {
            CalcSnapPos(snapData, yOffset);
            m_axis.StartMove(m_pSnap[0]);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunMoveSnapStart(Vision3D.Recipe.Snap snapData, int yOffset, bool bWait = true)
        {
            CalcSnapPos(snapData, yOffset);
            m_axis.StartMove(m_pSnap[0]);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunSnap()
        {
            try     
            {
                m_axis.SetTrigger(m_axis.m_trigger.m_aPos[0], m_axis.m_trigger.m_aPos[1], m_axis.m_trigger.m_dPos, 5, false);
                //m_axis.RunTrigger(true);
                     m_axis.StartMove(m_pSnap[1], "Snap");
                return m_axis.WaitReady();

            }
            finally { m_axis.RunTrigger(false); }
        }

        public void RunTreeAxis(Tree tree)
        {
            m_yScale = tree.Set(m_yScale, m_yScale, "Scale", "Snap Axis Scale (pulse / mm)");
            m_mmSnap = tree.Set(m_mmSnap, m_mmSnap, "Snap Length", "Snap Length (mm)");
            m_mmAcc = tree.Set(m_mmAcc, m_mmAcc, "Acc Length", "Acceleration Length (mm)"); 
        }
        #endregion

        #region Vacuum
        public void SetVacuum(bool[] aVacuum)
        {
            for (int n = 0; n < Math.Min(4, aVacuum.Length); n++) m_doVacuum.Write(n, aVacuum[n]);
        }

        public void RunVacuum(bool bOn)
        {
            m_doVacuumPump.Write(bOn);
            for (int n = 0; n < 4; n++) m_doVacuum.Write(n, bOn); 
        }

        public void RunBlow(bool bBlow)
        {
            m_doBlow.Write(bBlow);
        }
        #endregion

        #region Cleaner 
        string RunRoller(bool bDown, bool bWait = true)
        {
            m_doRollerPusher.Write(bDown);
            m_dioRollerDown.Write(bDown);
            return bWait ? m_dioRollerDown.WaitDone() : "OK";
        }

        void RunCleaner(bool bBlow, bool bSuction)
        {
            m_doCleanerBlow.Write(bBlow);
            m_doCleanerSuction.Write(bSuction);
        }

        BackgroundWorker m_bgwClean = new BackgroundWorker(); 
        public string StartClean()
        {
            if ((m_boats.p_bCleanRoller == false) && (m_boats.p_bCleanBlow == false)) return "OK";
            m_bgwClean.RunWorkerAsync(); 
            return "OK"; 
        }

        public bool m_bCleamDone = true;
        private void M_bgwClean_DoWork(object sender, DoWorkEventArgs e)
        {
            m_bCleamDone = false; 
            bool bRoller = m_boats.p_bCleanRoller;
            bool bBlow = m_boats.p_bCleanBlow; 
            try
            {
                if ((bRoller == false) && (bBlow == false)) return;
                RunMove(ePos.CleanStart);
                RunCleaner(bBlow, bBlow);
                RunRoller(bRoller);
                m_axis.StartMove(m_axis.GetPosValue(ePos.CleanEnd), "Clean");
                m_axis.WaitReady();
            }
            finally
            {
                RunCleaner(false, false);
                RunRoller(false, false);
                m_bCleamDone = true;
            }
        }
        #endregion

        #region Recipe
        public string _sRecipe = "";
        public string p_sRecipe
        {
            get { return _sRecipe; }
            set
            {
                if (_sRecipe == value) return;
                _sRecipe = value;
                if (value != "")
                {
                    m_recipe.RecipeOpen(value);
                }
            }
        }
        public dynamic m_recipe;

        public SnapInfo GetSnapInfo()
        {
            if (p_infoStrip == null)
                return new SnapInfo(m_recipe.m_eWorks, (int)m_recipe.p_eSnapMode, "", m_recipe.p_lSnap, true);

            return new SnapInfo(m_recipe.m_eWorks, (int)m_recipe.p_eSnapMode, p_infoStrip.p_id, m_recipe.p_lSnap, true);
        }
        #endregion

        #region Inspect
        public InfoStrip p_inspectStrip { get; set; }
        public string InspectDone(eVision eVision, string sStripID, string sStripResult, string sX, string sY, string sMapResult)
        {
            if (p_inspectStrip == null) return "InspectStrip id null";
            if (p_inspectStrip.p_id != sStripID) return "Strip ID MisMatch";
            string sRun = p_inspectStrip.SetResult(eVision, sStripResult, sX, sY, sMapResult); 
            p_inspectStrip = null;
            return sRun; 
        }
        #endregion

        #region Works Connect
        bool _bWorksConnect = false; 
        public bool p_bWorksConnect
        {
            get { return _bWorksConnect; }
            set
            {
                _bWorksConnect = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        public void Reset(ModuleBase.eState eState)
        {
            p_infoStrip = null;
            p_inspectStrip = null; 
            m_doTriggerSwitch.Write(false);
            if (eState == ModuleBase.eState.Ready) p_eStep = eStep.RunReady;
            RunVacuum(false); 
        }

        InfoStrip _infoStrip = null;
        public InfoStrip p_infoStrip 
        { 
            get { return _infoStrip; }
            set
            {
                _infoStrip = value;
                OnPropertyChanged(); 
            }
        }
        public string p_id { get; set; }
        Boats m_boats;
        public Boat(string id, Boats boats, eWorks eWorks, bool b3D)
        {
            m_bgwRunReady.DoWork += M_bgwRunReady_DoWork;
            m_bgwClean.DoWork += M_bgwClean_DoWork;
            p_id = id;
            m_boats = boats;
            if (b3D) m_recipe = new Vision3D.Recipe((Vision3D)boats.m_vision, eWorks);
            else m_recipe = new Vision2D.Recipe((Vision2D)boats.m_vision, eWorks);
        }

        public void RunThreadStop()
        {
            RunMove(ePos.Handler); 
        }
    }
}
