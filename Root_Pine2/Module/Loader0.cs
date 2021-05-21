using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_Pine2.Module
{
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXY;
        Axis m_axisZ;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXY, this, "Loader0");
            m_toolBox.GetAxis(ref m_axisZ, this, "Z");
            m_picker.GetTools(m_toolBox, this, bInit); 
            if (bInit) InitPosition();
        }

        public enum ePosLoad
        {
            LoadEV,
            Transfer0,
            Transfer1,
            Transfer2,
            Transfer3,
            Transfer4,
            Transfer5,
            Transfer6,
            Transfer7,
        }
        const string c_sPaperTray = "PaperTray"; 
        void InitPosition()
        {
            m_axisXY.AddPos(Enum.GetNames(typeof(ePosLoad)));
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosLoad)));
            InitPosition(GetPosString(Vision.eVision.Top3D, Vision.eVisionWorks.A));
            InitPosition(GetPosString(Vision.eVision.Top3D, Vision.eVisionWorks.B));
            InitPosition(GetPosString(Vision.eVision.Top2D, Vision.eVisionWorks.A));
            InitPosition(GetPosString(Vision.eVision.Top2D, Vision.eVisionWorks.B));
            InitPosition(c_sPaperTray); 
        }
        void InitPosition(string sPos)
        {
            m_axisXY.AddPos(sPos);
            m_axisZ.AddPos(sPos); 
        }

        string GetPosString(Vision.eVision eVision, Vision.eVisionWorks eVisionWorks)
        {
            return eVision.ToString() + eVisionWorks.ToString(); 
        }
        #endregion

        #region AxisXY
        public string RunMoveXY(ePosLoad ePos, bool bWait = true)
        {
            m_axisXY.StartMove(ePos);
            return bWait ? m_axisXY.WaitReady() : "OK";
        }

        public string RunMoveXY(Vision.eVision eVision, Vision.eVisionWorks eVisionWorks, bool bWait = true)
        {
            m_axisXY.StartMove(GetPosString(eVision, eVisionWorks));
            return bWait ? m_axisXY.WaitReady() : "OK";
        }

        public string RunMoveXYPaper(bool bWait = true)
        {
            m_axisXY.StartMove(c_sPaperTray);
            return bWait ? m_axisXY.WaitReady() : "OK";
        }

        int m_pulsemm = 1000; 
        public string RunMoveLoadEV(bool bWait = true)
        {
            double dPos = m_pulsemm * (77 - m_pine2.p_widthStrip);
            m_axisXY.StartMove(ePosLoad.LoadEV, new RPoint(dPos, 0));
            return bWait ? m_axisXY.WaitReady() : "OK"; 
        }

        void RunTreeAxisXY(Tree tree)
        {
            m_pulsemm = tree.Set(m_pulsemm, m_pulsemm, "Pulse / mm", "Axis X (Pulse / mm)");
        }
        #endregion

        #region AxisZ
        public string RunMoveZ(ePosLoad ePos, bool bWait = true)
        {
            m_axisZ.StartMove(ePos);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eVision eVision, Vision.eVisionWorks eVisionWorks, bool bWait = true)
        {
            m_axisZ.StartMove(GetPosString(eVision, eVisionWorks));
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunMoveZPaper(bool bWait = true)
        {
            m_axisZ.StartMove(c_sPaperTray);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axisZ.StartMove(0);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunShakeUp(int nShake, int dzPulse)
        {
            int iShake = 0; 
            while (iShake < nShake)
            {
                if (Run(RunShakeUp(-dzPulse))) return p_sInfo; 
                if (Run(RunShakeUp(0.9 * dzPulse))) return p_sInfo;
                iShake++;
            }
            return RunMoveUp(); 
        }

        string RunShakeUp(double dzPulse)
        {
            m_axisZ.StartShift(dzPulse);
            return m_axisZ.WaitReady(); 
        }
        #endregion

        #region RunLoad
        public string RunLoad(ePosLoad ePos, int nShake, int dzPulse)
        {
            if (Run(RunMoveUp())) return p_sInfo; 
            if (ePos == ePosLoad.LoadEV)
            {
                if (Run(RunMoveLoadEV())) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(true))) return p_sInfo; 
                if (Run(RunShakeUp(nShake, dzPulse))) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo; 
                //m_picker.p_infoStrip = 
            }
            else
            {
                if (Run(RunMoveXY(ePos))) return p_sInfo; 
            }
            return "OK";
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_picker.RunTreeVacuum(tree.GetTree("Vacuum"));
            RunTreeAxisXY(tree.GetTree("Axis")); 
        }
        #endregion

        Picker m_picker = null;
        LoadEV m_loadEV = null; 
        Pine2 m_pine2 = null; 
        public Loader0(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_pine2 = handler.m_pine2;
            m_loadEV = handler.m_loadEV; 
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop(); 
            base.ThreadStop();
        }
    }
}
